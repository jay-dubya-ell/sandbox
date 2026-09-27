# Desktop Reminder (WPF, C#)

A standalone Windows 11 app that shows an always-on-top notification popup at a
configurable interval, on configurable days/hours, at a configurable screen
position — with a settings window to adjust everything and enable/disable
notifications, plus a system tray icon so it keeps running in the background.

## Project contents
```
DesktopReminder.csproj
App.xaml / App.xaml.cs              Startup, owns the background scheduler
MainWindow.xaml / .xaml.cs          Settings UI (this is what you'll edit most)
NotificationWindow.xaml / .xaml.cs  The always-on-top popup itself
Models/ReminderConfig.cs            Settings model + JSON load/save (config.json)
Services/ReminderScheduler.cs       Timer that decides when to show a popup
```

## Requirements
- .NET 8 SDK (Windows) — https://dotnet.microsoft.com/download
- Windows 10/11

## Run it (development)
```
cd DesktopReminderWPF
dotnet run
```
The settings window opens on launch. `config.json` is created next to the
executable the first time it runs.

## Build a standalone .exe
Single-file, self-contained (no .NET runtime install needed on the target PC):
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
The executable lands in `bin\Release\net8.0-windows\win-x64\publish\DesktopReminder.exe`.
Copy that .exe anywhere; `config.json` and `ack_log.txt` will be created next to it.

## Using the app
- **Notifications enabled** checkbox (top of the window, and in the tray menu) — turns the whole thing on/off without losing your settings.
- **General** — the popup's title and message text.
- **Schedule** — how often it repeats (minutes), which days it's allowed to fire, and the active hour range (24h `HH:mm`).
- **Appearance and position** — corner/center/custom screen position, plus popup width/height.
- **Advanced** — how often (seconds) the app checks the clock; doesn't need to match the reminder interval.
- **Apply** — saves `config.json` and takes effect immediately (also resets the "time since last shown" counter, so a shortened interval kicks in right away).
- **Test notification** — shows the popup immediately using whatever is currently in the form, without waiting or saving.
- **Exit** — closes the app for real. Closing the window with the X instead just minimizes it to the system tray (the reminder keeps running); use the tray icon's right-click menu to reopen or exit.

The popup itself has **no title bar and no close (X) button** — on-screen "persistence" is enforced by re-asserting `Topmost` every second, so it can't get buried under other windows. The only way to dismiss it is the **Acknowledge** button, and every acknowledgment is timestamped in `ack_log.txt`.

## Run automatically at login
Use Task Scheduler → Create Task → Trigger "At log on" → Action: start
`DesktopReminder.exe` (published build) or `dotnet run` from the project
folder. Check "Run only when user is logged on" so the tray icon and popups
are visible.
