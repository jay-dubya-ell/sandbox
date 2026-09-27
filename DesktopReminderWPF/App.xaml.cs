using System.Windows;
using DesktopReminder.Models;
using DesktopReminder.Services;

namespace DesktopReminder
{
    public partial class App : Application
    {
        public static ReminderScheduler? Scheduler { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Keep running in the tray even if the settings window is closed/hidden.
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Scheduler = new ReminderScheduler(() => ReminderConfig.Load());

            var window = new MainWindow();
            window.Show();
        }
    }
}
