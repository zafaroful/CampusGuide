#!/usr/bin/env pwsh
# Extract Android Debug Certificate SHA-1 and save it
$keystorePath = "$env:USERPROFILE\.android\debug.keystore"
$javaPath = "C:\Program Files\Android\openjdk\jdk-21.0.8\bin\keytool.exe"

Write-Host "`n=== Android Certificate SHA-1 Extractor ===" -ForegroundColor Cyan

if (-not (Test-Path $keystorePath)) {
	Write-Host "ERROR: Keystore not found at: $keystorePath" -ForegroundColor Red
	Write-Host "Build the project first to create the keystore." -ForegroundColor Yellow
	exit 1
}

try {
	$output = & $javaPath -list -v -keystore $keystorePath -alias androiddebugkey -storepass android -keypass android 2>&1

	# Extract SHA1
	$sha1Line = $output | Select-String "SHA1:" | Select-Object -First 1

	if ($sha1Line) {
		$sha1Full = $sha1Line.ToString().Trim()

		# Extract just the fingerprint part
		$sha1Value = ($sha1Full -split "SHA1: ")[1]

		# Remove colons for API key configuration
		$sha1NoColons = $sha1Value -replace ":", ""

		Write-Host "`n✓ Found your SHA-1 fingerprint!`n" -ForegroundColor Green
		Write-Host "Full fingerprint (with colons):" -ForegroundColor Yellow
		Write-Host $sha1Value -ForegroundColor Cyan

		Write-Host "`nFor Google Maps API (without colons):" -ForegroundColor Yellow
		Write-Host $sha1NoColons -ForegroundColor Cyan

		Write-Host "`n" -ForegroundColor White

		# Save to file for reference
		$outputFile = "YOUR_FINGERPRINT.txt"
		@"
SHA-1 FINGERPRINT FOR YOUR ANDROID DEBUG CERTIFICATE
====================================================

Full (with colons):
$sha1Value

For Google Maps API (without colons):
$sha1NoColons

Package Name:
com.companyname.campusguide

Generated: $(Get-Date)

Next Step:
1. Go to https://console.cloud.google.com
2. Create a new project named "CampusGuide"
3. Enable "Maps SDK for Android"
4. Create an API key
5. Restrict it with this fingerprint and package name
"@ | Set-Content $outputFile -Encoding UTF8

		Write-Host "✓ Saved to: $outputFile" -ForegroundColor Green
		Write-Host "`nNow go to: https://console.cloud.google.com" -ForegroundColor Cyan

	} else {
		Write-Host "ERROR: Could not extract SHA1 from certificate" -ForegroundColor Red
		Write-Host "Output: $output"
		exit 1
	}
} catch {
	Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}
