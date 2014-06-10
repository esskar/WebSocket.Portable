namespace WebSocket.Portable.Extensions.Compression.Compression
{
    internal class Match
    {
        public MatchState State { get; set; }

        public int Position { get; set; }

        public int Length { get; set; }

        public byte Symbol { get; set; }
    }
}
