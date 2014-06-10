using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.Portable.Tasks
{
    public class AsyncLock
    {
        private readonly AsyncSemaphore _semaphore;
        private readonly Task<IDisposable> _releaser;
        private readonly TaskScheduler _taskScheduler;

        public AsyncLock()
            : this(TaskScheduler.Default) { }

        public AsyncLock(TaskScheduler taskScheduler)
        {
            if (taskScheduler == null)
                throw new ArgumentNullException("taskScheduler");

            _semaphore = new AsyncSemaphore(1);
            _releaser = TaskAsyncHelper.FromResult<IDisposable>(new Releaser(this));
            _taskScheduler = taskScheduler;
        }

        public Task<IDisposable> LockAsync()
        {
            return this.LockAsync(CancellationToken.None);
        }

        public Task<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            var wait = _semaphore.WaitAsync();
            if (wait.IsCompleted)
                return _releaser;

            return wait.ContinueWith(
                _ => (IDisposable) new Releaser(this),
                cancellationToken,
                TaskContinuationOptions.ExecuteSynchronously,
                _taskScheduler);
        }

        private void Release()
        {
            _semaphore.Release();
        }

        struct Releaser : IDisposable
        {
            private readonly AsyncLock _asyncLock;

            internal Releaser(AsyncLock asyncLock)
            {
                _asyncLock = asyncLock;
            }

            public void Dispose()
            {
                if (_asyncLock != null)
                    _asyncLock.Release();
            }
        }
    }
}
