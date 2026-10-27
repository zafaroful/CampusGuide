#!/usr/bin/env pwsh
<#
.SYNOPSIS
	CampusGuide Development Toolkit - Quick Launch Menu
.DESCRIPTION
	Central command center for common development tasks
#>

function Show-MainMenu {
	Clear-Host
	Write-Host @"
╔════════════════════════════════════════════════════════════════════════╗
║                                                                        ║
║              CampusGuide Android Development Toolkit                   ║
║                      .NET MAUI App (net10.0)                           ║
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝

┌─ 🚀 DEPLOYMENT ──────────────────────────────────────────────────────┐
│                                                                        │
│  1  Deploy to Emulator      Build, install, and run on emulator      │
│  2  Quick Deploy Menu       Fast access to common deploy tasks       │
│  3  View Live Logs          Stream app logs in real-time             │
│                                                                        │
├─ 🛠️  EMULATOR MANAGEMENT ─────────────────────────────────────────┐
│                                                                        │
│  4  Manage Emulators        Create, start, wipe, delete emulators   │
│  5  Device Info             Show connected devices and specs         │
│                                                                        │
├─ 🔧 BUILD & DEBUG ───────────────────────────────────────────────┐
│                                                                        │
│  6  Build Debug APK         Compile APK for debugging                │
│  7  Rebuild (Clean)         Full clean rebuild                       │
│  8  Run Unit Tests          Execute test suite (when available)      │
│                                                                        │
├─ 📚 DOCUMENTATION & Help ────────────────────────────────────────┐
│                                                                        │
│  9  View Deployment Guide   Open DEPLOYMENT_GUIDE.md                 │
│  10 Show ADB Commands       Useful adb command reference             │
│  11 Troubleshooting         Common issues and solutions              │
│                                                                        │
├─ 💾 UTILITIES ───────────────────────────────────────────────────┐
│                                                                        │
│  12 Open Project Directory  Launch explorer in project root          │
│  13 Open Logs Directory     View build and deployment logs           │
│  14 System Info             Show development environment details     │
│                                                                        │
│  0  Exit                                                              │
│                                                                        │
└─────────────────────────────────────────────────────────────────────┘

" -ForegroundColor Cyan
}

function Write-Status {
	param([string]$message, [string]$type = "Info")
	$colors = @{ Success = "Green"; Error = "Red"; Warning = "Yellow"; Info = "Cyan"; Highlight = "Magenta" }
	$color = $colors[$type]
	Write-Host "  [$type] " -NoNewline -ForegroundColor $color
	Write-Host $message
}

$projectRoot = "$PSScriptRoot"

while ($true) {
	Show-MainMenu
	$choice = Read-Host "▶ Select option"
	Write-Host ""

	switch ($choice) {
		"1" {
			Write-Status "Launching deployment script..." Info
			& "$projectRoot\deploy-android.ps1"
			Write-Host ""
			Read-Host "Press Enter to return to main menu"
		}

		"2" {
			Write-Status "Launching quick utilities menu..." Info
			& "$projectRoot\android-utils.ps1"
		}

		"3" {
			Write-Status "Streaming live logs (press Ctrl+C to stop)..." Info
			Write-Host ""
			$device = (adb devices | Select-Object -Skip 1 | Where-Object { $_ -match "emulator" } | Select-Object -First 1)
			if ($device) {
				$deviceId = ($device -split "\s+" | Select-Object -First 1).Trim()
				adb -s $deviceId logcat
			}
			else {
				Write-Status "No running emulator found" Error
				Write-Host "Start one with: option 1 or 4"
			}
		}

		"4" {
			Write-Status "Launching emulator manager..." Info
			& "$projectRoot\emulator-manager.ps1"
		}

		"5" {
			Write-Host ""
			Write-Status "Connected Devices/Emulators" Highlight
			Write-Host ""
			adb devices
			Write-Host ""
			Read-Host "Press Enter to continue"
		}

		"6" {
			Write-Host ""
			Write-Status "Building Debug APK..." Info
			dotnet build "$projectRoot\CampusGuide\CampusGuide.csproj" -c Debug -f net10.0-android
			Write-Host ""
			Read-Host "Press Enter to continue"
		}

		"7" {
			Write-Host ""
			Write-Status "Running clean rebuild..." Warning
			Remove-Item "$projectRoot\CampusGuide\bin" -Recurse -Force -ErrorAction SilentlyContinue
			Remove-Item "$projectRoot\CampusGuide\obj" -Recurse -Force -ErrorAction SilentlyContinue
			Write-Status "Cleaned build directories" Success
			Write-Host ""
			Write-Status "Building..." Info
			dotnet build "$projectRoot\CampusGuide\CampusGuide.csproj" -c Debug -f net10.0-android
			Write-Host ""
			Read-Host "Press Enter to continue"
		}

		"8" {
			Write-Host ""
			Write-Status "Unit tests not yet configured" Warning
			Write-Host ""
			Write-Host "To add tests in the future, create a test project:"
			Write-Host "  dotnet new nunit -n CampusGuide.Tests"
			Write-Host ""
			Read-Host "Press Enter to continue"
		}

		"9" {
			if (Test-Path "$projectRoot\DEPLOYMENT_GUIDE.md") {
				Write-Status "Opening DEPLOYMENT_GUIDE.md..." Info
				& notepad "$projectRoot\DEPLOYMENT_GUIDE.md"
			}
			else {
				Write-Status "DEPLOYMENT_GUIDE.md not found" Error
			}
		}

		"10" {
			Write-Host @"
╔════════════════════════════════════════════════════════════════════════╗
║                     ADB Command Reference                              ║
╚════════════════════════════════════════════════════════════════════════╝

Device Management:
  adb devices                                List connected devices
  adb devices -l                             List with details
  adb connect <host>:<port>                  Connect to remote device
  adb disconnect <host>:<port>               Disconnect from device

Package Management:
  adb install <path-to-apk>                  Install app
  adb install -r <path-to-apk>               Reinstall app
  adb uninstall <package-id>                 Uninstall app
  adb shell pm clear <package-id>            Clear app data

Logging & Debugging:
  adb logcat                                 View system logs
  adb logcat | grep <tag>                    Filter logs by tag
  adb logcat -c                              Clear logs
  adb logcat -d                               Dump and exit
  adb logcat -f <file>                       Save logs to file

App Execution:
  adb shell am start -n <pkg/activity>       Start activity
  adb shell am kill-all                      Kill all apps
  adb shell am crash <package-id>            Crash an app (debugging)

File Transfer:
  adb push <local> <remote>                  Push file to device
  adb pull <remote> <local>                  Pull file from device

Shell Commands:
  adb shell                                  Interactive shell
  adb shell ls                               List directories
  adb shell df                               Disk usage
  adb shell ps                               Running processes

Forward & Port Mapping:
  adb forward tcp:8000 tcp:8000              Forward ports
  adb reverse tcp:8000 tcp:8000              Reverse ports

Emulator Specific:
  emulator -list-avds                        List AVD definitions
  emulator -avd <name>                       Start specific emulator
  emulator -avd <name> -wipe-data            Start with clean data
  adb emu kill                               Stop emulator

Example Workflows:
  # Deploy and view logs
  adb install -r app.apk
  adb shell am start -n com.example/.MainActivity
  adb logcat | grep "com.example"

  # Debug specific issues
  adb logcat -c
  adb shell am start -n com.example/.MainActivity
  adb logcat | grep "Exception"

" -ForegroundColor Cyan
			Write-Host ""
			Read-Host "Press Enter to continue"
		}

		"11" {
			Write-Host @"
╔════════════════════════════════════════════════════════════════════════╗
║                   Troubleshooting Guide                                ║
╚════════════════════════════════════════════════════════════════════════╝

❌ BUILD FAILURES

Q: "Build failed - Compilation errors"
A: 
  1. Run clean rebuild: .\dev-toolkit.ps1 → option 7
  2. Restore workloads: dotnet workload restore
  3. Check file for syntax errors (check IDE)

Q: "Package not found"
A:
  1. Run: dotnet restore
  2. Check NuGet.config
  3. Verify internet connection

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❌ DEPLOYMENT FAILURES

Q: "No devices found"
A:
  1. Verify emulator is running: adb devices
  2. Restart ADB: adb kill-server && adb start-server
  3. Create new emulator: .\dev-toolkit.ps1 → option 4

Q: "Installation failed"
A:
  1. Uninstall old version: adb uninstall com.companyname.campusguide
  2. Clear storage: adb shell pm clear com.companyname.campusguide
  3. Restart emulator
  4. Retry install

Q: "APK not found after build"
A:
  1. Check build succeeded: dotnet build -c Debug -f net10.0-android
  2. Look in: CampusGuide/bin/Debug/net10.0-android/
  3. Check for files matching: *-debug.apk

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❌ RUNTIME FAILURES

Q: "App crashes on launch"
A:
  1. View logs: adb logcat
  2. Look for exception stack trace
  3. Check logcat for specific error
  4. Common issues:
	 - Missing permission declaration
	 - Service not initialized
	 - Invalid activity name

Q: "Permission denied errors"
A:
  1. Verify AndroidManifest.xml has permission
  2. Grant permission at runtime in code
  3. Check logs for which permission is missing
  4. Re-grant permission in Settings → Apps

Q: "Camera not working"
A:
  1. Grant camera permission in emulator settings
  2. Check: PermissionHelper requests at runtime
  3. Verify AndroidManifest.xml has CAMERA permission
  4. Check logs for camera initialization errors

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚠️  EMULATOR ISSUES

Q: "Emulator won't start"
A:
  1. Restart ADB: .\dev-toolkit.ps1 → option 5
  2. Manually start via Android Device Manager
  3. Check system RAM: need ~2 GB available
  4. Restart emulator process

Q: "Emulator very slow"
A:
  1. Close other applications
  2. Enable GPU acceleration in emulator settings
  3. Increase allocated RAM (2 GB recommended)
  4. Use SSD for emulator storage
  5. Try different API level (34 is fastest)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔧 COMMON FIXES

# Fix 1: ADB connection issues
adb kill-server
adb start-server
adb devices

# Fix 2: Emulator stuck
adb -e emu kill
emulator -avd <name>

# Fix 3: Build cache corruption
Remove-Item CampusGuide/bin -Recurse -Force
Remove-Item CampusGuide/obj -Recurse -Force
dotnet build

# Fix 4: Deep clean
dotnet clean CampusGuide/CampusGuide.csproj
dotnet restore
dotnet build -c Debug -f net10.0-android

" -ForegroundColor Yellow
			Write-Host ""
			Read-Host "Press Enter to continue"
		}

		"12" {
			Write-Status "Opening project directory..." Info
			explorer $projectRoot
		}

		"13" {
			$logsPath = "$projectRoot\CampusGuide\bin\Debug\net10.0-android"
			if (Test-Path $logsPath) {
				Write-Status "Opening logs directory..." Info
				explorer $logsPath
			}
			else {
				Write-Status "Logs directory doesn't exist yet (need to build first)" Warning
			}
		}

		"14" {
			Write-Host ""
			Write-Status "System Information" Highlight
			Write-Host ""

			Write-Host "  .NET SDK:" -NoNewline -ForegroundColor Cyan
			dotnet --version

			Write-Host "  MAUI Workload:" -NoNewline -ForegroundColor Cyan
			dotnet workload list 2>&1 | Select-String "maui" | Select-Object -First 1

			Write-Host "  Android SDK:" -NoNewline -ForegroundColor Cyan
			if (Test-Path "$env:USERPROFILE\AppData\Local\Android\Sdk") {
				Write-Host "Installed ✓"
			}
			else {
				Write-Host "Not found ✗" -ForegroundColor Red
			}

			Write-Host "  Java:" -NoNewline -ForegroundColor Cyan
			if (Get-Command java -ErrorAction SilentlyContinue) {
				java -version 2>&1 | Select-Object -First 1
			}
			else {
				Write-Host "Not found ✗" -ForegroundColor Red
			}

			Write-Host ""
			Write-Host "  Project Target: net10.0-android" -ForegroundColor Green
			Write-Host "  Min API Level: 31" -ForegroundColor Green
			Write-Host "  Target API Level: 34" -ForegroundColor Green
			Write-Host ""
			Read-Host "Press Enter to continue"
		}

		"0" {
			Write-Host ""
			Write-Status "Goodbye!" Success
			exit 0
		}

		default {
			Write-Status "Invalid option" Error
			Read-Host "Press Enter to try again"
		}
	}
}
