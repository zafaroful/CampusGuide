# ⚡ QUICK REFERENCE - Google Maps API Key Setup

## 🚀 30-Second Version

```powershell
# 1. Get your SHA-1 fingerprint
.\get-fingerprint.ps1
# → Copy the SHA-1 value shown

# 2. Create API key at console.cloud.google.com
#    - New Project → Enable "Maps SDK for Android" → Create API Key
#    - Restrict it: Android app + your SHA-1 + Maps SDK only
#    - Copy the AIzaSy... key

# 3. Update manifest
.\update-api-key.ps1 -ApiKey "AIzaSy_PASTE_HERE"

# 4. Rebuild and test
cd CampusGuide
dotnet clean -f net10.0-android
dotnet build -f net10.0-android -c Debug
dotnet run -f net10.0-android -c Debug --no-build
```

---

## 📋 Checklist

- [ ] Run `.\get-fingerprint.ps1` and copy SHA-1
- [ ] Navigate to [console.cloud.google.com](https://console.cloud.google.com)
- [ ] Create new project "CampusGuide"
- [ ] Enable "Maps SDK for Android"
- [ ] Create API Key in Credentials
- [ ] Restrict key: Android app + SHA-1 + Maps SDK
- [ ] Copy API key (AIzaSy...)
- [ ] Run `.\update-api-key.ps1 -ApiKey "..."`
- [ ] Clean and rebuild project
- [ ] Test on emulator
- [ ] ✅ Verify app doesn't crash

---

## 🔧 Troubleshooting One-Liner

```powershell
# Check the manifest has the key
Get-Content CampusGuide/Platforms/Android/AndroidManifest.xml | Select-String "API_KEY"

# Look for Google Maps errors in logcat
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" logcat -d | Select-String "API key|IllegalStateException"

# Verify process is still running (not crashed)
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" shell "ps" | Select-String "campusguide"
```

---

## ❌ Common Mistakes

| ❌ | ✅ |
|---|---|
| Forgetting colons in fingerprint | Use fingerprint without colons: `AB:CD:...` → `ABCD...` |
| API key doesn't start with AIzaSy | Check console.cloud.google.com for correct key |
| `<meta-data>` outside `<application>` tag | Must be INSIDE `<application>...</application>` |
| Using release fingerprint for debug | Use SHA-1 from debug keystore for dev builds |
| Not enabling Maps SDK for Android | Must explicitly enable in APIs & Services → Library |
| Rebuilding without clean | Use `dotnet clean -f net10.0-android` first |

---

## 📱 What to Verify After Setup

| Test | Expected | Pass? |
|------|----------|-------|
| App launches | Shows splash → Home screen | [ ] |
| No immediate crash | App stays open > 5 seconds | [ ] |
| Home visible | See "Welcome to CampusGuide" | [ ] |
| Buttons work | Can click navigation buttons | [ ] |
| Map page loads | No "API key not found" error | [ ] |
| Navigation works | Can route between pages | [ ] |
| Debug output | See "OnStart completed" in output | [ ] |

---

## 📞 When Things Go Wrong

```
ERROR: "API key not found"
├─ Check: <meta-data> inside <application> tag?
├─ Check: API key format (AIzaSy...)?
└─ Fix: Run update-api-key.ps1 again

ERROR: "Invalid API key"
├─ Check: Key matches exactly from console.cloud.google.com?
├─ Check: Maps SDK for Android enabled?
└─ Fix: Regenerate new key

ERROR: "Keystore not found"
├─ Fix: Build project first (creates keystore)
└─ Retry: .\get-fingerprint.ps1

ERROR: App still crashes
├─ Check: & "adb logcat -d | findstr API_KEY"
├─ Check: App process running? (ps | findstr campusguide)
└─ Report: Share logcat output + AndroidManifest.xml
```

---

## 💾 Key Commands Reference

```powershell
# Extract fingerprint
.\get-fingerprint.ps1

# Extract for release build
.\get-fingerprint.ps1 -BuildType release

# Update API key
.\update-api-key.ps1 -ApiKey "AIzaSy_YOUR_KEY"

# Build project
dotnet build -f net10.0-android -c Debug

# Clean and rebuild
dotnet clean -f net10.0-android
dotnet build -f net10.0-android -c Debug

# Deploy to emulator
dotnet run -f net10.0-android -c Debug --no-build

# View Android error logs
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" logcat -d | Select-String "Error|Fatal"
```

---

## ⏱️ Time Breakdown

| Task | Time |
|------|------|
| Extract fingerprint | 1 min |
| Create Google Cloud project | 2 min |
| Enable Maps SDK | 1 min |
| Create API key | 2 min |
| Update AndroidManifest.xml | 1 min |
| Build & test | 5-7 min |
| **Total** | **~15 min** |

---

## 📚 Full Documentation Available

- **`CRASH_FIX_SUMMARY.md`** - Full story & complete details
- **`API_KEY_SETUP_README.md`** - Complete setup guide
- **`GOOGLE_MAPS_SETUP.md`** - Step-by-step with detailed explanations

---

**Last Updated:** 2026-11-03 | **Status:** ✅ Ready to configure
