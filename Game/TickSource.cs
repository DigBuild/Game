using System;
using System.Threading;
using DigBuild.Engine.Ticking;

namespace DigBuild
{
    public sealed class TickSource : IStableTickSource
    {
        public const uint TicksPerSecond = 20;
        public const float TickDurationSeconds = 1f / TicksPerSecond;
        public const float TickDurationMilliseconds = 1000f / TicksPerSecond;
        private const long SystemTicksPerGameTick = TimeSpan.TicksPerSecond / TicksPerSecond;
        private static readonly TimeSpan TickTimeSpan = new(SystemTicksPerGameTick);

        private Thread? _thread;
        private Interpolator _interpolator;
        private bool _shouldStop;

        public event Action? Tick;
        public IInterpolator CurrentTick => _interpolator;
        public bool Running => _thread != null;

        public bool Paused { get; set; }

        public void Start()
        {
            if (Running)
                throw new InvalidOperationException("Tick source is already running.");

            _shouldStop = false;
            _thread = new Thread(() =>
            {
                while (!_shouldStop)
                {
                    while (Paused)
                        Thread.Sleep(TickTimeSpan);

                    long elapsed;
                    lock (this)
                    {
                        var start = DateTime.Now.Ticks;
                        _interpolator = new Interpolator(start);
                        Tick?.Invoke();
                        elapsed = DateTime.Now.Ticks - start;
                    }
                    var remainder = SystemTicksPerGameTick - elapsed;
                    if (remainder > 0)
                        Thread.Sleep(new TimeSpan(remainder));
                }

                _thread = null;
            }) { Name = "Ticking Thread" };
            _thread.Start();
        }

        public void Stop()
        {
            if (!Running)
                throw new InvalidOperationException("Tick source is not running.");

            _shouldStop = true;
        }

        public void Await()
        {
            _thread?.Join();
        }

        private readonly struct Interpolator : IInterpolator
        {
            private readonly long _tickStart;

            public float Value => (float) Math.Min((DateTime.Now.Ticks - _tickStart) / (double) SystemTicksPerGameTick, 1);

            public Interpolator(long tickStart)
            {
                _tickStart = tickStart;
            }
        }
    }
}