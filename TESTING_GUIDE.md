# CampusGuide Favorites & Events Testing Guide

## Build Status ✅
- **Android Debug Build**: Successfully compiled
- **Warnings**: Only dependency version resolution (non-blocking)
- **Ready for Deployment**: Yes

---

## Prerequisites
1. **Android Emulator Running**: You need an Android emulator instance running
   - Options:
	 - Use Android Studio's AVD Manager to create/launch an emulator
	 - Use existing pixel_7_-_api_36_0 emulator if available
	 - Or any Android 10+ emulator

2. **Visual Studio or Command Line**: Either:
   - Use Visual Studio's "Run on Android Emulator" button
   - Or use PowerShell commands below

---

## Deployment Method 1: Visual Studio (Easiest)
1. Open `CampusGuide.slnx` in Visual Studio
2. Select a running Android emulator from the target device dropdown
3. Click "Run" or press F5
4. Wait for app to load (~30-60 seconds)

---

## Deployment Method 2: PowerShell (Command Line)
```powershell
cd "C:\Subject Semester Degree\SEM 7\CSNB5123(MAD)\Project\CampusGuide"

# Build and run on Android emulator
dotnet maui run -f net10.0-android
```

---

## Testing Checklist

### **1. App Launch & Navigation**
- [ ] App launches without crashes
- [ ] Bottom tab bar displays 6 tabs: Home, Map, Search, Events, Favorites, Alerts
- [ ] Tabs are responsive and clickable

### **2. Events Tab Testing**
Navigate to **Events** tab:
- [ ] Page loads with "Events" title
- [ ] Pull-to-refresh works (drag down)
- [ ] Event list displays with 5 test events:
  - Campus Orientation (2 days from now)
  - Library Workshop: Research Skills (3 days)
  - Sports Day (5 days)
  - Tech Talks: AI in Education (7 days)
  - Career Fair 2024 (10 days)
- [ ] Each event card shows:
  - Month/day badge (teal/turquoise color)
  - Event title (bold)
  - Start date/time
  - First line of description
  - Location ID (e.g., "student-center-001")

### **3. Event Details Testing**
- [ ] Tap an event to open details page
- [ ] Details page displays:
  - Event title (large, bold)
  - 📅 Date & Time section with full date range
  - 📍 Location section with location ID
  - "View Location on Map" button (blue)
  - 📝 Description section with full text
- [ ] "View Location on Map" button navigates to location details page
- [ ] "Back to Events" works correctly

### **4. Favorites Tab Testing**
Navigate to **Favorites** tab:
- [ ] Page loads with "Favorites" title
- [ ] Pull-to-refresh works
- [ ] Empty state displays:
  - ❤️ emoji
  - "No Favorites Yet" heading
  - Helper text about saving favorites

### **5. Favorites Save Operation**
From **Map** or **Search** page (if integration exists):
- [ ] Verify if favorites can be added (implementation may be partial)
- [ ] If saved, favorites appear in Favorites tab as cards with:
  - Location name (bold)
  - Location ID
  - "✕ Remove" button (red)

### **6. Favorites Remove Operation**
- [ ] Tap "✕ Remove" button on a favorite
- [ ] Favorite is removed from the list
- [ ] If empty, empty state returns

### **7. Navigation Cross-Tab**
- [ ] Navigate between Events → Details → Map → Back to Events
- [ ] Navigate between all 6 tabs
- [ ] No crashes or freezes during navigation

### **8. Loading & Error States**
- [ ] Pull-to-refresh shows loading indicator
- [ ] Error messages display clearly if Firestore is unavailable
- [ ] App gracefully handles network issues

### **9. Data Persistence**
- [ ] Exit app and relaunch
- [ ] Favorites persist (if any were saved)
- [ ] Events reload correctly

---

## Expected Results

### ✅ What Should Work
- Events list loads with test data
- Smooth navigation between pages
- Pull-to-refresh functionality
- Event details display correctly
- Favorites empty state shows

### ⚠️ What May Be Partial
- Favorites save from other pages (integration with Map/LocationDetails may need refinement)
- Real Firestore events (test data is fallback when Firestore is unavailable)
- Images for event headers (currently placeholder)

### ❌ Known Issues to Ignore
- MVVM warnings about direct field references (cosmetic, not breaking)
- Android SDK warnings (non-blocking)

---

## Troubleshooting

### App crashes on launch
- Rebuild project: `dotnet build -f net10.0-android -c Debug`
- Clear emulator cache: Run > Wipe Data from AVD Manager
- Restart emulator

### Events page shows no data
- Pull-to-refresh to reload
- Check emulator network connectivity
- Events may be loading test data if Firestore is unavailable

### Favorites not saving
- This feature may require additional integration work
- Verify IFavoritesService is properly registered in MauiProgram
- Check app has write permissions

### Navigation doesn't work
- Verify AppShell.xaml routes are correct
- Check Shell.Current.GoToAsync() calls in page code-behind
- Ensure routes match: `event-details?id={eventId}`, `location-details?id={locationId}`

---

## Next Steps After Testing
1. **If tests pass**: Proceed to QR Scanner (Phase 10) or FCM (Phase 11)
2. **If issues found**: 
   - Document specific error messages
   - Check debug output in Visual Studio
   - Fix and rebuild

---

## Debug Output Tips
In Visual Studio, view diagnostics:
- **Debug** → **Windows** → **Output**
- Filter for app output
- Look for `System.Diagnostics.Debug.WriteLine()` messages from ViewModels

---

## Build Artifacts
- APK location: `CampusGuide/bin/Debug/net10.0-android/`
- If you need to rebuild: `dotnet build -f net10.0-android -c Debug`
