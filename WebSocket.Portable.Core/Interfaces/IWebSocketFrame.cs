namespace WebSocket.Portable.Interfaces
{
    public interface IWebSocketFrame
    {
        /// <summary>
        /// Gets a value indicating whether this frame is a control frame.
        /// </summary>
        /// <value>
        /// <c>true</c> if this frame is a control frame; otherwise, <c>false</c>.
        /// </value>
        bool IsControlFrame { get; }

        /// <summary>
        /// Gets a value indicating whether this frame is a data frame.
        /// </summary>
        /// <value>
        /// <c>true</c> if this frame is a data frame; otherwise, <c>false</c>.
        /// </value>
        bool IsDataFrame { get; }

        bool IsFin { get; }

        bool IsMasked { get; }

        bool IsRsv1 { get; }

        bool IsRsv2 { get; }

        bool IsRsv3 { get; }

        /// <summary>
        /// Gets the masking key.
        /// </summary>
        /// <value>
        /// The masking key.
        /// </value>
        byte[] MaskingKey { get; }

        /// <summary>
        /// Gets the opcode.
        /// </summary>
        /// <value>
        /// The opcode.
        /// </value>
        WebSocketOpcode Opcode { get; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        byte[] Payload { get; }        
    }
}