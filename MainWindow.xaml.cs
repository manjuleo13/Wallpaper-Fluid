using System;
using System.IO;
using System.Windows;
using LibVLCSharp.Shared;
using Microsoft.Win32;

namespace VideoWallpaper;

public partial class MainWindow : Window
{
    private LibVLC? _libVlc;
    private WallpaperWindow? _wallpaperWindow;
    private string? _selectedVideoPath;

    public MainWindow()
    {
        InitializeComponent();

        var arch = Environment.Is64BitProcess ? "win-x64" : "win-x86";
        var nativeDir = Path.Combine(AppContext.BaseDirectory, "libvlc", arch);
        Core.Initialize(nativeDir);
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose a video for your wallpaper",
            Filter = "Video files (*.mp4;*.mkv;*.mov;*.avi;*.webm;*.wmv)|*.mp4;*.mkv;*.mov;*.avi;*.webm;*.wmv|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            _selectedVideoPath = dialog.FileName;
            VideoPathBox.Text = Path.GetFileName(_selectedVideoPath);
        }
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedVideoPath) || !File.Exists(_selectedVideoPath))
        {
            StatusText.Foreground = System.Windows.Media.Brushes.IndianRed;
            StatusText.Text = "Please choose a valid video file first.";
            return;
        }

        try
        {
            StopWallpaperInternal();

            _libVlc ??= new LibVLC(enableDebugLogs: false);
            _wallpaperWindow = new WallpaperWindow(_libVlc, _selectedVideoPath, MuteCheckBox.IsChecked == true);
            _wallpaperWindow.Show();

            StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            StatusText.Text = $"Playing \"{Path.GetFileName(_selectedVideoPath)}\" as your wallpaper.";
            StopButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            StatusText.Foreground = System.Windows.Media.Brushes.IndianRed;
            StatusText.Text = "Failed to start wallpaper: " + ex.Message;
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        StopWallpaperInternal();
        StatusText.Foreground = System.Windows.Media.Brushes.White;
        StatusText.Text = "Wallpaper stopped.";
        StopButton.IsEnabled = false;
    }

    private void MuteCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        _wallpaperWindow?.SetMuted(MuteCheckBox.IsChecked == true);
    }

    private void StopWallpaperInternal()
    {
        if (_wallpaperWindow != null)
        {
            _wallpaperWindow.StopAndDispose();
            _wallpaperWindow.Close();
            _wallpaperWindow = null;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        StopWallpaperInternal();
        _libVlc?.Dispose();
        base.OnClosed(e);
    }
}
