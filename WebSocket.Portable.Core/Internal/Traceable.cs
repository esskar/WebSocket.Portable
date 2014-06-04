using System;
using WebSocket.Portable.Interfaces;

namespace WebSocket.Portable.Internal
{
    public abstract class Traceable : ITraceable
    {
        private ITracer _tracer;
        private IDisposable _disposable;

        public ITracer Tracer
        {
            get { return _tracer; }
            set
            {
                if (_disposable != null)
                    _disposable.Dispose();
                _tracer = value;
                if (_tracer != null)
                    _disposable = LogManager.Instance.AddReceiver(_tracer.Trace);
            }
        }
    }
}
