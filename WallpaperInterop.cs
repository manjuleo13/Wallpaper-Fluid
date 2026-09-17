using System;
using System.Runtime.InteropServices;

namespace VideoWallpaper
{
    /// <summary>
    /// Keeps the wallpaper window pinned to the bottom of the desktop z-order,
    /// so it renders behind all other application windows.
    ///
    /// Note: this deliberately does NOT use the classic Progman/WorkerW
    /// reparenting trick (SetParent into a WorkerW behind the desktop icons).
    /// On this Windows build that reparenting is unreliable - it can succeed
    /// briefly and then get reverted by the shell, and re-attempting it
    /// periodically was observed to intermittently push the window somewhere
    /// invisible (behind the real desktop background layer) instead of behind
    /// the icons. Plain z-order placement is less fancy - desktop icons won't
    /// show through - but it is stable.
    /// </summary>
    internal static class WallpaperInterop
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const int SW_SHOWNOACTIVATE = 4;
        private static readonly IntPtr HWND_BOTTOM = new(1);

        /// <summary>Pushes the window to the bottom of the desktop z-order.</summary>
        public static void AttachAsWallpaper(IntPtr targetWindow)
        {
            SetWindowPos(targetWindow, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            ShowWindow(targetWindow, SW_SHOWNOACTIVATE);
        }
    }
}
