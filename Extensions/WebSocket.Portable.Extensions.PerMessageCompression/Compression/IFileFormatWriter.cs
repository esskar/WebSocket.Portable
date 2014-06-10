namespace WebSocket.Portable.Extensions.Compression.Compression
{
    internal interface IFileFormatWriter
    {
        byte[] GetHeader();
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
        byte[] GetFooter();
    }
}
