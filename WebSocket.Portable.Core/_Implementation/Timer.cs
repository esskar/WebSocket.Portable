using System.Threading.Tasks;

namespace System.Threading
{
    public delegate void TimerCallback(object state);

    public sealed class Timer 
    {
		TimerCallback Callback { get; set; }
		object State { get; set; }
		int Period { get; set; }
		bool IsRunning { get; set; }
        TimerInternal Internal;

        public Timer(TimerCallback callback, int period, object state = null)
        {
            Callback = callback;
            State = state;
            Period = period;
            Start();
        }

        public void Start()
        {
            if (IsRunning)
                Stop();
            Internal = new TimerInternal(Callback, State, Period);
            IsRunning = true;
        }

        public void Stop()
		{	
			if(IsRunning)
				Internal.Dispose();
            Internal = null;
			IsRunning = false;
        }
    }

	internal sealed class TimerInternal : CancellationTokenSource, IDisposable
	{

        internal TimerInternal(TimerCallback callback, object state, int period)
		{
            Task.Delay(period, Token).ContinueWith((t, s) =>
				{
					var tuple = (Tuple<TimerCallback, object>)s;
					tuple.Item1(tuple.Item2);
				}, Tuple.Create(callback, state), CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
				TaskScheduler.Default);
		}

		protected override void Dispose (bool disposing)
		{
			Cancel();
			base.Dispose (disposing);
		}
	}
}
