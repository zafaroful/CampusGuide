# ⚠️ No Android Emulator Available

The build failed because no Android emulator is currently running.

## Quick Fix: Start an Android Emulator

### **Option 1: Using Android Studio (Easiest)**
1. Open **Android Studio**
2. Click **Device Manager** (or Tools → Device Manager)
3. You should see available AVDs (Android Virtual Devices)
4. Click the **Play ▶️ button** next to an emulator
   - Common one: `pixel_7_-_api_36_0` or similar
5. Wait for emulator to fully boot (2-3 minutes, watch for home screen)
6. Once loaded, emulator is ready for deployment

### **Option 2: Using Command Line**
If you know your AVD name:
```powershell
# List available AVDs
$env:ANDROID_SDK_ROOT + "\emulator\emulator.exe" -list-avds

# Launch an emulator (replace "Pixel_7_API_36" with your AVD name)
$env:ANDROID_SDK_ROOT + "\emulator\emulator.exe" -avd Pixel_7_API_36 -no-snapshot-load &
```

### **Option 3: Verify Android SDK Path**
If command line doesn't work:
```powershell
# Check ANDROID_SDK_ROOT
echo $env:ANDROID_SDK_ROOT

# If empty, check common locations:
ls "C:\Android\Sdk"
ls "$env:LOCALAPPDATA\Android\Sdk"
```

---

## After Emulator is Running

Once you see the Android emulator home screen on screen, run:

```powershell
cd "C:\Subject Semester Degree\SEM 7\CSNB5123(MAD)\Project\CampusGuide"
dotnet build -t Install -f net10.0-android -c Debug
```

Or from Visual Studio:
1. Select the running emulator from the device dropdown at the top
2. Click the **Play button** (Run/F5)

---

## Troubleshooting

**Emulator won't start?**
- Android Studio → Settings → System Settings → Android SDK → SDK Tools → Check "Android Emulator" is installed
- Restart Android Studio
- Try creating a new AVD with API 35 or higher

**Still no device shows up?**
- In PowerShell:
  ```powershell
  adb.exe devices
  ```
  - Should show a running emulator with state "device"
  - If showing "offline", restart emulator

**ADB not found?**
```powershell
# Set path to ADB
$env:PATH += ";$env:ANDROID_SDK_ROOT\platform-tools"
adb.exe devices
```

---

## Let me know when:
✅ Emulator is running and you see the Android home screen
Then I can help deploy the app!
