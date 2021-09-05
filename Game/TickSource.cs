using System;
using System.Threading;
using DigBuild.Engine.Ticking;

namespace DigBuild
{
    /// <summary>
    /// A stable tick source running at 20 ticks per second.
    /// </summary>
    public sealed class TickSource : IStableTickSource
    {
        /// <summary>
        /// The amount of ticks per second.
        /// </summary>
        public const uint TicksPerSecond = 20;
        /// <summary>
        /// The tick duration in seconds.
        /// </summary>
        public const float TickDurationSeconds = 1f / TicksPerSecond;
        /// <summary>
        /// The tick duration in milliseconds.
        /// </summary>
        public const float TickDurationMilliseconds = 1000f / TicksPerSecond;
        /// <summary>
        /// The number of system ticks per game tick.
        /// </summary>
        public const long SystemTicksPerGameTick = TimeSpan.TicksPerSecond / TicksPerSecond;

        private Thread? _thread;
        private Interpolator _interpolator;
        private bool _shouldStop;

        /// <summary>
        /// Fired every tick while the tick source is not paused.
        /// </summary>
        public event Action? Tick;
        /// <summary>
        /// Fired every tick even if the tick source is paused.
        /// </summary>
        public event Action? HighPriorityTick;

        public IInterpolator CurrentTick => _interpolator;

        /// <summary>
        /// Whether the tick source is running or not.
        /// </summary>
        public bool Running => _thread != null;

        /// <summary>
        /// Whether the tick source is paused or not.
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// Starts the tick source.
        /// </summary>
        public void Start()
        {
            if (Running)
                throw new InvalidOperationException("Tick source is already running.");

            _shouldStop = false;
            _thread = new Thread(Run) { Name = "Ticking Thread" };
            _thread.Start();
        }

        private void Run()
        {
            while (!_shouldStop)
            {
                long elapsed;
                lock (this)
                {
                    var start = DateTime.Now.Ticks;
                    if (!Paused)
                        _interpolator = new Interpolator(start);
                    HighPriorityTick?.Invoke();
                    if (!Paused)
                        Tick?.Invoke();
                    elapsed = DateTime.Now.Ticks - start;
                }

                var remainder = SystemTicksPerGameTick - elapsed;
                if (remainder > 0) Thread.Sleep(new TimeSpan(remainder));
            }

            _thread = null;
        }

        /// <summary>
        /// Notifies the tick source that it should stop.
        /// </summary>
        public void Stop()
        {
            if (!Running)
                throw new InvalidOperationException("Tick source is not running.");

            _shouldStop = true;
        }

        /// <summary>
        /// Waits for the tick source to stop.
        /// </summary>
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