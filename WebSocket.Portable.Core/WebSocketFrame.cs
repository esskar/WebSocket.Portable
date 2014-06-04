using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Internal;
using WebSocket.Portable.Resources;

namespace WebSocket.Portable
{
    internal class WebSocketFrame : IWebSocketFrame
    {
        /// <summary>
        /// Gets a value indicating whether this frame is a control frame.
        /// </summary>
        /// <value>
        /// <c>true</c> if this frame is a control frame; otherwise, <c>false</c>.
        /// </value>
        public bool IsControlFrame
        {
            get { return this.Opcode.IsControl(); }            
        }

        /// <summary>
        /// Gets a value indicating whether this frame is a data frame.
        /// </summary>
        /// <value>
        /// <c>true</c> if this frame is a data frame; otherwise, <c>false</c>.
        /// </value>
        public bool IsDataFrame
        {
            get { return this.Opcode.IsData(); }
        }

        public bool IsFin { get; set; }

        public bool IsMasked { get; set; }

        public bool IsRsv1 { get; set; }

        public bool IsRsv2 { get; set; }

        public bool IsRsv3 { get; set; }

        /// <summary>
        /// Gets the masking key.
        /// </summary>
        /// <value>
        /// The masking key.
        /// </value>
        public byte[] MaskingKey { get; set; }

        /// <summary>
        /// Gets the opcode.
        /// </summary>
        /// <value>
        /// The opcode.
        /// </value>
        public WebSocketOpcode Opcode { get; set; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        public byte[] Payload { get; set; }

        public async Task ReadFromAsync(IDataLayer layer, CancellationToken cancellationToken)
        {
            var headerBytes = await layer.ReadAsync(2, cancellationToken);

            this.IsFin = (headerBytes[0] & 0x80) == 0x80;
            this.IsRsv1 = (headerBytes[0] & 0x40) == 0x40;
            this.IsRsv2 = (headerBytes[0] & 0x20) == 0x20;
            this.IsRsv3 = (headerBytes[0] & 0x10) == 0x10;
            this.Opcode = (WebSocketOpcode) (headerBytes[0] & 0x0f);

            if (this.IsControlFrame && this.IsFin)
                throw new WebSocketException(WebSocketErrorCode.CloseInvalidData, ErrorMessages.FragmentedControlFrame);

            if (!this.IsDataFrame && this.IsRsv1)
                throw new WebSocketException(WebSocketErrorCode.CloseInvalidData, ErrorMessages.CompressedNonDataFrame);

            this.IsMasked = (headerBytes[1] & 0x80) == 0x80;
            if (this.IsMasked)
                this.MaskingKey = await layer.ReadAsync(4, cancellationToken);
            else
                this.MaskingKey = null;

            ulong payloadLength = (byte)(headerBytes[1] & 0x7f);            
            if (this.IsControlFrame && payloadLength > 125)
                throw new WebSocketException(WebSocketErrorCode.CloseInconstistentData, ErrorMessages.PayloadLengthControlFrame);

            var extendedPayloadLengthSize = payloadLength < 126 ? 0 : payloadLength < 127 ? 2 : 8;
            if (extendedPayloadLengthSize > 0)
            {
                var extendedPayloadLength = await layer.ReadAsync(extendedPayloadLengthSize, cancellationToken);
                payloadLength = extendedPayloadLengthSize == 2
                    ? extendedPayloadLength.ToUInt16(ByteOrder.BigEndian)
                    : extendedPayloadLength.ToUInt64(ByteOrder.BigEndian);

                if (payloadLength > int.MaxValue)
                    throw new WebSocketException(WebSocketErrorCode.CloseMessageTooBig);
            }

            if (payloadLength > 0)
            {
                this.Payload = await layer.ReadAsync((int) payloadLength, cancellationToken);
                if (this.IsMasked)
                    this.MaskPayload(this.Payload, 0, this.Payload.Length, this.Payload, 0);
            }
            else
            {
                this.Payload = null;
            }
        }

        public async Task WriteToAsync(IDataLayer layer, CancellationToken cancellationToken)
        {
            int payloadLength;
            byte[] extendedPayloadLength;

            var payload = this.Payload ?? new byte[0];
            if (payload.Length < 126)
            {
                payloadLength = payload.Length;
                extendedPayloadLength = null;
            }
            // Extended payload (16bit)
            else if (payload.Length < 65536)
            {
                payloadLength = 126;
                extendedPayloadLength = ((ushort)payload.Length).ToByteArray(ByteOrder.BigEndian);
            }
            // Extended payload (64bit)
            else
            {
                payloadLength = 127;
                extendedPayloadLength = payload.LongCount().ToByteArray(ByteOrder.BigEndian);
            }

            var header = this.IsFin.ToBit();
            header = (header << 1) + this.IsRsv1.ToBit();
            header = (header << 1) + this.IsRsv2.ToBit();
            header = (header << 1) + this.IsRsv3.ToBit();
            header = (header << 4) + (int)this.Opcode;
            header = (header << 1) + this.IsMasked.ToBit();
            header = (header << 7) + payloadLength;

            var headerBytes = ((ushort)header).ToByteArray(ByteOrder.BigEndian);
            await layer.WriteAsync(headerBytes, 0, headerBytes.Length, cancellationToken);

            if (extendedPayloadLength != null)
                await layer.WriteAsync(extendedPayloadLength, 0, extendedPayloadLength.Length, cancellationToken);

            if (this.IsMasked)
            {
                await layer.WriteAsync(this.MaskingKey, 0, this.MaskingKey.Length, cancellationToken);

                // try to keep the memory footprint to a minimum
                var offset = 0;
                var buffer = new byte[Math.Min(payload.Length, 1024 * this.MaskingKey.Length)];
                while (offset < payload.Length)
                {
                    var count = Math.Min(buffer.Length, payload.Length - offset);
                    this.MaskPayload(payload, offset, count, buffer, 0);
                    await layer.WriteAsync(buffer, 0, count, cancellationToken);
                    offset += count;
                }
            }
            else
            {
                await layer.WriteAsync(payload, 0, payload.Length, cancellationToken);                    
            }
        }

        private void MaskPayload(IList<byte> input, int inputOffset, int inputLength, IList<byte> output, int outputOffset)
        {
            for (var i = 0; i < inputLength; i++)
                output[i + outputOffset] = (byte)(input[i + inputOffset] ^ this.MaskingKey[i % this.MaskingKey.Length]);
        }
    }
}
