using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DesktopReminder.Models;

namespace DesktopReminder
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon? _trayIcon;
        private System.Windows.Forms.ToolStripMenuItem? _trayEnabledItem;
        private bool _isExit;

        public MainWindow()
        {
            InitializeComponent();
            LoadIntoUI(ReminderConfig.Load());
            SetupTrayIcon();
            Closing += MainWindow_Closing;
        }

        // ----- Tray icon -----

        private void SetupTrayIcon()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Desktop Reminder"
            };
            _trayIcon.DoubleClick += (s, e) => ShowFromTray();

            var menu = new System.Windows.Forms.ContextMenuStrip();

            var openItem = menu.Items.Add("Open settings");
            openItem.Click += (s, e) => ShowFromTray();

            _trayEnabledItem = new System.Windows.Forms.ToolStripMenuItem("Notifications enabled")
            {
                CheckOnClick = true,
                Checked = ChkEnabled.IsChecked == true
            };
            _trayEnabledItem.Click += (s, e) =>
            {
                var cfg = ReminderConfig.Load();
                cfg.Enabled = _trayEnabledItem.Checked;
                cfg.Save();
                ChkEnabled.IsChecked = cfg.Enabled;
                StatusText.Text = cfg.Enabled ? "Notifications enabled" : "Notifications disabled";
            };
            menu.Items.Add(_trayEnabledItem);

            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            var exitItem = menu.Items.Add("Exit");
            exitItem.Click += (s, e) =>
            {
                _isExit = true;
                Close();
            };

            _trayIcon.ContextMenuStrip = menu;
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                // Minimize to tray instead of quitting; the scheduler keeps running.
                e.Cancel = true;
                Hide();
                return;
            }

            _trayIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        // ----- Load / gather config -----

        private void LoadIntoUI(ReminderConfig cfg)
        {
            ChkEnabled.IsChecked = cfg.Enabled;
            TxtTitle.Text = cfg.Title;
            TxtMessage.Text = cfg.Message;
            TxtInterval.Text = cfg.IntervalMinutes.ToString();
            TxtStartTime.Text = cfg.StartTime;
            TxtEndTime.Text = cfg.EndTime;
            TxtWidth.Text = cfg.Width.ToString();
            TxtHeight.Text = cfg.Height.ToString();
            TxtCheckInterval.Text = cfg.CheckIntervalSeconds.ToString();
            TxtCustomX.Text = cfg.CustomX.ToString();
            TxtCustomY.Text = cfg.CustomY.ToString();

            ChkMon.IsChecked = cfg.ActiveDays.Contains("Mon");
            ChkTue.IsChecked = cfg.ActiveDays.Contains("Tue");
            ChkWed.IsChecked = cfg.ActiveDays.Contains("Wed");
            ChkThu.IsChecked = cfg.ActiveDays.Contains("Thu");
            ChkFri.IsChecked = cfg.ActiveDays.Contains("Fri");
            ChkSat.IsChecked = cfg.ActiveDays.Contains("Sat");
            ChkSun.IsChecked = cfg.ActiveDays.Contains("Sun");

            foreach (var obj in CmbPosition.Items)
            {
                if (obj is ComboBoxItem item && (string)item.Tag == cfg.Position.ToString())
                {
                    CmbPosition.SelectedItem = item;
                    break;
                }
            }

            UpdateCustomFieldsEnabled();
        }

        private ReminderConfig GatherFromUI()
        {
            var cfg = new ReminderConfig
            {
                Enabled = ChkEnabled.IsChecked == true,
                Title = string.IsNullOrWhiteSpace(TxtTitle.Text) ? "Reminder" : TxtTitle.Text.Trim(),
                Message = TxtMessage.Text.Trim(),
                IntervalMinutes = ParseIntOr(TxtInterval.Text, 60),
                StartTime = TxtStartTime.Text.Trim(),
                EndTime = TxtEndTime.Text.Trim(),
                Width = ParseIntOr(TxtWidth.Text, 340),
                Height = ParseIntOr(TxtHeight.Text, 160),
                CheckIntervalSeconds = ParseIntOr(TxtCheckInterval.Text, 30),
                CustomX = ParseIntOr(TxtCustomX.Text, 100),
                CustomY = ParseIntOr(TxtCustomY.Text, 100)
            };

            cfg.ActiveDays.Clear();
            if (ChkMon.IsChecked == true) cfg.ActiveDays.Add("Mon");
            if (ChkTue.IsChecked == true) cfg.ActiveDays.Add("Tue");
            if (ChkWed.IsChecked == true) cfg.ActiveDays.Add("Wed");
            if (ChkThu.IsChecked == true) cfg.ActiveDays.Add("Thu");
            if (ChkFri.IsChecked == true) cfg.ActiveDays.Add("Fri");
            if (ChkSat.IsChecked == true) cfg.ActiveDays.Add("Sat");
            if (ChkSun.IsChecked == true) cfg.ActiveDays.Add("Sun");

            var selected = CmbPosition.SelectedItem as ComboBoxItem;
            cfg.Position = Enum.TryParse<PopupPosition>(selected?.Tag as string, out var pos)
                ? pos
                : PopupPosition.TopRight;

            return cfg;
        }

        private static int ParseIntOr(string text, int fallback) =>
            int.TryParse(text, out var v) ? v : fallback;

        private void UpdateCustomFieldsEnabled()
        {
            var selected = CmbPosition.SelectedItem as ComboBoxItem;
            bool isCustom = (selected?.Tag as string) == "Custom";
            TxtCustomX.IsEnabled = isCustom;
            TxtCustomY.IsEnabled = isCustom;
        }

        // ----- Event handlers -----

        private void CmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            UpdateCustomFieldsEnabled();

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (!TimeSpan.TryParse(TxtStartTime.Text.Trim(), out _) ||
                !TimeSpan.TryParse(TxtEndTime.Text.Trim(), out _))
            {
                System.Windows.MessageBox.Show(
                    "Start and end time must be in 24-hour HH:mm format, e.g. 09:00.",
                    "Invalid time", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cfg = GatherFromUI();
            cfg.Save();
            App.Scheduler?.ResetTimer();

            if (_trayEnabledItem != null)
                _trayEnabledItem.Checked = cfg.Enabled;

            StatusText.Text = $"Applied at {DateTime.Now:HH:mm:ss}";
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            var cfg = GatherFromUI();
            App.Scheduler?.ShowPopup(cfg);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            _isExit = true;
            Close();
        }
    }
}
