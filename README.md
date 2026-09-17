# Video Wallpaper

A small Windows desktop app that lets you pick any video file and set it as an
animated desktop wallpaper.

## How it works

- **Video playback**: [LibVLCSharp](https://code.videolan.org/videolan/LibVLCSharp)
  (a .NET wrapper around libVLC) decodes and renders the video, so it supports
  most common formats (MP4, MKV, MOV, AVI, WebM, WMV, ...) with hardware
  acceleration where available.
- **Desktop integration**: the video plays in a borderless, full-screen window
  that the app pushes behind your other application windows. On most Windows
  installs it also reparents the window into `Progman`/`WorkerW` (the same
  trick used by tools like Wallpaper Engine) so it renders behind your desktop
  icons; on builds where that reparenting is blocked or reverted by the shell,
  it falls back to keeping the window at the bottom of the normal z-order via
  a periodic re-assert timer, so the video still shows as a background behind
  your open windows either way.

## Usage

1. Run `VideoWallpaper.exe`.
2. Click **Browse...** and pick a video file.
3. Optionally check **Mute audio**.
4. Click **Set as Wallpaper**.
5. Click **Stop Wallpaper** to remove it.

### Headless mode

You can also apply the wallpaper directly without showing the control window,
e.g. from a Startup shortcut so it re-applies automatically after signing in:

```
VideoWallpaper.exe --apply "C:\path\to\video.mp4" --mute
```

## Building

Requires the .NET 8 SDK.

```
dotnet build -c Release
```

The build restores `LibVLCSharp`, `LibVLCSharp.WPF`, and
`VideoLAN.LibVLC.Windows` from NuGet automatically (the last one bundles the
native libVLC engine, so no separate VLC install is required).

**Note:** build the project from a short path (e.g. `C:\VideoWallpaper`) — a
deeply nested path can exceed Windows' 260-character `MAX_PATH` limit while
copying libVLC's plugin files, which silently breaks video playback.
