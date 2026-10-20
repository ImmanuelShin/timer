using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MultiTimerApp.Timers
{
    public class StreakTracker : TimerBase
    {
        public DateTime? StartTime { get; set; }  // Changed from private set
        public TimeSpan LongestStreak { get; set; }  // Changed from private set
        public List<DateTime> BreakHistory { get; set; } = new();  // Changed from private set
        public bool IsRunning { get; set; }  // Changed from private set
        

        [JsonIgnore]
        public bool HasUnsavedChanges { get; set; }

        // Add a property for JSON serialization of LongestStreak
        [JsonIgnore]
        public long LongestStreakTicks
        {
            get => LongestStreak.Ticks;
            set => LongestStreak = TimeSpan.FromTicks(value);
        }

        public void Start()
        {
            if (!IsRunning)
            {
                StartTime = DateTime.Now;
                IsRunning = true;
                HasUnsavedChanges = true;
            }
        }

        public void BreakStreak()
        {
            if (IsRunning && StartTime.HasValue)
            {
                var currentDuration = DateTime.Now - StartTime.Value;
                if (currentDuration > LongestStreak)
                    LongestStreak = currentDuration;

                BreakHistory.Add(DateTime.Now);

                StartTime = null;
                IsRunning = false;
                HasUnsavedChanges = true;
            }
        }

        public void Restart()
        {
            BreakStreak();
            Start();
        }

        public override TimeSpan GetElapsed()
        {
            if (StartTime.HasValue)
                return DateTime.Now - StartTime.Value;
            return TimeSpan.Zero;
        }

        public TimeSpan GetLongestStreak() => LongestStreak;

        public override string ToString()
        {
            return $"{Name}: {GetElapsed():hh\\:mm\\:ss}";
        }
    }
}