using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.Portable.Tests.Internal
{
    /// <summary>
    /// Provides a pump that supports running asynchronous methods on the current thread.
    /// </summary>
    /// <remarks>
    /// The implementation is based on http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
    /// </remarks>
    public static class AsyncPump
    {
        /// <summary>
        /// Runs the specified asynchronous method.
        /// </summary>
        /// <param name="asyncMethod">The asynchronous method to execute.</param>
        /// <exception cref="System.ArgumentNullException">asyncMethod</exception>
        public static void Run(Action asyncMethod)
        {
            if (asyncMethod == null) 
                throw new ArgumentNullException("asyncMethod");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSynchronizationContext(true);
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function
                syncCtx.OperationStarted();
                asyncMethod();
                syncCtx.OperationCompleted();

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        /// <summary>
        /// Runs the specified asynchronous method.
        /// </summary>
        /// <param name="asyncMethod">The asynchronous method to execute.</param>
        /// <exception cref="System.ArgumentNullException">asyncMethod</exception>
        /// <exception cref="System.InvalidOperationException">No task provided.</exception>
        public static void Run(Func<Task> asyncMethod)
        {
            if (asyncMethod == null) 
                throw new ArgumentNullException("asyncMethod");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSynchronizationContext(false);
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function and alert the context to when it completes
                var t = asyncMethod();
                if (t == null) 
                    throw new InvalidOperationException("No task provided.");

                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();
                t.Wait();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        public static TResult RunWithResult<TResult>(Func<Task<TResult>> asyncMethod)
        {
            if (asyncMethod == null)
                throw new ArgumentNullException("asyncMethod");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSynchronizationContext(false);
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function and alert the context to when it completes
                var t = asyncMethod();
                if (t == null)
                    throw new InvalidOperationException("No task provided.");

                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();
                return t.Result;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        /// <summary>
        /// Runs the specified asynchronous method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncMethod">The asynchronous method to execute.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">asyncMethod</exception>
        /// <exception cref="System.InvalidOperationException">No task provided.</exception>
        public static T Run<T>(Func<Task<T>> asyncMethod)
        {
            if (asyncMethod == null) 
                throw new ArgumentNullException("asyncMethod");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSynchronizationContext(false);
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function and alert the context to when it completes
                var t = asyncMethod();
                if (t == null) 
                    throw new InvalidOperationException("No task provided.");

                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();

                return t.Result;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        /// <summary>
        /// Provides a SynchronizationContext that's single-threaded.
        /// </summary>
        private sealed class SingleThreadSynchronizationContext : SynchronizationContext
        {
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue;
            private int _operationCount;
            private readonly bool _trackOperations;

            /// <summary>
            /// Prevents a default instance of the <see cref="SingleThreadSynchronizationContext"/> class from being created.
            /// </summary>
            private SingleThreadSynchronizationContext()
            {
                _queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SingleThreadSynchronizationContext"/> class.
            /// </summary>
            /// <param name="trackOperations">if set to <c>true</c> [track operations].</param>
            public SingleThreadSynchronizationContext(bool trackOperations)
                : this()
            {
                _trackOperations = trackOperations;
            }

            /// <summary>
            /// When overridden in a derived class, dispatches an asynchronous message to a synchronization context.
            /// </summary>
            /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            /// <exception cref="System.ArgumentNullException">d</exception>
            public override void Post(SendOrPostCallback d, object state)
            {
                if (d == null) 
                    throw new ArgumentNullException("d");

                _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            }

            /// <summary>
            /// When overridden in a derived class, dispatches a synchronous message to a synchronization context.
            /// </summary>
            /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            /// <exception cref="System.NotSupportedException">Synchronously sending is not supported.</exception>
            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Synchronously sending is not supported.");
            }

            /// <summary>
            /// Run queued items on current thread.
            /// </summary>
            public void RunOnCurrentThread()
            {
                foreach (var workItem in _queue.GetConsumingEnumerable())
                {
                    workItem.Key(workItem.Value);
                }
            }

            /// <summary>
            /// Completes this instance.
            /// </summary>
            public void Complete()
            {
                _queue.CompleteAdding();
            }

            /// <summary>
            /// When overridden in a derived class, responds to the notification that an operation has started.
            /// </summary>
            public override void OperationStarted()
            {
                if (_trackOperations)
                    Interlocked.Increment(ref _operationCount);
            }

            /// <summary>
            /// When overridden in a derived class, responds to the notification that an operation has completed.
            /// </summary>
            public override void OperationCompleted()
            {
                if (_trackOperations && Interlocked.Decrement(ref _operationCount) == 0)
                    Complete();
            }
        }
    }
}
