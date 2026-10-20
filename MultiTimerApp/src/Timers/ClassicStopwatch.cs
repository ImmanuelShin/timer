using System;

namespace MultiTimerApp.Timers
{
    public class ClassicStopwatch : TimerBase
    {
        public long ElapsedTicks { get; set; }
        public DateTime? StartTime { get; set; }
        public bool IsRunning { get; set; }

        public void Start()
        {
            if (!IsRunning)
            {
                StartTime = DateTime.Now;
                IsRunning = true;
            }
        }

        public void Stop()
        {
            if (IsRunning && StartTime.HasValue)
            {
                ElapsedTicks += (DateTime.Now - StartTime.Value).Ticks;
                StartTime = null;
                IsRunning = false;
            }
        }

        public void Reset()
        {
            ElapsedTicks = 0;
            StartTime = null;
            IsRunning = false;
        }

        public override TimeSpan GetElapsed()
        {
            if (IsRunning && StartTime.HasValue)
                return TimeSpan.FromTicks(ElapsedTicks + (DateTime.Now - StartTime.Value).Ticks);

            return TimeSpan.FromTicks(ElapsedTicks);
        }
    }
}
