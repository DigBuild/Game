using System;
using System.Threading;
using System.Threading.Tasks;
using DigBuild.Engine.Ticking;

namespace DigBuild
{
    public class TickSource : IStableTickSource
    {
        public const uint TicksPerSecond = 20;
        public const float TickDurationSeconds = 1f / TicksPerSecond;
        public const float TickDurationMilliseconds = 1000f / TicksPerSecond;
        private const long SystemTicksPerGameTick = TimeSpan.TicksPerSecond / TicksPerSecond;
        
        private Interpolator _interpolator;
        private bool _stop;

        public event Action? Tick;

        public IInterpolator CurrentTick => _interpolator;
        
        public void Start()
        {
            while (!_stop)
            {
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
        }

        public void Stop()
        {
            _stop = true;
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