using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VideoWallpaper
{
    /// <summary>
    /// Implements the "WorkerW" technique used by tools like Wallpaper Engine:
    /// ask Progman to spawn a WorkerW behind the desktop icons, then reparent
    /// our own window into it so it renders behind the icons but above the
    /// normal desktop background.
    /// </summary>
    internal static class WallpaperInterop
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam,
            uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const uint SMTO_NORMAL = 0x0000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const int SW_SHOW = 5;
        private static readonly IntPtr HWND_BOTTOM = new(1);

        /// <summary>
        /// Reparents the given window handle so it renders as the desktop wallpaper,
        /// behind desktop icons. Returns the handle it was attached to, or
        /// IntPtr.Zero if the technique failed entirely.
        /// </summary>
        public static IntPtr AttachAsWallpaper(IntPtr targetWindow)
        {
            IntPtr progman = FindWindow("Progman", null);
            if (progman == IntPtr.Zero)
                return IntPtr.Zero;

            // Ask Progman to spawn a WorkerW behind the icons. Undocumented but
            // widely relied upon (0x052C).
            SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, SMTO_NORMAL, 1000, out _);

            // Classic technique (most Windows 10/11 builds): SHELLDLL_DefView (the
            // desktop icons) ends up hosted by a window whose next sibling is a
            // fresh WorkerW - that WorkerW is what we attach behind.
            IntPtr workerw = IntPtr.Zero;
            IntPtr progmanHostsIconsDirectly = IntPtr.Zero;
            EnumWindows((hwnd, lParam) =>
            {
                IntPtr shellDefView = FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellDefView != IntPtr.Zero)
                {
                    if (hwnd == progman)
                    {
                        // Newer Windows builds: SHELLDLL_DefView is a direct child
                        // of Progman itself, no separate WorkerW is created.
                        progmanHostsIconsDirectly = progman;
                    }
                    else
                    {
                        IntPtr candidate = FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);
                        if (candidate != IntPtr.Zero)
                            workerw = candidate;
                    }
                }
                return true;
            }, IntPtr.Zero);

            // Try to reparent behind the icons (works on most Windows installs).
            // Some builds silently revert this within a second or two; regardless
            // of whether it stuck, always push to the bottom of whatever z-order
            // the window currently belongs to (its parent's children, or the
            // top-level desktop order if reparenting didn't take) so the video
            // still renders behind other application windows either way.
            IntPtr attachedTo = IntPtr.Zero;
            if (workerw != IntPtr.Zero)
            {
                SetParent(targetWindow, workerw);
                attachedTo = workerw;
            }
            else if (progmanHostsIconsDirectly != IntPtr.Zero)
            {
                SetParent(targetWindow, progman);
                attachedTo = progman;
            }

            SetWindowPos(targetWindow, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            ShowWindow(targetWindow, SW_SHOW);
            return attachedTo;
        }

        /// <summary>Detaches the window from the WorkerW back to being a normal top-level window.</summary>
        public static void Detach(IntPtr targetWindow)
        {
            SetParent(targetWindow, IntPtr.Zero);
        }
    }
}
