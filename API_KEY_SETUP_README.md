# CampusGuide - Google Maps API Key Setup

## Quick Start (3 Steps)

### 1️⃣ Get Your Debug Fingerprint

Run this PowerShell script to extract your Android certificate's SHA-1 fingerprint:

```powershell
.\get-fingerprint.ps1
```

This will:
- Extract your Android debug certificate SHA-1 fingerprint
- Display it on screen
- Auto-copy it to your clipboard

**Note:** The debug keystore (`~/.android/debug.keystore`) is created automatically on your first Android build. If the script says "keystore not found," build the project first:

```powershell
cd CampusGuide
dotnet build -f net10.0-android -c Debug
```

---

### 2️⃣ Create Google Maps API Key

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. **Create a new project** named "CampusGuide"
3. Go to **APIs & Services** → **Library**
4. Search for and enable **"Maps SDK for Android"**
5. Go to **APIs & Services** → **Credentials**
6. Click **Create Credentials** → **API Key**
7. Copy the generated key (looks like: `AIzaSy...`)
8. *(Optional but recommended)* Click the key to edit it and:
   - Set **Application restrictions** → **Android apps**
   - Add your package name (`com.companyname.campusguide`) and SHA-1 fingerprint from step 1
   - Set **API restrictions** → **Maps SDK for Android only**

---

### 3️⃣ Update AndroidManifest.xml

Use the helper script to safely add your API key:

```powershell
.\update-api-key.ps1 -ApiKey "AIzaSy_YOUR_KEY_HERE" -Build debug
```

Or manually edit `CampusGuide/Platforms/Android/AndroidManifest.xml`:

```xml
<application android:allowBackup="true" android:icon="@mipmap/appicon" android:roundIcon="@mipmap/appicon_round" android:supportsRtl="true">
  <meta-data android:name="com.google.android.geo.API_KEY" 
			 android:value="AIzaSy_YOUR_KEY_HERE" />
</application>
```

---

## Verify It Works

```powershell
cd CampusGuide

# Clean build
dotnet clean -f net10.0-android

# Rebuild
dotnet build -f net10.0-android -c Debug

# Deploy to emulator
dotnet run -f net10.0-android -c Debug --no-build
```

✅ The app should now launch without crashing and display the Home screen with functional Google Maps support.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "API key not found" | Verify `<meta-data>` tag is inside `<application>` tag, not outside |
| "Invalid API key" | Check key format starts with `AIzaSy`, regenerate if needed |
| "Keystore not found" | Build project first: `dotnet build -f net10.0-android -c Debug` |
| Maps don't load | Ensure "Maps SDK for Android" is enabled in Google Cloud Console |
| "Quotas exceeded" | Check Google Cloud Console billing/quotas settings |

---

## Files Included

- **`GOOGLE_MAPS_SETUP.md`** - Detailed step-by-step guide
- **`get-fingerprint.ps1`** - Script to extract your certificate fingerprint
- **`update-api-key.ps1`** - Script to safely update AndroidManifest.xml
- **`CampusGuide/Platforms/Android/AndroidManifest.xml`** - Android configuration (contains API key)

---

## Security Considerations

⚠️ **Best Practices:**

1. **Use restricted keys** - Always restrict API keys by:
   - Application (Android app with SHA-1 fingerprint)
   - API (Maps SDK for Android only)

2. **Separate keys for environments:**
   - **Debug key** - Use demo fingerprint for development
   - **Release key** - Use production release certificate fingerprint
   - **Different Google Cloud projects** - One for each environment (optional but recommended)

3. **Never commit real keys to public repos** - Use environment variables or separate config files

4. **Rotate keys periodically** - Regenerate API keys every 6-12 months

5. **Monitor usage** - Check Google Cloud Console for suspicious activity

---

## For Release Builds

When preparing to release to Google Play Store:

1. Generate a release signing key:
```powershell
$javaPath = "C:\Program Files\Android\openjdk\jdk-21.0.8\bin\keytool.exe"
& $javaPath -genkey -v -keystore "release.keystore" -keyalg RSA -keysize 2048 -validity 10000 -alias release_key
```

2. Extract your release certificate's SHA-1 fingerprint:
```powershell
.\get-fingerprint.ps1 -BuildType release
```

3. Create a **release-specific API key** in Google Cloud Console with your release certificate's SHA-1

4. Create `CampusGuide/Platforms/Android/AndroidManifest.Release.xml` with the release API key

5. Update your `.csproj` to use the appropriate manifest for each build configuration

---

## Need Help?

- [Google Maps SDK for Android Docs](https://developers.google.com/maps/documentation/android-sdk/overview)
- [API Key Setup Guide](https://developers.google.com/maps/documentation/android-sdk/get-api-key)
- [Google Cloud Console](https://console.cloud.google.com)

**Last Updated:** 2026-11-03
