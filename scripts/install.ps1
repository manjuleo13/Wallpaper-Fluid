# Installs Video Wallpaper for the current user: publishes a Release build,
# copies it to %LocalAppData%\Programs\VideoWallpaper, adds Start Menu and
# Desktop shortcuts, and registers an uninstall entry (Settings > Installed apps).
#
# Run from the repo root: powershell -ExecutionPolicy Bypass -File scripts\install.ps1

$ErrorAction = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$installDir = Join-Path $env:LocalAppData "Programs\VideoWallpaper"
$exeName = "VideoWallpaper.exe"

Write-Host "Publishing Release build..."
Push-Location $repoRoot
dotnet publish -c Release -o publish
Pop-Location

Write-Host "Stopping any running instance..."
Get-Process VideoWallpaper -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Installing to $installDir ..."
if (Test-Path $installDir) {
    Remove-Item $installDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Copy-Item "$repoRoot\publish\*" $installDir -Recurse -Force

$exePath = Join-Path $installDir $exeName
$iconPath = Join-Path $installDir "icon.ico"

Write-Host "Creating shortcuts..."
$wshell = New-Object -ComObject WScript.Shell

$startMenuDir = Join-Path $env:AppData "Microsoft\Windows\Start Menu\Programs"
$startShortcut = $wshell.CreateShortcut((Join-Path $startMenuDir "Video Wallpaper.lnk"))
$startShortcut.TargetPath = $exePath
$startShortcut.WorkingDirectory = $installDir
$startShortcut.IconLocation = $iconPath
$startShortcut.Save()

$desktopShortcut = $wshell.CreateShortcut((Join-Path ([Environment]::GetFolderPath("Desktop")) "Video Wallpaper.lnk"))
$desktopShortcut.TargetPath = $exePath
$desktopShortcut.WorkingDirectory = $installDir
$desktopShortcut.IconLocation = $iconPath
$desktopShortcut.Save()

Write-Host "Registering uninstall entry..."
$uninstallKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\VideoWallpaper"
New-Item -Path $uninstallKey -Force | Out-Null
Set-ItemProperty -Path $uninstallKey -Name "DisplayName" -Value "Video Wallpaper"
Set-ItemProperty -Path $uninstallKey -Name "DisplayIcon" -Value $iconPath
Set-ItemProperty -Path $uninstallKey -Name "Publisher" -Value "Video Wallpaper"
Set-ItemProperty -Path $uninstallKey -Name "InstallLocation" -Value $installDir
Set-ItemProperty -Path $uninstallKey -Name "UninstallString" -Value "powershell -ExecutionPolicy Bypass -File `"$installDir\uninstall.ps1`""
Set-ItemProperty -Path $uninstallKey -Name "NoModify" -Value 1 -Type DWord
Set-ItemProperty -Path $uninstallKey -Name "NoRepair" -Value 1 -Type DWord

Copy-Item "$repoRoot\scripts\uninstall.ps1" (Join-Path $installDir "uninstall.ps1") -Force

Write-Host ""
Write-Host "Installed to $installDir"
Write-Host "Launch it from the Start Menu, Desktop, or run:"
Write-Host "  `"$exePath`""
