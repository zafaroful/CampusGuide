# Phase 15 Quick Test Reference Card

## 🚀 Quick Start Testing

**Time Required:** 15-20 minutes  
**Emulator Status:** Must be running  
**App Status:** Must be deployed

---

## 🎯 Critical Path Tests (Must Pass)

### 1. Map Loads (30 seconds)
```
→ Open app
→ Go to Map tab
→ Verify map displays with 6 pins
✅ PASS: Map visible, all 6 pins showing
❌ FAIL: Blank screen, crash, or pins missing
```

### 2. Tap Pin → Navigate (1 minute)
```
→ Tap on Library pin (🔵)
→ Verify navigation to details
→ Verify details page shows library info
→ Tap back to return to map
✅ PASS: Navigation smooth, details correct
❌ FAIL: Wrong page, crash, or navigation broken
```

### 3. Search Works (1 minute)
```
→ Type "cafe" in search box
→ Verify only cafeteria pin shows
→ Clear search
→ Verify all 6 pins return
✅ PASS: Filtering instant and accurate
❌ FAIL: Wrong results or lag
```

### 4. Filter Buttons Work (1 minute)
```
→ Click "sports" filter
→ Verify only sports pin shows
→ Click "All"
→ Verify all 6 pins return
✅ PASS: Filters work instantly
❌ FAIL: Wrong pins shown or lag
```

### 5. Map Controls Work (1 minute)
```
→ Click 🎯 Center button
→ Verify map recenters
→ Click 🔍+ Zoom In
→ Verify map gets closer
→ Click 🔍− Zoom Out
→ Verify map gets wider
✅ PASS: All buttons responsive
❌ FAIL: Buttons don't work or cause crash
```

---

## 📋 Complete Test Matrix

| Test # | Feature | Steps | Duration | Status |
|--------|---------|-------|----------|--------|
| 1 | Map Load | Open → Look at map | 30s | [ ] ✅ [ ] ❌ |
| 2 | 6 Pins Display | Count pins on map | 30s | [ ] ✅ [ ] ❌ |
| 3 | Tap Library | Tap 🔵 → Navigate → Back | 1m | [ ] ✅ [ ] ❌ |
| 4 | Tap Cafeteria | Tap 🟠 → Navigate → Back | 1m | [ ] ✅ [ ] ❌ |
| 5 | Search "Library" | Type → Filter → Clear | 1m | [ ] ✅ [ ] ❌ |
| 6 | Search "Sports" | Type → Filter → Clear | 1m | [ ] ✅ [ ] ❌ |
| 7 | Filter "library" | Click button → Show 1 → All | 1m | [ ] ✅ [ ] ❌ |
| 8 | Filter "cafeteria" | Click button → Show 1 → All | 1m | [ ] ✅ [ ] ❌ |
| 9 | Filter "All" | Click → Verify all 6 | 30s | [ ] ✅ [ ] ❌ |
| 10 | Zoom In | Click 🔍+ → Verify closer | 30s | [ ] ✅ [ ] ❌ |
| 11 | Zoom Out | Click 🔍− → Verify wider | 30s | [ ] ✅ [ ] ❌ |
| 12 | Center Button | Click 🎯 → Recenter | 30s | [ ] ✅ [ ] ❌ |
| 13 | Header Visible | Check purple header, title | 20s | [ ] ✅ [ ] ❌ |
| 14 | Legend Visible | Check emoji colors | 20s | [ ] ✅ [ ] ❌ |
| 15 | Search + Filter | Search + apply type filter | 1m | [ ] ✅ [ ] ❌ |
| 16 | No Crash | Use all features quickly | 2m | [ ] ✅ [ ] ❌ |
| 17 | Performance | No lag during operations | 2m | [ ] ✅ [ ] ❌ |

**Total Tests:** 17  
**Pass Rate Target:** 100% (17/17)  
**Total Duration:** 18-20 minutes

---

## ⚡ Smoke Test (5 minutes)

For quick validation:

```powershell
# 1. App opens without crash (30s)
# 2. Map tab shows map with 6 pins (30s)
# 3. Can tap a pin and see details (1m)
# 4. Search filters work (1m)
# 5. Can close app without crash (30s)

# If all 5 pass → Deploy OK ✅
# If any fail → Debug before deploying ❌
```

---

## 🔍 Debug Checklist if Tests Fail

### Map Not Loading
- [ ] Check emulator running
- [ ] Check internet connection
- [ ] Check Maps package installed (MauiProgram.cs has `.UseMauiMaps()`)
- [ ] Check Android permissions in manifest
- [ ] Check logcat for errors

### Pins Not Showing
- [ ] Check ViewModel has 6 locations in MapPins
- [ ] Check locations have valid coordinates
- [ ] Check RefreshMapPins() is called
- [ ] Check pin Location is not null

### Search Not Working
- [ ] Check Entry TextChanged event connected
- [ ] Check OnSearchTextChanged() method exists
- [ ] Check IsVisible property is being set
- [ ] Check RefreshMapPins() called after filtering

### Tap Not Working
- [ ] Check Pin.MarkerClicked event connected
- [ ] Check OnPinTapped() method exists
- [ ] Check Shell navigation configured
- [ ] Check location-details route exists

### Performance Issues
- [ ] Check pin count (6 is small, should be fast)
- [ ] Check RefreshMapPins() not called excessively
- [ ] Check no infinite loops
- [ ] Check no blocking operations on UI thread

---

## 📊 Pass/Fail Template

```
Date: _______________
Tester: ______________

SMOKE TEST:
- Map loads: [✅] [❌]
- 6 pins visible: [✅] [❌]
- Tap navigation: [✅] [❌]
- Search works: [✅] [❌]
- No crash: [✅] [❌]

RESULT: [✅ PASS] [❌ FAIL]

Issues found:
1. _______________
2. _______________
3. _______________

Notes:
_________________________
```

---

## 🎯 Success Criteria

**Phase 15 Passes Testing If:**

1. ✅ Map displays without crash
2. ✅ All 6 pins visible with correct locations
3. ✅ Each pin tappable → navigates to details
4. ✅ Search filters in real-time
5. ✅ All 7 filter types work
6. ✅ Zoom in/out buttons work
7. ✅ Center button works
8. ✅ No duplicate pins
9. ✅ No error messages
10. ✅ Performance is smooth (no lag)

**If ALL 10 Pass:** ✅ **READY FOR PRODUCTION**

---

## 🚀 Deployment After Testing

```powershell
# If testing passes:
# 1. Build in Release mode
dotnet build -c Release -f net10.0-android

# 2. Deploy to device
dotnet build -t:Run -c Debug -f net10.0-android

# 3. Users can now:
#    - See interactive map
#    - Search and filter locations
#    - Navigate to location details
#    - Explore campus visually
```

---

## 📱 Emulator Tips

### If Map Blank
- Restart emulator
- Ensure network connectivity in emulator
- Check map tile loading in logcat

### If Pins Not Showing
- Cold boot the emulator
- Rebuild the app
- Check APK has Maps library

### If Slow
- Close other apps in emulator
- Restart emulator
- Check emulator RAM allocation

---

## ✅ Test Sign-Off

```
Phase 15 Testing Status: ___________

Tested By: ____________________
Date: _________________________
Time Spent: ____________________

Result: [✅ PASS] [⚠️ PASS with notes] [❌ FAIL]

Sign-off: ______________________
```

---

**Phase 15 is ready for deployment and testing! 🚀**
