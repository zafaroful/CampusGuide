# CampusGuide Setup - Currently In Progress

## 🔄 Current Status

**What's happening right now:**
- ✅ Android project build started (background process)
- ✅ Helper scripts created
- ⏳ Waiting for debug keystore to be generated...

---

## 📋 Complete Setup Checklist

### Phase 1: Android Build *(In Progress)*
- [ ] Run: `dotnet build -f net10.0-android -c Debug`
- [ ] Creates: `~/.android/debug.keystore`
- [ ] Expected time: 1-2 minutes
- ⏳ **CURRENT: Build in progress...**

### Phase 2: Extract Your SHA-1 *(Next)*
Once build completes, run:
```powershell
cd "C:\Subject Semester Degree\SEM 7\CSNB5123(MAD)\Project\CampusGuide"
.\extract-sha1.ps1
```

This will display and save your certificate's SHA-1 fingerprint.

### Phase 3: Create Google Cloud Project *(Manual - 5 min)*

1. Go to: https://console.cloud.google.com
2. Click **Select a Project** → **New Project**
3. Name: `CampusGuide`
4. Click **Create**
5. Wait for project initialization

### Phase 4: Enable Maps SDK *(Manual - 2 min)*

1. In Google Cloud Console, go to: **APIs & Services** →  **Library**
2. Search: `Maps SDK for Android`
3. Click the result
4. Click **Enable**
5. Wait for confirmation

### Phase 5: Create API Key *(Manual - 2 min)*

1. Go to: **APIs & Services** → **Credentials**
2. Click **+ Create Credentials** → **API Key**
3. Copy the key (starts with `AIzaSy`)
4. Save it somewhere safe

### Phase 6: Restrict API Key *(Manual - 3 min, Optional but Recommended)*

1. Click on your API key to edit it
2. Set **Application restrictions** → **Android apps**
3. Add your package name: `com.companyname.campusguide`
4. Add your SHA-1 (from Phase 2)
5. Set **API restrictions** → **Maps SDK for Android**
6. Click **Save**

### Phase 7: Update AndroidManifest.xml *(Automated - 1 min)*

I'll update this file automatically once you have your API key.

### Phase 8: Rebuild & Deploy *(Automated - 5-7 min)*

```powershell
cd CampusGuide
dotnet clean -f net10.0-android
dotnet build -f net10.0-android -c Debug
dotnet run -f net10.0-android -c Debug --no-build
```

---

## ⏱️ Timeline

| Phase | Status | Time | Action |
|-------|--------|------|--------|
| 1. Android Build | ⏳ In Progress | 1-2 min | Wait... |
| 2. Extract SHA-1 | ⏳ Waiting | 1 min | Click: verify build done |
| 3-5. Google Setup | 📋 TODO | 10 min | Manual (copy-paste) |
| 6. Restrict Key | 📋 TODO | 3 min | Optional |
| 7. Update Manifest | 📋 TODO | 1 min | I'll do this for you |
| 8. Deploy & Test | 📋 TODO | 5-7 min | I'll do this for you |

**Total Estimated Time:** 25-35 minutes (mostly waiting for builds)

---

## 📁 Files Ready for You

✅ **extract-sha1.ps1** - Run this after build completes to get your fingerprint

---

## 🎯 Next Step

**Wait for the Android build to complete.** It's running in the background right now.

Once it's done:
1. Run: `.\extract-sha1.ps1`
2. Copy your SHA-1 fingerprint
3. Let me know (or just proceed to Google Cloud Console)

---

## 💬 What to Do

**Option A: Wait and Let Me Know When Done**
- Monitor the build progress
- Tell me "Build is done!"
- I'll extract your SHA-1 and continue

**Option B: Check Build Status Yourself**
```powershell
# Check build progress
$keystorePath = "$env:USERPROFILE\.android\debug.keystore"
if (Test-Path $keystorePath) {
	Write-Host "✅ Build complete! Keystore created."
} else {
	Write-Host "⏳ Build still running..."
}
```

**Option C: Start Google Cloud Setup Now (While Build Runs)**
- Go to https://console.cloud.google.com
- Create a new project "CampusGuide"
- Enable "Maps SDK for Android"
- Create an API key
- (You'll use your SHA-1 in Phase 6)

---

**Let me know when the build is done!**

*Last Updated: 2026-11-03*
