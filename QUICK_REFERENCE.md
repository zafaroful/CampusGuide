# 🚀 Quick Reference - CampusGuide Deployment Scripts

## Start Here

```powershell
# Make scripts executable (one-time setup)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Launch main toolkit
.\dev-toolkit.ps1
```

## Script Overview

| Script | Purpose | Command |
|--------|---------|---------|
| **dev-toolkit.ps1** | Central command center | `.\dev-toolkit.ps1` |
| **deploy-android.ps1** | Full deployment pipeline | `.\deploy-android.ps1` |
| **android-utils.ps1** | Quick utilities menu | `.\android-utils.ps1` |
| **emulator-manager.ps1** | Emulator management | `.\emulator-manager.ps1` |

## Common Workflows

### 🎯 First-Time Setup
```powershell
# 1. Enable scripts
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# 2. Create Android emulator
.\dev-toolkit.ps1          # Menu option 4
# OR
.\emulator-manager.ps1     # Menu option 1

# 3. Deploy app
.\deploy-android.ps1
```

### 🚀 Deploy to Emulator
```powershell
# Quick deploy (interactive)
.\deploy-android.ps1

# Specific emulator
.\deploy-android.ps1 -emulatorName "Pixel_5_API_34"

# Clean rebuild
.\deploy-android.ps1 -rebuild

# Uninstall before install
.\deploy-android.ps1 -uninstall
```

### 👁️ View Logs
```powershell
# Method 1: From main toolkit
.\dev-toolkit.ps1          # Menu option 3

# Method 2: From deployment script
.\deploy-android.ps1 -logonly

# Method 3: Manual
adb logcat
```

### 🔧 Manage Emulators
```powershell
# Interactive menu
.\emulator-manager.ps1

# Or from toolkit
.\dev-toolkit.ps1          # Menu option 4
```

### 🏗️ Build Operations
```powershell
# Build APK
dotnet build CampusGuide/CampusGuide.csproj -c Debug -f net10.0-android

# Clean rebuild
.\dev-toolkit.ps1          # Menu option 7

# From toolkit
.\dev-toolkit.ps1          # Menu option 6
```

## Key Parameters

### deploy-android.ps1
```powershell
-emulatorName "name"   # Specify emulator
-rebuild               # Force clean rebuild
-uninstall            # Uninstall before install
-logonly              # View logs only
-help                 # Show help
```

### Examples
```powershell
# Deploy to specific emulator
.\deploy-android.ps1 -emulatorName "Pixel_5_API_34"

# Clean rebuild and deploy
.\deploy-android.ps1 -rebuild

# Deploy, uninstall old first
.\deploy-android.ps1 -uninstall

# Just view logs
.\deploy-android.ps1 -logonly

# Get help
.\deploy-android.ps1 -help
```

## Essential ADB Commands

```powershell
# Devices
adb devices                            # List devices
adb devices -l                         # List with details

# Install/Uninstall
adb install -r app.apk               # Install APK
adb uninstall com.companyname.campusguide  # Uninstall

# App Management
adb shell am start -n pkg/activity   # Launch app
adb shell pm clear com.companyname   # Clear data

# Logs
adb logcat                            # View logs
adb logcat -c                         # Clear logs
adb logcat | grep "tag"              # Filter logs

# Emulator
emulator -list-avds                   # List emulators
emulator -avd name                    # Start emulator
```

## Troubleshooting Quick Fixes

| Issue | Fix |
|-------|-----|
| Scripts won't run | `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser` |
| No emulator found | Run `.\emulator-manager.ps1` to create one |
| Build fails | `.\dev-toolkit.ps1` → Option 7 (clean rebuild) |
| Installation fails | `adb uninstall com.companyname.campusguide` then retry |
| App crashes | `.\dev-toolkit.ps1` → Option 3 (view logs) |
| Emulator won't start | Restart emulator in Android Device Manager |
| Slow emulator | Enable GPU acceleration in emulator settings |

## File Locations

```
Project Root:
├── deploy-android.ps1          # Main deployment
├── android-utils.ps1           # Quick utilities
├── emulator-manager.ps1        # Emulator management
├── dev-toolkit.ps1             # Central hub
├── DEPLOYMENT_GUIDE.md          # Full documentation
│
└── CampusGuide/
	├── CampusGuide.csproj       # Project file
	├── bin/Debug/net10.0-android/   # Build output
	│   └── *-debug.apk          # Generated APK
	└── Platforms/Android/       # Android-specific files
		├── AndroidManifest.xml
		└── MainActivity.cs
```

## Test the QR Scanner

After deploying:
1. Open app on emulator
2. Navigate to QR Scanner
3. Click "Test Scan" button
4. Verify location lookup works
5. Check logs for any errors

## Next Steps

- ✅ Scripts created
- ⏭️ [Next] Run `.\deploy-android.ps1` to deploy
- ⏭️ Test app functionality
- ⏭️ Review logs for issues
- ⏭️ Iterate and improve

---

**Last Updated:** 2024  
**Target:** .NET 10 (MAUI)  
**Android:** API 31+ (Target 34)
