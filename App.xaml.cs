using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using LibVLCSharp.Shared;

namespace VideoWallpaper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Optional headless mode: "VideoWallpaper.exe --apply <videoPath> [--mute]"
    // Sets the wallpaper directly without showing the control window. Useful for
    // re-applying the wallpaper from a Startup shortcut after signing in.
    private LibVLC? _libVlc;
    private WallpaperWindow? _wallpaperWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Length >= 2 && e.Args[0] == "--apply")
        {
            var videoPath = e.Args[1];
            var mute = e.Args.Length > 2 && e.Args[2] == "--mute";

            if (!File.Exists(videoPath))
            {
                Shutdown(1);
                return;
            }

            try
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                _libVlc = new LibVLC(enableDebugLogs: false);
                _wallpaperWindow = new WallpaperWindow(_libVlc, videoPath, mute);
                _wallpaperWindow.Show();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "apply-error.log"), ex.ToString());
            }
            return;
        }

        base.OnStartup(e);
    }
}
