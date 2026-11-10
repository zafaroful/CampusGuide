#!/usr/bin/env pwsh
# Update AndroidManifest.xml with Google Maps API Key
# This script safely updates the API key without overwriting other settings

param(
	[Parameter(Mandatory=$true, HelpMessage="Your Google Maps API key (starts with AIzaSy...)")]
	[string]$ApiKey,

	[ValidateSet("debug", "release")]
	[string]$Build = "debug"
)

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host "AndroidManifest.xml API Key Updater" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host ""

# Validate API key format
if (-not $ApiKey.StartsWith("AIzaSy")) {
	Write-Host "ERROR: API key should start with 'AIzaSy'" -ForegroundColor Red
	Write-Host "Provided key: $ApiKey"
	exit 1
}

# Find AndroidManifest.xml
$manifestPath = "CampusGuide\Platforms\Android\AndroidManifest.xml"

if (-not (Test-Path $manifestPath)) {
	Write-Host "ERROR: AndroidManifest.xml not found at: $manifestPath" -ForegroundColor Red
	exit 1
}

Write-Host "Found: $manifestPath" -ForegroundColor Green
Write-Host ""

# Read the manifest
$manifest = Get-Content $manifestPath -Raw

# Check if API key metadata already exists
if ($manifest -match 'com\.google\.android\.geo\.API_KEY') {
	Write-Host "Updating existing API key..." -ForegroundColor Yellow

	# Replace existing API key
	$newManifest = $manifest -replace 'android:value="[^"]*?"(\s*/?>.*?com\.google\.android\.geo\.API_KEY)', "android:value=`"$ApiKey`"$1"
} else {
	Write-Host "Adding new API key metadata..." -ForegroundColor Yellow

	# Find the application closing tag and insert before it
	if ($manifest -match '</application>') {
		$newManifest = $manifest -replace '</application>', "    `n    <!-- Google Maps API Key -->`n    <meta-data android:name=`"com.google.android.geo.API_KEY`" android:value=`"$ApiKey`" />`n  </application>"
	} else {
		Write-Host "ERROR: Could not find </application> tag in manifest" -ForegroundColor Red
		exit 1
	}
}

# Backup original
$backupPath = "$manifestPath.backup"
Copy-Item $manifestPath $backupPath
Write-Host "Backup created: $backupPath" -ForegroundColor Gray
Write-Host ""

# Write updated manifest
Set-Content $manifestPath $newManifest -Encoding UTF8

Write-Host "✓ AndroidManifest.xml updated!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Clean the build: dotnet clean -f net10.0-android"
Write-Host "2. Rebuild: dotnet build -f net10.0-android -c $Build"
Write-Host "3. Deploy: dotnet run -f net10.0-android -c $Build --no-build"
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
