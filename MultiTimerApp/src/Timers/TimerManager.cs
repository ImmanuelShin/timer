using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MultiTimerApp.Timers;

namespace MultiTimerApp
{
    public class TimerManager
    {
        private readonly string _dataFile = "timers.json";
        private readonly List<TimerBase> _timers = new List<TimerBase>();

        public IEnumerable<TimerBase> Timers => _timers;

        public void AddTimer(TimerBase timer) => _timers.Add(timer);

        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new TimerJsonConverter() }
            };

            var json = JsonSerializer.Serialize(_timers, options);
            File.WriteAllText(_dataFile, json);
        }

        public void Load()
        {
            if (!File.Exists(_dataFile)) return;

            var options = new JsonSerializerOptions
            {
                Converters = { new TimerJsonConverter() }
            };

            var json = File.ReadAllText(_dataFile);
            var loaded = JsonSerializer.Deserialize<List<TimerBase>>(json, options);

            if (loaded != null)
            {
                _timers.Clear();
                _timers.AddRange(loaded);
            }
        }
        
    }
}
