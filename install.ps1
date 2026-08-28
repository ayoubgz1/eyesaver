# EyeSaver 1-Line Installer for Windows
# Usage: irm https://raw.githubusercontent.com/ayoubgz1/eyesaver/main/install.ps1 | iex

$ErrorActionPreference = "Stop"

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  👁️  Installing EyeSaver for Windows (20-20-20 Rule)   " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

$installDir = Join-Path $env:LOCALAPPDATA "EyeSaver"
$exePath = Join-Path $installDir "EyeSaver.exe"
$downloadUrl = "https://github.com/ayoubgz1/eyesaver/releases/latest/download/EyeSaver.exe"

# Stop existing running instance
Get-Process -Name "EyeSaver" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Ensure install directory exists
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

Write-Host "⬇️  Downloading latest EyeSaver from GitHub..." -ForegroundColor Yellow
try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls13
    Invoke-WebRequest -Uri $downloadUrl -OutFile $exePath -UseBasicParsing
} catch {
    Write-Host "[ERROR] Failed to download EyeSaver.exe: $($_.Exception.Message)" -ForegroundColor Red
    Exit 1
}

# Create Desktop Shortcut
try {
    $wshShell = New-Object -ComObject WScript.Shell
    
    # Desktop
    $desktopPath = [Environment]::GetFolderPath("Desktop")
    $desktopShortcut = $wshShell.CreateShortcut((Join-Path $desktopPath "EyeSaver.lnk"))
    $desktopShortcut.TargetPath = $exePath
    $desktopShortcut.WorkingDirectory = $installDir
    $desktopShortcut.Description = "EyeSaver - 20-20-20 Eye Rest Timer"
    $desktopShortcut.Save()

    # Start Menu
    $startMenuPrograms = [Environment]::GetFolderPath("Programs")
    $startMenuShortcut = $wshShell.CreateShortcut((Join-Path $startMenuPrograms "EyeSaver.lnk"))
    $startMenuShortcut.TargetPath = $exePath
    $startMenuShortcut.WorkingDirectory = $installDir
    $startMenuShortcut.Description = "EyeSaver - 20-20-20 Eye Rest Timer"
    $startMenuShortcut.Save()
} catch {
    Write-Host "Warning: Could not create shortcuts: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "✅ EyeSaver successfully installed to $installDir" -ForegroundColor Green
Write-Host "🚀 Launching EyeSaver in system tray..." -ForegroundColor Green

Start-Process -FilePath $exePath
