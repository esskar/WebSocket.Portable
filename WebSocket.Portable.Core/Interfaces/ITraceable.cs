namespace WebSocket.Portable.Interfaces
{
    public interface ITraceable
    {
        ITracer Tracer { get; set; }
    }
}
