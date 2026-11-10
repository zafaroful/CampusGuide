#!/usr/bin/env pwsh
# Get Android Debug Certificate Fingerprint
# This script extracts the SHA-1 fingerprint needed for Google Maps API key configuration

param(
	[ValidateSet("debug", "release")]
	[string]$BuildType = "debug"
)

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host "Android Certificate Fingerprint Extractor" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host ""

# Path to keytool
$javaPath = "C:\Program Files\Android\openjdk\jdk-21.0.8\bin\keytool.exe"

if (-not (Test-Path $javaPath)) {
	Write-Host "ERROR: keytool not found at $javaPath" -ForegroundColor Red
	Write-Host ""
	Write-Host "Make sure Android JDK is installed. Update the path if needed."
	exit 1
}

if ($BuildType -eq "debug") {
	$keystorePath = "$env:USERPROFILE\.android\debug.keystore"
	$alias = "androiddebugkey"
	$password = "android"

	Write-Host "Getting DEBUG certificate fingerprint..." -ForegroundColor Yellow
} else {
	Write-Host "For RELEASE builds, provide the path to your release keystore:" -ForegroundColor Yellow
	$keystorePath = Read-Host "Keystore path"
	$alias = Read-Host "Alias name"
	$password = Read-Host "Password" -AsSecureString
	$password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password))
}

if (-not (Test-Path $keystorePath)) {
	Write-Host "ERROR: Keystore not found at: $keystorePath" -ForegroundColor Red
	Write-Host ""
	Write-Host "For debug builds, the keystore will be created on your first Android build."
	Write-Host "After building, run this script again."
	exit 1
}

Write-Host "Keystore: $keystorePath" -ForegroundColor Gray
Write-Host "Alias: $alias" -ForegroundColor Gray
Write-Host ""

try {
	$output = & $javaPath -list -v -keystore $keystorePath -alias $alias -storepass $password -keypass $password 2>&1

	# Extract fingerprints
	$sha1 = ($output | Select-String "SHA1:" | ForEach-Object { $_.Line.Split(":")[1].Trim() }) -join ""
	$sha256 = ($output | Select-String "SHA256:" | ForEach-Object { $_.Line.Split(":")[1].Trim() }) -join ""

	if ([string]::IsNullOrWhiteSpace($sha1)) {
		Write-Host "ERROR: Could not extract fingerprints" -ForegroundColor Red
		Write-Host "Full output:"
		Write-Host $output
		exit 1
	}

	Write-Host "✓ Certificate found!" -ForegroundColor Green
	Write-Host ""
	Write-Host "SHA-1 (for Google Maps):" -ForegroundColor Cyan
	Write-Host $sha1 -ForegroundColor Yellow
	Write-Host ""
	Write-Host "SHA-256:" -ForegroundColor Cyan
	Write-Host $sha256 -ForegroundColor Yellow
	Write-Host ""
	Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	Write-Host "TIP: Use the SHA-1 fingerprint above when configuring"
	Write-Host "     your Google Maps API key at console.cloud.google.com"
	Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

	# Copy to clipboard
	$sha1 | Set-Clipboard
	Write-Host ""
	Write-Host "✓ SHA-1 copied to clipboard!" -ForegroundColor Green

} catch {
	Write-Host "ERROR: Failed to extract fingerprint" -ForegroundColor Red
	Write-Host $_.Exception.Message
	exit 1
}
