# Google Maps API Configuration Guide

## Overview
The CampusGuide app uses Google Maps for displaying campus locations. An API key is required in `AndroidManifest.xml` to enable this functionality.

## Step 1: Get Your Android App Fingerprint

Before requesting an API key, you need your app's SHA-1 fingerprint.

### For Debug Builds:
```powershell
# Run this command in PowerShell
$keystorePath = "$env:USERPROFILE\.android\debug.keystore"
$javaPath = "C:\Program Files\Android\openjdk\jdk-21.0.8\bin\keytool.exe"

& $javaPath -list -v -keystore $keystorePath -alias androiddebugkey -storepass android -keypass android
```

Look for the **SHA1** fingerprint in the output. It will look like:
```
SHA1: AB:CD:EF:12:34:56:78:90:AB:CD:EF:12:34:56:78:90:AB:CD:EF:12
```

Remove the colons to get:
```
ABCDEF1234567890ABCDEF1234567890ABCDEF12
```

### For Release Builds:
You'll need to run the keytool command with your release keystore:
```powershell
& $javaPath -list -v -keystore "path\to\your\release.keystore" -alias your_alias
```

---

## Step 2: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Click **Create Project**
3. Name it: `CampusGuide` (or your preferred name)
4. Click **Create** and wait for the project to initialize
5. Select the newly created project from the dropdown

---

## Step 3: Enable Maps SDK for Android

1. In Google Cloud Console, go to **APIs & Services** → **Library**
2. Search for **"Maps SDK for Android"**
3. Click on it and then click **Enable**
4. Wait for it to enable (you'll see a checkmark)

---

## Step 4: Create an API Key

1. Go to **APIs & Services** → **Credentials**
2. Click **Create Credentials** → **API Key**
3. A dialog will show your new API key (it will look like: `AIzaSy...`)
4. **Copy this key** to a safe location

---

## Step 5: Restrict the API Key (Recommended for Security)

1. In the Credentials page, click on your newly created API key
2. Under **Application restrictions**, select **Android apps**
3. Click **Add package name and fingerprint**
4. Enter:
   - **Package name**: `com.companyname.campusguide`
   - **SHA-1 certificate fingerprint**: Paste your fingerprint from Step 1

5. Under **API restrictions**, select **Restrict key**
6. Choose only **Maps SDK for Android**
7. Click **Save**

---

## Step 6: Add API Key to AndroidManifest.xml

1. Open `/CampusGuide/Platforms/Android/AndroidManifest.xml`
2. Find the `<application>` element
3. Add this line inside it (between the opening and closing tags):
```xml
<meta-data android:name="com.google.android.geo.API_KEY" 
		   android:value="YOUR_API_KEY_HERE" />
```

4. Replace `YOUR_API_KEY_HERE` with your actual API key
5. Save the file

**Example:**
```xml
<application android:allowBackup="true" android:icon="@mipmap/appicon" android:roundIcon="@mipmap/appicon_round" android:supportsRtl="true">
  <meta-data android:name="com.google.android.geo.API_KEY" 
			 android:value="AIzaSyDemoKey1234567890_YourActualKeyHere" />
</application>
```

---

## Step 7: Rebuild and Test

```powershell
cd "C:\Subject Semester Degree\SEM 7\CSNB5123(MAD)\Project\CampusGuide\CampusGuide"

# Clean build
dotnet clean -f net10.0-android -c Debug

# Rebuild
dotnet build -f net10.0-android -c Debug

# Deploy to emulator
dotnet run -f net10.0-android -c Debug --no-build
```

---

## Troubleshooting

### "API key not found" error
- Verify the `<meta-data>` tag is inside the `<application>` element
- Check that the API key is correctly pasted (no extra spaces or quotes)
- Clean and rebuild the project

### "Invalid API Key" error
- Verify the key format (should start with `AIzaSy...`)
- Check that Maps SDK for Android is enabled in Google Cloud Console
- Verify your package name matches `com.companyname.campusguide`
- Verify your SHA-1 fingerprint matches the one registered

### "Quotas exceeded" error
- Check your Google Cloud Console billing settings
- API keys are free to use but may have usage quotas

---

## Security Notes

⚠️ **Important:**
- Never commit your real API key to public repositories
- Use restricted keys (application fingerprint + API restrictions)
- Consider using environment variables for production releases
- Rotate your API key periodically

For this development project, the debug key above is acceptable while testing.

---

## References

- [Google Maps SDK for Android Documentation](https://developers.google.com/maps/documentation/android-sdk/overview)
- [API Key Setup Guide](https://developers.google.com/maps/documentation/android-sdk/get-api-key)
- [Key Restrictions](https://cloud.google.com/docs/authentication/api-keys#api_key_restrictions)
