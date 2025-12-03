# PowerShell script to build the application and prepare for installer
# This script publishes the app as self-contained and prepares files for Inno Setup

Write-Host "Building BarcodeApp installer..." -ForegroundColor Green

# Get the project directory (script location)
$scriptPath = $MyInvocation.MyCommand.Path
if (-not $scriptPath) {
    $scriptPath = $PSCommandPath
}
if (-not $scriptPath) {
    $scriptPath = (Get-Location).Path
}
$projectDir = Split-Path -Parent $scriptPath
if (-not $projectDir) {
    $projectDir = Get-Location
}

$projectFile = Join-Path $projectDir "BarcodeApp.csproj"
$publishDir = Join-Path $projectDir "publish"

Write-Host "Project directory: $projectDir" -ForegroundColor Cyan
Write-Host "Project file: $projectFile" -ForegroundColor Cyan

# Clean previous publish
if (Test-Path $publishDir) {
    Write-Host "Cleaning previous publish directory..." -ForegroundColor Yellow
    Remove-Item -Path $publishDir -Recurse -Force
}

# Publish as self-contained (includes .NET runtime)
Write-Host "Publishing application as self-contained..." -ForegroundColor Green
dotnet publish $projectFile `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDir `
    --property:PublishSingleFile=false `
    --property:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Published files are in: $publishDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Install Inno Setup from https://jrsoftware.org/isinfo.php" -ForegroundColor White
Write-Host "2. Open BarcodeApp.iss in Inno Setup Compiler" -ForegroundColor White
Write-Host "3. Build the installer" -ForegroundColor White

