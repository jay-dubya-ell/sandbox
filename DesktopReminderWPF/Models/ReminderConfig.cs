using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DesktopReminder.Models
{
    public enum PopupPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center,
        Custom
    }

    public class ReminderConfig
    {
        public bool Enabled { get; set; } = true;
        public int IntervalMinutes { get; set; } = 60;
        public List<string> ActiveDays { get; set; } = new() { "Mon", "Tue", "Wed", "Thu", "Fri" };
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public string Title { get; set; } = "Reminder";
        public string Message { get; set; } = "Time for a break! Stand up and stretch.";
        public PopupPosition Position { get; set; } = PopupPosition.TopRight;
        public int CustomX { get; set; } = 100;
        public int CustomY { get; set; } = 100;
        public int Width { get; set; } = 340;
        public int Height { get; set; } = 160;
        public int CheckIntervalSeconds { get; set; } = 30;

        private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "config.json");

        public static ReminderConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var cfg = JsonSerializer.Deserialize<ReminderConfig>(json);
                    if (cfg != null)
                        return cfg;
                }
            }
            catch
            {
                // Fall through to defaults on any read/parse failure.
            }

            var def = new ReminderConfig();
            def.Save();
            return def;
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
