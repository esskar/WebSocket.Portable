using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebSocket.Portable.Tasks
{
    public class AsyncSemaphore
    {
        private readonly Queue<TaskCompletionSource<bool>> _waits;
        private int _count;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException("initialCount");
            _count = initialCount;
            _waits = new Queue<TaskCompletionSource<bool>>();
        }

        public Task WaitAsync()
        {
            lock (_waits)
            {
                if (_count > 0)
                {
                    --_count;
                    return TaskAsyncHelper.True;
                }
                var tcs = new TaskCompletionSource<bool>();
                _waits.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> release = null;
            lock (_waits)
            {
                if (_waits.Count > 0)
                    release = _waits.Dequeue();
                else
                    ++_count;
            }
            if (release != null)
                release.SetResult(true);
        }
    }
}
