#!/usr/bin/env pwsh
<#
.SYNOPSIS
	Android Emulator Management Script for CampusGuide
.DESCRIPTION
	Create, list, start, and manage Android emulators
#>

param(
	[string]$command = "",
	[string]$emulatorName = ""
)

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
║           CampusGuide Android Emulator Manager                         ║
╚════════════════════════════════════════════════════════════════════════╝

Emulator Management Commands:

  1  Create New Emulator      Set up a new Android virtual device
  2  List Emulators           Show all available emulators
  3  Start Emulator           Boot up an emulator
  4  Wipe Emulator            Reset emulator to factory defaults
  5  Delete Emulator          Remove an emulator
  6  Show Running             List currently running emulators
  7  Emulator Settings        Configure emulator options

  0  Exit

" -ForegroundColor Cyan
}

function Create-Emulator {
	Write-Host ""
	Write-Status "Android Emulator Creation" Highlight
	Write-Host ""

	$name = Read-Host "Emulator name (e.g., Pixel_5_API_34)"
	if ([string]::IsNullOrWhiteSpace($name)) {
		Write-Status "Invalid name" Error
		return
	}

	Write-Host ""
	Write-Host "Available System Images:" -ForegroundColor Cyan
	Write-Host "  • google_apis-34         (Android 14 - Recommended)"
	Write-Host "  • google_apis-33         (Android 13)"
	Write-Host "  • google_apis-32         (Android 12)"
	Write-Host "  • google_apis-31         (Android 12 with APIs)"
	Write-Host ""

	$apiLevel = Read-Host "API Level (e.g., 34)"
	if ([string]::IsNullOrWhiteSpace($apiLevel)) {
		$apiLevel = "34"
	}

	$systemImage = "system-images;android-$apiLevel;google_apis;x86_64"

	Write-Host ""
	Write-Host "Available Device Types:" -ForegroundColor Cyan
	Write-Host "  • pixel_5        (Recommended)"
	Write-Host "  • pixel_4"
	Write-Host "  • pixel_3"
	Write-Host "  • pixel_2"
	Write-Host ""

	$device = Read-Host "Device type (e.g., pixel_5)"
	if ([string]::IsNullOrWhiteSpace($device)) {
		$device = "pixel_5"
	}

	Write-Status "Creating emulator: $name (API $apiLevel)" Info
	Write-Status "This may take a few minutes..." Warning
	Write-Host ""

	# Use avdmanager to create emulator
	$avdPath = "$env:USERPROFILE\.android\avd"

	# First install the system image if needed
	Write-Status "Ensuring system image is installed..." Info
	& sdkmanager $systemImage 2>&1 | Out-Null

	# Create the emulator
	Write-Status "Creating virtual device..." Info
	$output = & avdmanager create avd -n $name -d $device -k $systemImage -f 2>&1

	if ($LASTEXITCODE -eq 0 -or $output -match "Created") {
		Write-Status "Emulator created successfully!" Success
		Write-Host ""
		Write-Host "Next steps:" -ForegroundColor Cyan
		Write-Host "  1. Start emulator: emulator -avd $name"
		Write-Host "  2. Deploy app: .\deploy-android.ps1 -emulatorName '$name'"
	}
	else {
		Write-Status "Failed to create emulator" Error
		Write-Host $output -ForegroundColor Red
	}
}

function List-Emulators {
	Write-Host ""
	Write-Status "Available Emulators" Highlight
	Write-Host ""

	if (Get-Command avdmanager -ErrorAction SilentlyContinue) {
		$emulators = @(avdmanager list avd 2>&1 | Where-Object { $_ -match "Name:" })

		if ($emulators.Count -eq 0) {
			Write-Status "No emulators found" Warning
			Write-Host "Create one with: .\emulator-manager.ps1 (option 1)"
			return
		}

		$emulators | ForEach-Object {
			$parts = $_ -split ":"
			if ($parts.Count -ge 2) {
				$name = $parts[1].Trim()
				Write-Host "  ✓ $name" -ForegroundColor Green
			}
		}
	}
	else {
		Write-Status "avdmanager not found in PATH" Error
	}

	Write-Host ""
	Write-Status "Running Emulators" Info
	adb devices | Select-Object -Skip 1 | Where-Object { $_ -match "emulator" } | ForEach-Object {
		$parts = $_ -split "\s+"
		$device = $parts[0]
		$status = $parts[1]
		Write-Host "  ◉ $device ($status)" -ForegroundColor Cyan
	}
}

function Start-Emulator {
	Write-Host ""
	$name = Read-Host "Enter emulator name to start (e.g., Pixel_5_API_34)"

	if ([string]::IsNullOrWhiteSpace($name)) {
		Write-Status "Invalid name" Error
		return
	}

	Write-Status "Starting emulator: $name" Info
	Write-Status "This may take 30-60 seconds..." Warning
	Write-Host ""

	Start-Process emulator -ArgumentList "-avd", $name -RedirectStandardOutput $null -RedirectStandardError $null -WindowStyle Hidden

	# Wait for device to be ready
	$maxWait = 120
	$elapsed = 0
	while ($elapsed -lt $maxWait) {
		$devices = adb devices | Select-String $name
		if ($devices) {
			$bootComplete = adb shell getprop dev.bootcomplete 2>$null
			if ($bootComplete -eq "1") {
				Write-Status "Emulator ready!" Success
				adb devices | Select-Object -Skip 1 | Where-Object { $_ -match $name }
				return
			}
		}
		Write-Host "." -NoNewline -ForegroundColor Yellow
		Start-Sleep -Seconds 2
		$elapsed += 2
	}

	Write-Status "Emulator startup timeout" Error
}

function Wipe-Emulator {
	Write-Host ""
	Write-Status "Wipe Emulator (Factory Reset)" Warning

	$name = Read-Host "Enter emulator name to wipe"
	if ([string]::IsNullOrWhiteSpace($name)) {
		Write-Status "Invalid name" Error
		return
	}

	Write-Status "Wiping emulator: $name" Warning
	Write-Status "This will reset all data, apps, and settings" Warning

	$confirm = Read-Host "Are you sure? (yes/no)"
	if ($confirm -ne "yes") {
		Write-Status "Cancelled" Info
		return
	}

	# Use emulator command to wipe
	Write-Status "Please wait while emulator is wiped..." Warning
	& emulator -avd $name -wipe-data -no-window 2>&1 | Out-Null

	Write-Status "Emulator wiped successfully!" Success
}

function Delete-Emulator {
	Write-Host ""
	Write-Status "Delete Emulator" Warning

	$name = Read-Host "Enter emulator name to delete"
	if ([string]::IsNullOrWhiteSpace($name)) {
		Write-Status "Invalid name" Error
		return
	}

	Write-Status "Deleting emulator: $name" Warning

	$confirm = Read-Host "Are you sure? This cannot be undone (yes/no)"
	if ($confirm -ne "yes") {
		Write-Status "Cancelled" Info
		return
	}

	if (Get-Command avdmanager -ErrorAction SilentlyContinue) {
		& avdmanager delete avd -n $name 2>&1 | Out-Null

		if ($LASTEXITCODE -eq 0) {
			Write-Status "Emulator deleted successfully!" Success
		}
		else {
			Write-Status "Failed to delete emulator" Error
		}
	}
	else {
		Write-Status "avdmanager not found" Error
	}
}

function Show-Running {
	Write-Host ""
	Write-Status "Running Emulators/Devices" Highlight
	Write-Host ""

	$devices = adb devices

	if ($devices -match "emulator") {
		Write-Host $devices -ForegroundColor Cyan
	}
	else {
		Write-Status "No running emulators found" Info
		Write-Host ""
		Write-Host "To start an emulator:" -ForegroundColor Yellow
		Write-Host "  .\emulator-manager.ps1 (option 3)"
	}
}

function Show-Settings {
	Write-Host ""
	Write-Status "Recommended Emulator Settings" Highlight
	Write-Host ""
	Write-Host "For optimal CampusGuide testing, use these settings:" -ForegroundColor Cyan
	Write-Host ""
	Write-Host "Device: Pixel 5 (or higher)" -ForegroundColor Green
	Write-Host "Android API: 34 (Android 14)" -ForegroundColor Green
	Write-Host "RAM: 2048 MB (or more)" -ForegroundColor Green
	Write-Host "VM Heap: 512 MB" -ForegroundColor Green
	Write-Host "Internal Storage: 2 GB" -ForegroundColor Green
	Write-Host "SD Card: 512 MB" -ForegroundColor Green
	Write-Host ""
	Write-Host "Performance:" -ForegroundColor Cyan
	Write-Host "  • Use KVM on Linux / Hyper-V on Windows for HW acceleration"
	Write-Host "  • Enable GPU emulation for better graphics"
	Write-Host "  • Disk cache may slow things down - disable if needed"
	Write-Host ""
	Write-Host "Network:" -ForegroundColor Cyan
	Write-Host "  • Speed: Full 56K"
	Write-Host "  • Latency: None"
	Write-Host ""
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
		"1" { Create-Emulator }
		"2" { List-Emulators }
		"3" { Start-Emulator }
		"4" { Wipe-Emulator }
		"5" { Delete-Emulator }
		"6" { Show-Running }
		"7" { Show-Settings }
		"0" { Write-Status "Exiting..." Info; exit 0 }
		default { Write-Status "Invalid option" Error }
	}

	Write-Host ""
	Read-Host "Press Enter to continue"
	Clear-Host
}
