#!/usr/bin/env pwsh
<#
.SYNOPSIS
	Quick Android Emulator Utilities Script for CampusGuide
.DESCRIPTION
	Provides quick utility functions for Android development:
	- Quick deploy
	- View logs
	- Uninstall app
	- Clear app data
	- Restart adb
	- Device info
#>

param(
	[string]$command = "",
	[string]$emulatorName = ""
)

$packageId = "com.companyname.campusguide"

function Write-Status {
	param([string]$message, [string]$type = "Info")
	$colors = @{ Success = "Green"; Error = "Red"; Warning = "Yellow"; Info = "Cyan"; Highlight = "Magenta" }
	$color = $colors[$type]
	Write-Host "[$((Get-Date).ToString('HH:mm:ss'))] " -NoNewline -ForegroundColor Gray
	Write-Host $message -ForegroundColor $color
}

function Show-Menu {
	Write-Host @"
╔════════════════════════════════════════════════════════════════════════╗
║           CampusGuide Android Emulator Utilities Menu                  ║
╚════════════════════════════════════════════════════════════════════════╝

Quick Commands:

  1  Deploy & Run       Build APK, install, and launch app
  2  Logs Only          View live logs from running app
  3  Uninstall App      Remove app from emulator
  4  Clear App Data     Reset app preferences and data
  5  Device Info        Show connected devices and details
  6  Restart ADB        Restart Android Debug Bridge
  7  Stop Emulator      Stop all running emulators
  8  Full Deploy        Full rebuild and deploy

  0  Exit

" -ForegroundColor Cyan
}

function Deploy-Quick {
	Write-Host ""
	Write-Status "Running quick deploy..." Highlight
	& "$PSScriptRoot\deploy-android.ps1"
}

function View-Logs {
	Write-Host ""
	Write-Status "Showing logs..." Info

	$devices = adb devices | Select-Object -Skip 1 | Where-Object { $_ -match "emulator|device" } | Select-Object -First 1
	if ($devices) {
		$device = ($devices -split "\s+" | Select-Object -First 1).Trim()
		Write-Status "Streaming from: $device" Success

		& adb -s $device logcat --clear 2>$null
		& adb -s $device logcat | ForEach-Object {
			if ($_ -match "Exception|Error|FATAL") { Write-Host $_ -ForegroundColor Red }
			elseif ($_ -match "Warning") { Write-Host $_ -ForegroundColor Yellow }
			elseif ($_ -match "Success") { Write-Host $_ -ForegroundColor Green }
			else { Write-Host $_ }
		}
	}
	else {
		Write-Status "No devices found" Error
	}
}

function Uninstall-App {
	Write-Status "Uninstalling $packageId..." Warning
	$result = adb uninstall $packageId 2>&1
	if ($LASTEXITCODE -eq 0 -or $result -match "Success") {
		Write-Status "Uninstalled successfully" Success
	}
	else {
		Write-Status "Uninstall failed: $result" Error
	}
}

function Clear-AppData {
	Write-Status "Clearing app data for $packageId..." Warning
	$result = adb shell pm clear $packageId 2>&1
	if ($LASTEXITCODE -eq 0 -or $result -match "Success") {
		Write-Status "App data cleared" Success
	}
	else {
		Write-Status "Clear failed: $result" Error
	}
}

function Show-DeviceInfo {
	Write-Host ""
	Write-Status "Connected Devices/Emulators:" Highlight
	Write-Host ""

	$devices = adb devices
	Write-Host $devices

	Write-Host ""
	Write-Status "Device Details:" Info

	$device = (adb devices | Select-Object -Skip 1 | Where-Object { $_ -match "emulator|device" } | Select-Object -First 1)
	if ($device) {
		$deviceId = ($device -split "\s+" | Select-Object -First 1).Trim()

		Write-Host ""
		Write-Host "  Model: " -NoNewline -ForegroundColor Cyan
		adb -s $deviceId shell getprop ro.product.model

		Write-Host "  Android Version: " -NoNewline -ForegroundColor Cyan
		adb -s $deviceId shell getprop ro.build.version.release

		Write-Host "  API Level: " -NoNewline -ForegroundColor Cyan
		adb -s $deviceId shell getprop ro.build.version.sdk

		Write-Host "  Boot Complete: " -NoNewline -ForegroundColor Cyan
		$boot = adb -s $deviceId shell getprop dev.bootcomplete
		Write-Host $(if ($boot -eq "1") { "Yes ✓" } else { "Loading..." })

		Write-Host "  Storage: " -NoNewline -ForegroundColor Cyan
		adb -s $deviceId shell df /data | Select-Object -Skip 1 | ForEach-Object {
			$parts = $_ -split "\s+" | Where-Object { $_ }
			Write-Host "$($parts[-3]) / $($parts[-4])"
		}
	}
}

function Restart-ADB {
	Write-Status "Restarting ADB..." Warning
	adb kill-server | Out-Null
	Start-Sleep -Seconds 1
	adb start-server | Out-Null
	Write-Status "ADB restarted" Success

	Write-Host ""
	adb devices
}

function Stop-Emulator {
	Write-Status "Stopping all emulators..." Warning

	$devices = adb devices | Select-Object -Skip 1 | Where-Object { $_ -match "emulator" }
	if ($devices.Count -eq 0) {
		Write-Status "No running emulators" Info
		return
	}

	$devices | ForEach-Object {
		$device = ($_ -split "\s+" | Select-Object -First 1).Trim()
		Write-Status "Stopping: $device" Info
		adb -s $device emu kill 2>$null
	}

	Write-Status "All emulators stopped" Success
}

function Full-Deploy {
	Write-Host ""
	Write-Status "Running full rebuild and deploy..." Highlight
	& "$PSScriptRoot\deploy-android.ps1" -rebuild
}

# Main loop
while ($true) {
	if ($command) {
		$choice = $command
		$command = ""
	}
	else {
		Show-Menu
		$choice = Read-Host "Select option"
	}

	Write-Host ""

	switch ($choice) {
		"1" { Deploy-Quick; Read-Host "Press Enter to continue" }
		"2" { View-Logs }
		"3" { Uninstall-App; Read-Host "Press Enter to continue" }
		"4" { Clear-AppData; Read-Host "Press Enter to continue" }
		"5" { Show-DeviceInfo; Read-Host "Press Enter to continue" }
		"6" { Restart-ADB; Read-Host "Press Enter to continue" }
		"7" { Stop-Emulator; Read-Host "Press Enter to continue" }
		"8" { Full-Deploy; Read-Host "Press Enter to continue" }
		"0" { Write-Status "Exiting..." Info; exit 0 }
		default { Write-Status "Invalid option. Please try again." Error; Read-Host "Press Enter to continue" }
	}

	Write-Host ""
}
