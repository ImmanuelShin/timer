using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiTimerApp.Timers;
using System.Collections.Generic;

namespace MultiTimerApp
{
    public class TimerJsonConverter : JsonConverter<TimerBase>
    {
        public override TimerBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                
                if (!root.TryGetProperty("$type", out var typeProperty))
                    throw new JsonException("Missing $type discriminator");

                string typeName = typeProperty.GetString() ?? throw new JsonException("Invalid $type value");

                // Create new options without this converter to avoid infinite recursion
                var newOptions = new JsonSerializerOptions();
                foreach (var converter in options.Converters)
                {
                    if (converter is not TimerJsonConverter)
                        newOptions.Converters.Add(converter);
                }

                return typeName switch
                {
                    "ClassicStopwatch" => DeserializeClassicStopwatch(root),
                    "ReminderTimer" => DeserializeReminderTimer(root),
                    "StreakTracker" => DeserializeStreakTracker(root),
                    _ => throw new JsonException($"Unknown timer type: {typeName}")
                };
            }
        }

        private ClassicStopwatch DeserializeClassicStopwatch(JsonElement root)
        {
            var stopwatch = new ClassicStopwatch();
            
            if (root.TryGetProperty("Name", out var name))
                stopwatch.Name = name.GetString() ?? "Unnamed";
            
            if (root.TryGetProperty("Id", out var id))
                stopwatch.Id = Guid.Parse(id.GetString()!);
            
            if (root.TryGetProperty("ElapsedTicks", out var ticks))
                stopwatch.ElapsedTicks = ticks.GetInt64();
            
            if (root.TryGetProperty("StartTime", out var startTime))
                stopwatch.StartTime = startTime.GetDateTime();
            
            if (root.TryGetProperty("IsRunning", out var isRunning))
                stopwatch.IsRunning = isRunning.GetBoolean();
            
            return stopwatch;
        }

        private ReminderTimer DeserializeReminderTimer(JsonElement root)
        {
            var reminder = new ReminderTimer();
            
            if (root.TryGetProperty("Name", out var name))
                reminder.Name = name.GetString() ?? "Unnamed";
            
            if (root.TryGetProperty("Id", out var id))
                reminder.Id = Guid.Parse(id.GetString()!);
            
            if (root.TryGetProperty("DurationTicks", out var ticks))
                reminder.DurationTicks = ticks.GetInt64();
            
            if (root.TryGetProperty("EndTime", out var endTime))
                reminder.EndTime = endTime.GetDateTime();
            
            if (root.TryGetProperty("IsRunning", out var isRunning))
                reminder.IsRunning = isRunning.GetBoolean();
            
            return reminder;
        }

        private StreakTracker DeserializeStreakTracker(JsonElement root)
        {
            var streak = new StreakTracker();
            
            if (root.TryGetProperty("Name", out var name))
                streak.Name = name.GetString() ?? "Unnamed";
            
            if (root.TryGetProperty("Id", out var id))
                streak.Id = Guid.Parse(id.GetString()!);
            
            if (root.TryGetProperty("StartTime", out var startTime))
                streak.StartTime = startTime.GetDateTime();
            
            if (root.TryGetProperty("LongestStreakTicks", out var longestTicks))
                streak.LongestStreak = TimeSpan.FromTicks(longestTicks.GetInt64());
            
            if (root.TryGetProperty("IsRunning", out var isRunning))
                streak.IsRunning = isRunning.GetBoolean();
            
            if (root.TryGetProperty("BreakHistory", out var history) && history.ValueKind == JsonValueKind.Array)
            {
                streak.BreakHistory = new List<DateTime>();
                foreach (var item in history.EnumerateArray())
                {
                    streak.BreakHistory.Add(item.GetDateTime());
                }
            }
            
            return streak;
        }

        public override void Write(Utf8JsonWriter writer, TimerBase value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write discriminator
            string typeName = value switch
            {
                ClassicStopwatch => "ClassicStopwatch",
                ReminderTimer => "ReminderTimer",
                StreakTracker => "StreakTracker",
                _ => throw new JsonException($"Unknown timer type: {value.GetType().Name}")
            };
            writer.WriteString("$type", typeName);

            // Serialize properties based on type
            switch (value)
            {
                case ClassicStopwatch stopwatch:
                    writer.WriteString("Name", stopwatch.Name);
                    writer.WriteString("Id", stopwatch.Id.ToString());
                    writer.WriteNumber("ElapsedTicks", stopwatch.ElapsedTicks);
                    if (stopwatch.StartTime.HasValue)
                        writer.WriteString("StartTime", stopwatch.StartTime.Value);
                    writer.WriteBoolean("IsRunning", stopwatch.IsRunning);
                    break;

                case ReminderTimer reminder:
                    writer.WriteString("Name", reminder.Name);
                    writer.WriteString("Id", reminder.Id.ToString());
                    writer.WriteNumber("DurationTicks", reminder.DurationTicks);
                    if (reminder.EndTime.HasValue)
                        writer.WriteString("EndTime", reminder.EndTime.Value);
                    writer.WriteBoolean("IsRunning", reminder.IsRunning);
                    break;

                case StreakTracker streak:
                    writer.WriteString("Name", streak.Name);
                    writer.WriteString("Id", streak.Id.ToString());
                    if (streak.StartTime.HasValue)
                        writer.WriteString("StartTime", streak.StartTime.Value);
                    writer.WriteNumber("LongestStreakTicks", streak.LongestStreak.Ticks);
                    writer.WriteBoolean("IsRunning", streak.IsRunning);
                    
                    // Serialize BreakHistory
                    writer.WriteStartArray("BreakHistory");
                    foreach (var date in streak.BreakHistory)
                    {
                        writer.WriteStringValue(date);
                    }
                    writer.WriteEndArray();
                    break;
            }

            writer.WriteEndObject();
        }
    }
}