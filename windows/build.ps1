Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  Building EyeSaver for Windows (Single-File Standalone)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    Exit 1
}

$outputDir = Join-Path $PSScriptRoot "..\dist\windows"
Write-Host "`nCompiling Release binary for win-x64..." -ForegroundColor Yellow
dotnet publish (Join-Path $PSScriptRoot "EyeSaver.csproj") -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $outputDir

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================================" -ForegroundColor Green
    Write-Host "  BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "  Output executable: dist\windows\EyeSaver.exe" -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Green
} else {
    Write-Host "`n[ERROR] Build failed." -ForegroundColor Red
    Exit 1
}
