using System;

namespace MultiTimerApp.Timers
{
    public abstract class TimerBase
    {
        public string Name { get; set; } = "Unnamed";
        public Guid Id { get; set; } = Guid.NewGuid();

        public abstract TimeSpan GetElapsed();
    }
}
