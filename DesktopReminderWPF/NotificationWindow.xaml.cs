using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DesktopReminder.Models;

namespace DesktopReminder
{
    public partial class NotificationWindow : Window
    {
        private readonly ReminderConfig _cfg;
        private readonly DispatcherTimer _topmostTimer;

        public NotificationWindow(ReminderConfig cfg)
        {
            InitializeComponent();
            _cfg = cfg;

            Width = cfg.Width;
            Height = cfg.Height;
            TitleText.Text = cfg.Title;
            MessageText.Text = cfg.Message;

            Loaded += (s, e) => PositionWindow();

            // Re-assert topmost every second so nothing can cover the reminder.
            _topmostTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _topmostTimer.Tick += (s, e) =>
            {
                Topmost = false;
                Topmost = true;
            };
            _topmostTimer.Start();
        }

        private void PositionWindow()
        {
            var area = SystemParameters.WorkArea;
            const double margin = 16;
            double x, y;

            switch (_cfg.Position)
            {
                case PopupPosition.TopLeft:
                    x = area.Left + margin;
                    y = area.Top + margin;
                    break;
                case PopupPosition.TopRight:
                    x = area.Right - Width - margin;
                    y = area.Top + margin;
                    break;
                case PopupPosition.BottomLeft:
                    x = area.Left + margin;
                    y = area.Bottom - Height - margin;
                    break;
                case PopupPosition.BottomRight:
                    x = area.Right - Width - margin;
                    y = area.Bottom - Height - margin;
                    break;
                case PopupPosition.Center:
                    x = area.Left + (area.Width - Width) / 2;
                    y = area.Top + (area.Height - Height) / 2;
                    break;
                case PopupPosition.Custom:
                    x = _cfg.CustomX;
                    y = _cfg.CustomY;
                    break;
                default:
                    x = area.Right - Width - margin;
                    y = area.Top + margin;
                    break;
            }

            Left = x;
            Top = y;
        }

        private void Acknowledge_Click(object sender, RoutedEventArgs e)
        {
            _topmostTimer.Stop();
            LogAck();
            Close();
        }

        private void LogAck()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "ack_log.txt");
                File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Acknowledged: {_cfg.Message}{Environment.NewLine}");
            }
            catch
            {
                // Non-critical: logging failure shouldn't block dismissal.
            }
        }
    }
}
