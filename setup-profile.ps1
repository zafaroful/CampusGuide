#!/usr/bin/env pwsh
<#
.SYNOPSIS
	CampusGuide PowerShell Environment Setup
.DESCRIPTION
	Optional: Add shortcuts to PowerShell profile for quick access
#>

Write-Host @"
╔════════════════════════════════════════════════════════════════════════╗
║          CampusGuide Development Environment Setup                     ║
╚════════════════════════════════════════════════════════════════════════╝

This script will help you set up convenient PowerShell shortcuts.

" -ForegroundColor Cyan

# Check if running as admin
$isAdmin = [Security.Principal.WindowsIdentity]::GetCurrent().Groups -contains 'S-1-5-32-544'

if ($isAdmin) {
	Write-Host "✓ Running as Administrator" -ForegroundColor Green
}
else {
	Write-Host "⚠ Not running as administrator (optional, not required)" -ForegroundColor Yellow
}

Write-Host ""
$setupProfile = Read-Host "Add shortcuts to PowerShell profile? (y/n)"

if ($setupProfile -eq "y" -or $setupProfile -eq "Y") {
	$profilePath = $PROFILE
	$profileDir = Split-Path $profilePath

	# Create profile directory if it doesn't exist
	if (-not (Test-Path $profileDir)) {
		New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
		Write-Host "Created profile directory: $profileDir" -ForegroundColor Green
	}

	# Create or append to profile
	$shortcutContent = @'

# ╔════════════════════════════════════════════════════════════════════════╗
# ║ CampusGuide Development Shortcuts                                      ║
# ╚════════════════════════════════════════════════════════════════════════╝

# Navigate to CampusGuide project
function campus {
	Set-Location "$env:USERPROFILE\Subject Semester Degree\SEM 7\CSNB5123(MAD)\Project\CampusGuide"
}

# Quick deploy
function deploy {
	.\deploy-android.ps1
}

# View logs
function logs {
	adb logcat
}

# Quick utilities menu
function utils {
	.\android-utils.ps1
}

# Emulator manager
function emu {
	.\emulator-manager.ps1
}

# Main toolkit
function toolkit {
	.\dev-toolkit.ps1
}

# List devices
function devices {
	adb devices
}

# ADB shell
function adbsh {
	adb shell
}

# Clear all app data
function clear-campus {
	adb shell pm clear com.companyname.campusguide
}

# Uninstall app
function uninstall-campus {
	adb uninstall com.companyname.campusguide
}

# Restart ADB
function restart-adb {
	adb kill-server
	adb start-server
	Write-Host "ADB restarted" -ForegroundColor Green
}

# View help
function campus-help {
	Write-Host @"
╔════════════════════════════════════════════════════════════════════════╗
║ CampusGuide PowerShell Shortcuts                                       ║
╚════════════════════════════════════════════════════════════════════════╝

Navigation:
  campus                    Go to project directory

Development:
  deploy                    Run full deployment
  deploy -rebuild           Clean rebuild and deploy
  deploy -logonly           View logs only
  logs                      View live logs (Ctrl+C to stop)

Emulator & Device:
  emu                       Emulator manager menu
  devices                   List connected devices
  adbsh                     Android shell prompt
  clear-campus              Clear app data
  uninstall-campus          Uninstall app
  restart-adb               Restart Android Debug Bridge

Utilities:
  utils                     Quick utilities menu
  toolkit                   Main development toolkit
  campus-help               Show this help

Examples:
  campus                    # Jump to project
  deploy                    # Deploy app
  logs                      # Watch real-time logs
  emu                       # Create/manage emulators
  utils                     # Quick operations menu

"@ -ForegroundColor Cyan
}

# Alias shortcuts
Set-Alias -Name cdeploy -Value deploy -Force
Set-Alias -Name cemu -Value emu -Force
Set-Alias -Name cutils -Value utils -Force

Write-Host ""
Write-Host "CampusGuide shortcuts loaded! Type 'campus-help' for details" -ForegroundColor Green
'@

	# Check if profile exists and has our content
	$profileExists = Test-Path $profilePath
	$hasContent = if ($profileExists) { Select-String "CampusGuide Development Shortcuts" $profilePath -ErrorAction SilentlyContinue } else { $false }

	if (-not $hasContent) {
		Add-Content -Path $profilePath -Value $shortcutContent
		Write-Host "✓ Added shortcuts to PowerShell profile" -ForegroundColor Green
		Write-Host "  Location: $profilePath" -ForegroundColor Cyan
	}
	else {
		Write-Host "✓ Shortcuts already in profile (no changes needed)" -ForegroundColor Green
	}

	# Reload profile
	Write-Host ""
	$reload = Read-Host "Reload PowerShell profile now? (y/n)"
	if ($reload -eq "y" -or $reload -eq "Y") {
		& $PROFILE
		Write-Host "✓ Profile reloaded" -ForegroundColor Green
	}
	else {
		Write-Host "⚠ Restart PowerShell to load shortcuts" -ForegroundColor Yellow
	}
}

Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: .\deploy-android.ps1" -ForegroundColor White
Write-Host "  2. Or: .\dev-toolkit.ps1" -ForegroundColor White
Write-Host "  3. Or: campus (then toolkit)" -ForegroundColor White
Write-Host ""
