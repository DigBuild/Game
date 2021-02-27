using System;
using System.Threading;
using System.Threading.Tasks;

namespace DigBuild
{
    public class TickManager
    {
        public const int TicksPerSecond = 20;
        public const float TickDurationSeconds = 1f / TicksPerSecond;
        public const float TickDurationMilliseconds = 1000f / TicksPerSecond;
        private const long SystemTicksPerGameTick = TimeSpan.TicksPerSecond / TicksPerSecond;

        private readonly Action _tickFunction;

        private long _lastTick;

        public TickManager(Action tickFunction)
        {
            _tickFunction = tickFunction;
        }

        public float PartialTick => (float) Math.Min((DateTime.Now.Ticks - _lastTick) / (double) SystemTicksPerGameTick, 1);

        public void Start(Task windowClosed)
        {
            while (!windowClosed.IsCompleted)
            {
                long elapsed;
                lock (this)
                {
                    var start = _lastTick = DateTime.Now.Ticks;
                    _tickFunction();
                    elapsed = DateTime.Now.Ticks - start;
                }
                var remainder = SystemTicksPerGameTick - elapsed;
                if (remainder > 0)
                    Thread.Sleep(new TimeSpan(remainder));
            }
        }
    }
}