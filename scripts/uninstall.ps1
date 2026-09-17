# Uninstalls Video Wallpaper: stops it, removes shortcuts, the install
# directory, and the registry uninstall entry.

$installDir = Join-Path $env:LocalAppData "Programs\VideoWallpaper"

Write-Host "Stopping Video Wallpaper..."
Get-Process VideoWallpaper -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Removing shortcuts..."
Remove-Item (Join-Path $env:AppData "Microsoft\Windows\Start Menu\Programs\Video Wallpaper.lnk") -ErrorAction SilentlyContinue
Remove-Item (Join-Path ([Environment]::GetFolderPath("Desktop")) "Video Wallpaper.lnk") -ErrorAction SilentlyContinue

Write-Host "Removing uninstall registry entry..."
Remove-Item "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\VideoWallpaper" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Removing install directory..."
Start-Sleep -Milliseconds 500
Remove-Item $installDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Video Wallpaper has been uninstalled."
