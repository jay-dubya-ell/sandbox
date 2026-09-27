using System;
using System.Windows.Threading;
using DesktopReminder.Models;

namespace DesktopReminder.Services
{
    /// <summary>
    /// Polls the current config on a timer and shows a NotificationWindow
    /// whenever the configured interval has elapsed inside the active window.
    /// </summary>
    public class ReminderScheduler
    {
        private static readonly string[] DayOrder = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        private readonly DispatcherTimer _timer;
        private DateTime? _lastShown;
        private NotificationWindow? _activePopup;

        public Func<ReminderConfig> GetConfig { get; }

        public ReminderScheduler(Func<ReminderConfig> getConfig)
        {
            GetConfig = getConfig;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private static bool IsWithinActiveWindow(DateTime now, ReminderConfig cfg)
        {
            var dayIndex = ((int)now.DayOfWeek + 6) % 7; // Sunday=0 -> index 6, Monday=1 -> index 0
            var dayName = DayOrder[dayIndex];
            if (!cfg.ActiveDays.Contains(dayName))
                return false;

            if (!TimeSpan.TryParse(cfg.StartTime, out var start))
                start = new TimeSpan(9, 0, 0);
            if (!TimeSpan.TryParse(cfg.EndTime, out var end))
                end = new TimeSpan(17, 0, 0);

            var t = now.TimeOfDay;
            return t >= start && t <= end;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var cfg = GetConfig();
            _timer.Interval = TimeSpan.FromSeconds(Math.Max(5, cfg.CheckIntervalSeconds));

            if (!cfg.Enabled || _activePopup != null)
                return;

            var now = DateTime.Now;
            if (!IsWithinActiveWindow(now, cfg))
                return;

            var due = _lastShown == null || (now - _lastShown.Value).TotalMinutes >= cfg.IntervalMinutes;
            if (due)
            {
                ShowPopup(cfg);
                _lastShown = now;
            }
        }

        /// <summary>Shows the popup immediately (used for the Test button too).</summary>
        public void ShowPopup(ReminderConfig cfg)
        {
            if (_activePopup != null)
                return;

            _activePopup = new NotificationWindow(cfg);
            _activePopup.Closed += (s, e) => _activePopup = null;
            _activePopup.Show();
        }

        /// <summary>Call after Apply so a changed interval starts counting from now.</summary>
        public void ResetTimer() => _lastShown = null;
    }
}
