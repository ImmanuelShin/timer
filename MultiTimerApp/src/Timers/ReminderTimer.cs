using System;

namespace MultiTimerApp.Timers
{
    public class ReminderTimer : TimerBase
    {
        public long DurationTicks { get; set; }  // stored for JSON
        public DateTime? EndTime { get; set; }
        public bool IsRunning { get; set; }

        public void Start(TimeSpan duration)
        {
            DurationTicks = duration.Ticks;
            EndTime = DateTime.Now + duration;
            IsRunning = true;
        }

        public void Stop()
        {
            EndTime = null;
            IsRunning = false;
        }

        public TimeSpan GetRemaining()
        {
            if (IsRunning && EndTime.HasValue)
            {
                var remaining = EndTime.Value - DateTime.Now;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
            return TimeSpan.Zero;
        }

        public bool IsExpired => IsRunning && EndTime.HasValue && DateTime.Now >= EndTime.Value;

        public override TimeSpan GetElapsed()
        {
            return TimeSpan.FromTicks(DurationTicks) - GetRemaining();
        }
    }
}
