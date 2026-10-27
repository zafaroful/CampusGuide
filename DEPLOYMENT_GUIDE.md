# 🚀 CampusGuide Android Deployment Scripts

Automated PowerShell scripts for deploying and testing the CampusGuide MAUI app on Android emulator.

## 📦 What's Included

### 1. **deploy-android.ps1** - Complete Deployment Pipeline
Full-featured deployment script with:
- ✓ Android SDK verification
- ✓ Emulator detection and startup
- ✓ APK build (clean or incremental)
- ✓ Automatic installation
- ✓ App launch
- ✓ Live log streaming
- ✓ Error handling and recovery

### 2. **android-utils.ps1** - Quick Utilities Menu
Interactive menu with quick commands:
- Deploy & run
- View logs
- Uninstall app
- Clear app data
- Device info
- Restart ADB
- Manage emulators

## 🎯 Quick Start

### First Time Setup

1. **Open PowerShell** in your project's root directory:
   ```powershell
   cd "C:\Subject Semester Degree\SEM 7\CSNB5123(MAD)\Project\CampusGuide"
   ```

2. **Allow script execution** (one-time setup):
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

3. **Run the main deployment script**:
   ```powershell
   .\deploy-android.ps1
   ```

### Deploy to Emulator

**Option A: Interactive (Easiest)**
```powershell
.\deploy-android.ps1
```
- Script will list available emulators
- Select one to deploy
- Watch the build and deployment process
- Choose to view logs at the end

**Option B: Specify Emulator**
```powershell
.\deploy-android.ps1 -emulatorName "Pixel_5_API_34"
```

**Option C: Clean Rebuild**
```powershell
.\deploy-android.ps1 -rebuild
```
- Useful when having build cache issues

**Option D: Uninstall Before Installing**
```powershell
.\deploy-android.ps1 -uninstall
```

**Option E: View Logs Only**
```powershell
.\deploy-android.ps1 -logonly
```
- Skips build/deploy
- Only streams logs from running app

### Quick Utilities Menu

For a menu of common operations:
```powershell
.\android-utils.ps1
```

Then select from:
1. Deploy & Run
2. Logs Only
3. Uninstall App
4. Clear App Data
5. Device Info
6. Restart ADB
7. Stop Emulator
8. Full Deploy

## 📊 Script Features

### deploy-android.ps1

| Feature | Description |
|---------|-------------|
| **Environment Check** | Verifies Android SDK, ADB, emulator availability |
| **Build Optimization** | Incremental or clean builds with dependency resolution |
| **Error Recovery** | Graceful failure handling with detailed error messages |
| **Device Management** | Auto-detect emulators, interactive selection |
| **Progress Tracking** | Real-time status updates with timestamps and colors |
| **Log Streaming** | Colored log output with exception highlighting |
| **Help System** | Built-in help with examples |

### android-utils.ps1

| Feature | Description |
|---------|-------------|
| **Quick Deploy** | One-command build and launch |
| **Live Logs** | Stream app logs with highlighted errors |
| **App Management** | Uninstall, clear data, manage lifecycle |
| **Device Info** | Show connected devices, API level, storage |
| **ADB Utilities** | Restart ADB, stop emulators |
| **Interactive Menu** | Repeat operations without script restarts |

## 📋 Common Workflows

### Workflow 1: Deploy and Test
```powershell
.\deploy-android.ps1
# Selects emulator → Builds → Installs → Launches → Shows logs
```

### Workflow 2: Quick Rebuild After Code Changes
```powershell
.\deploy-android.ps1 -emulatorName "Pixel_5_API_34"
```

### Workflow 3: Full Clean Deploy
```powershell
.\deploy-android.ps1 -rebuild -uninstall
# Cleans bin/obj → Builds fresh → Uninstalls old → Installs new
```

### Workflow 4: Debugging Session
```powershell
# Terminal 1: Deploy and view logs
.\deploy-android.ps1 -logonly

# Terminal 2: Work on code and rebuild
dotnet build CampusGuide/CampusGuide.csproj -c Debug -f net10.0-android
```

### Workflow 5: Device Diagnostics
```powershell
.\android-utils.ps1
# Select option 5 for device info
# Select option 6 to restart ADB if having issues
```

## ⚙️ Configuration

### Modify Package ID
Edit both script files and change:
```powershell
$packageId = "com.companyname.campusguide"
```

### Modify Main Activity
Edit both scripts and change:
```powershell
$mainActivity = "com.companyname.campusguide.MainActivity"
```

### Change Build Configuration
In `deploy-android.ps1`, modify:
```powershell
-c Debug  # Change to Release for production
-f net10.0-android  # Change target framework if needed
```

## 🔧 Troubleshooting

### Issue: "Android SDK not found"
**Solution:**
1. Install Android SDK via Visual Studio → Tools → Android → Android SDK Manager
2. Set environment variable:
   ```powershell
   $env:ANDROID_SDK_ROOT = "C:\Users\YourUsername\AppData\Local\Android\Sdk"
   ```

### Issue: "No emulators found"
**Solution:**
1. Create emulator via Visual Studio → Tools → Android → Android Device Manager
2. Or use emulator CLI:
   ```powershell
   cd "$env:ANDROID_SDK_ROOT\tools"
   .\android.bat avd
   ```

### Issue: "Build failed - Compilation errors"
**Solution:**
1. Run clean build:
   ```powershell
   .\deploy-android.ps1 -rebuild
   ```
2. Check Visual Studio output for specific errors
3. Ensure .NET MAUI workload is installed:
   ```powershell
   dotnet workload restore
   ```

### Issue: "Installation failed"
**Solution:**
1. Restart ADB:
   ```powershell
   .\android-utils.ps1  # Option 6
   ```
2. Clear app:
   ```powershell
   adb uninstall com.companyname.campusguide
   ```
3. Restart emulator

### Issue: "App crashes on launch"
**Solution:**
1. View logs to find exception:
   ```powershell
   .\deploy-android.ps1 -logonly
   ```
2. Look for stack traces and fix the underlying issue
3. Redeploy when ready

### Issue: "Emulator won't start"
**Solution:**
1. Restart ADB and emulator:
   ```powershell
   .\android-utils.ps1  # Option 6 then 7
   ```
2. Manually start via Android Device Manager
3. Check system resources (RAM, disk space)

## 📚 Manual Commands Reference

If you prefer not to use the scripts, here are the equivalent manual commands:

```powershell
# List available devices/emulators
adb devices

# Start specific emulator
emulator -avd Pixel_5_API_34

# Build APK
dotnet build CampusGuide/CampusGuide.csproj -c Debug -f net10.0-android

# Install APK
adb install -r "bin\Debug\net10.0-android\com.companyname.campusguide-debug.apk"

# Launch app
adb shell am start -n com.companyname.campusguide/.MainActivity

# View logs
adb logcat

# Uninstall app
adb uninstall com.companyname.campusguide

# Clear app data
adb shell pm clear com.companyname.campusguide
```

## 🎓 CampusGuide QR Scanner - Testing Guide

Once deployed, test the QR Scanner feature:

1. **Navigate to QR Scanner:**
   - Open app on emulator
   - Tap Home tab
   - Look for "Scan QR Code" or similar button

2. **Test Scan Button:**
   - Click "Test Scan"
   - Grant camera permission if prompted
   - Watch for success message

3. **Verify Firestore Integration:**
   - App should look up location by ID
   - Should navigate to location detail page
   - Check logs for any errors

4. **Check Logs for Issues:**
   - Run: `.\deploy-android.ps1 -logonly`
   - Look for exceptions or permission errors
   - Report any crashes

## 📞 Support

For issues or questions:
1. Check troubleshooting section
2. Review build output logs
3. Check Android emulator console
4. Test with manual `adb` commands to isolate issues

---

**Last Updated:** 2024
**Target Framework:** .NET 10
**Min Android API:** 31
**Target Android API:** 34
