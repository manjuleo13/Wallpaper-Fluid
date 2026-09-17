using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using LibVLCSharp.Shared;

namespace VideoWallpaper
{
    public partial class WallpaperWindow : Window
    {
        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mediaPlayer;
        private readonly Media _media;
        private readonly DispatcherTimer _reattachTimer;
        private IntPtr _hwnd;

        public WallpaperWindow(LibVLC libVlc, string videoPath, bool muted)
        {
            InitializeComponent();
            _libVlc = libVlc;

            Left = 0;
            Top = 0;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            _mediaPlayer = new MediaPlayer(_libVlc)
            {
                Mute = muted,
                EnableHardwareDecoding = true
            };

            VideoViewControl.MediaPlayer = _mediaPlayer;

            _media = new Media(_libVlc, videoPath, FromType.FromPath);
            _media.AddOption(":input-repeat=65535"); // loop
            _mediaPlayer.EndReached += (s, e) =>
            {
                // Restart playback to loop seamlessly.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _mediaPlayer.Play();
                }));
            };

            SourceInitialized += (s, e) =>
            {
                _hwnd = new WindowInteropHelper(this).Handle;
            };

            // Re-assert the window's bottom-most z-order position periodically,
            // since opening new windows can otherwise end up stacking above it.
            _reattachTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _reattachTimer.Tick += (s, e) => WallpaperInterop.AttachAsWallpaper(_hwnd);

            Loaded += (s, e) =>
            {
                _mediaPlayer.Play(_media);
                WallpaperInterop.AttachAsWallpaper(_hwnd);
                _reattachTimer.Start();
            };
        }

        public void SetMuted(bool muted) => _mediaPlayer.Mute = muted;

        public void StopAndDispose()
        {
            try
            {
                _reattachTimer.Stop();
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
                _media.Dispose();
            }
            catch { /* already disposed / process shutting down */ }
        }
    }
}
