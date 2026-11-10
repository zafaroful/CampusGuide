# Phase 15 Testing Guide - Native Map Display

## 📱 Pre-Test Setup

### Requirements
- Emulator running (pixel_7_-_api_36_0)
- App built and deployed
- Phase 15 map feature installed
- ~15-20 minutes for full test suite

### Test Data Available
- 6 locations with pins on map
- All location types represented
- Coordinates: 3.13xx, 101.68xx (Malaysia campus area)

---

## ✅ Test Scenarios (20 Total)

### 1️⃣ MAP LOAD & DISPLAY (Tests 1-3)

#### Test 1: Map Loads Successfully
**Steps:**
1. Open CampusGuide app
2. Navigate to Map tab
3. Wait 2-3 seconds for map to fully load

**Expected:**
- Map view displays
- Background is satellite or map tile view
- No crash or error dialogs
- Map is interactive (can pan)

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 2: All 6 Pins Visible Initially
**Steps:**
1. On Map tab (all filters "All")
2. Look at map for pin markers

**Expected:**
- Exactly 6 pins visible on map
- Pins spread across campus area (3.1385-3.1398 latitude)
- Each pin has a label with building name
- Pins clickable (try hovering/tapping)

**Pin IDs to verify:**
- [ ] lib-001 (Central Library)
- [ ] cafe-001 (Student Cafeteria)
- [ ] gym-001 (Sports Complex)
- [ ] lab-001 (CS Lab)
- [ ] aud-001 (Main Auditorium)
- [ ] health-001 (Health Center)

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 3: Map Center & Zoom Correct
**Steps:**
1. On Map tab
2. Observe current zoom level
3. Try to identify if campus is centered

**Expected:**
- Map centered around 3.1391, 101.6871
- ~0.5 mile radius visible (campus-scale view)
- All 6 pins visible in current viewport
- Map not zoomed too far in or out

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

### 2️⃣ PIN INTERACTION (Tests 4-6)

#### Test 4: Tap Library Pin
**Steps:**
1. On Map tab, find Central Library pin (🔵)
2. Tap on the pin marker

**Expected:**
- Pin highlights or shows callout
- Callout shows: "Central Library", "LIBRARY", "Academic Block A, Floor 3"
- Navigation to Location Details page
- Details page shows: Library name, description, images, events, etc.

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 5: Tap Cafeteria Pin
**Steps:**
1. On Map tab, find Student Cafeteria pin (🟠)
2. Tap on the pin

**Expected:**
- Navigation to cafeteria details page
- Details show: "Student Cafeteria" with type "CAFETERIA"
- Building: "Student Center", Floor "G"
- Can add to favorites

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 6: Back from Details to Map
**Steps:**
1. On Location Details page (after tapping a pin)
2. Tap back button or swipe back
3. Or use navigation menu

**Expected:**
- Returns to Map tab
- Map still shows all 6 pins
- Previously tapped pin is highlighted or visible
- Filter state maintained

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

### 3️⃣ SEARCH FEATURE (Tests 7-9)

#### Test 7: Search for "Library"
**Steps:**
1. On Map tab with all pins visible
2. Tap search box
3. Type "library"
4. Wait for real-time filter

**Expected:**
- Map filters immediately as you type
- Only Central Library pin remains visible
- Other 5 pins hidden
- Only 1 pin on map
- Can still tap to navigate

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 8: Search for "Cafeteria"
**Steps:**
1. Clear search box (or select all and delete)
2. Type "cafeteria"

**Expected:**
- Only Student Cafeteria pin visible
- All other pins hidden
- Map may auto-zoom to show the single pin
- Search results count: 1 location

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 9: Search No Results
**Steps:**
1. Clear search
2. Type "xyz" or "nonexistent"
3. Wait for filter

**Expected:**
- No pins visible on map
- Map area is empty
- No error message (silent filter)
- Clearing search shows all 6 again

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

### 4️⃣ FILTER BUTTONS (Tests 10-15)

#### Test 10: Filter - "All"
**Steps:**
1. On Map tab
2. Click "All" button (should already be selected)

**Expected:**
- All 6 pins visible
- "All" button highlighted (white/active)
- Other filter buttons not highlighted

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 11: Filter - "library"
**Steps:**
1. Click "library" filter button

**Expected:**
- Only library pins visible (1: Central Library)
- All other location types hidden (5 pins hidden)
- "library" button appears selected
- "All" button appearance changes

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 12: Filter - "cafeteria"
**Steps:**
1. Click "cafeteria" filter button

**Expected:**
- Only Student Cafeteria pin visible (1 pin)
- All 5 others hidden
- Button state shows "cafeteria" selected

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 13: Filter - "sports"
**Steps:**
1. Click "sports" filter button

**Expected:**
- Only Sports Complex pin visible (1 pin)
- Map may focus on that location

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 14: Filter - "lab"
**Steps:**
1. Click "lab" filter button

**Expected:**
- Only CS Lab pin visible (1 pin)
- All others hidden

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 15: Filter - "auditorium" & "health"
**Steps:**
1. Click "auditorium" button
2. Verify only auditorium shows
3. Click "health" button
4. Verify only health center shows

**Expected:**
- Each filter shows exactly 1 corresponding pin
- All others hidden
- Buttons reflect selection state

**Status (auditorium):** [ ] PASS  [ ] FAIL  
**Status (health):** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

### 5️⃣ COMBINED FILTERS (Tests 16-17)

#### Test 16: Search + Filter Together
**Steps:**
1. Click "cafeteria" filter (shows 1 pin)
2. Type "student" in search
3. Clear search
4. Verify cafeteria filter still active

**Expected:**
- When filter + search both applied: shows only matching + filtered
- Clearing search keeps filter active
- Can switch between filters
- No duplicate pins

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 17: Filter then Search Partial Match
**Steps:**
1. Click "All" to show all pins
2. Type "block" (matches multiple buildings)
3. Expected: library and auditorium have "Block" in building

**Expected:**
- 2 pins visible: Central Library + Main Auditorium
- Both have "Block" in building name
- Other 4 hidden
- Results update in real-time

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

### 6️⃣ MAP CONTROLS (Tests 18-19)

#### Test 18: Zoom In Button
**Steps:**
1. On map with multiple pins visible
2. Click 🔍+ (Zoom In) button
3. Observe map change

**Expected:**
- Map zooms in (2x magnification)
- Visible area becomes smaller
- Buildings/features become larger and more visible
- Same pins stay visible (or may move off-screen if edges)
- Can still interact with pins

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

#### Test 19: Zoom Out & Center Button
**Steps:**
1. After zooming in, click 🔍− (Zoom Out) button
2. Click 🎯 (Center) button
3. Observe map changes

**Expected:**
- Zoom Out: Map zooms out, shows wider area
- Center: Map recenter on campus (3.1391, 101.6871)
- All 6 pins visible again after center
- Smooth transitions

**Status (zoom out):** [ ] PASS  [ ] FAIL  
**Status (center):** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

### 7️⃣ UI & LEGEND (Test 20)

#### Test 20: Legend & Header Display
**Steps:**
1. On Map tab
2. Look at header section
3. Verify legend visibility

**Expected:**
- Purple header with "Campus Map" title
- Search box visible and functional
- Filter buttons displayed (7 buttons, 2 rows)
- Color legend visible: 🔵 🟠 🟢 🟣 🔴 🌸
- All text readable in white on purple
- Controls toolbar visible at bottom
- Info label visible

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

## 🔄 Regression Tests (Verify Phase 14 Still Works)

### Test 21: Card List Not Showing
**Expected:**
- Map view is now native map (NOT card list)
- No scrollable location cards
- Pure map interface

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

### Test 22: Navigation to Details Still Works
**Expected:**
- Tapping any pin navigates correctly
- Location Details page loads
- Shell routing intact
- Back navigation works
- No broken routes

**Status:** [ ] PASS  [ ] FAIL  
**Notes:** _______________

---

## 📊 Results Summary

### Scoring

**Total Tests:** 22  
**Passing:** ____ / 22  
**Failing:** ____ / 22  
**Pass Rate:** _____%

### Test Categories

| Category | Tests | Passed | Status |
|----------|-------|--------|--------|
| Map Load & Display | 3 | ___ | ✅ or ⚠️ |
| Pin Interaction | 3 | ___ | ✅ or ⚠️ |
| Search Feature | 3 | ___ | ✅ or ⚠️ |
| Filter Buttons | 6 | ___ | ✅ or ⚠️ |
| Combined Filters | 2 | ___ | ✅ or ⚠️ |
| Map Controls | 2 | ___ | ✅ or ⚠️ |
| UI & Legend | 1 | ___ | ✅ or ⚠️ |
| Regression | 2 | ___ | ✅ or ⚠️ |

---

## 🐛 Issue Log

### Issue #1
**Description:** ______________________  
**Steps to Reproduce:** ______________  
**Expected:** ________________________  
**Actual:** ___________________________  
**Severity:** [ ] Critical [ ] High [ ] Medium [ ] Low  

---

### Issue #2
**Description:** ______________________  
**Steps to Reproduce:** ______________  
**Expected:** ________________________  
**Actual:** ___________________________  
**Severity:** [ ] Critical [ ] High [ ] Medium [ ] Low  

---

### Issue #3
**Description:** ______________________  
**Steps to Reproduce:** ______________  
**Expected:** ________________________  
**Actual:** ___________________________  
**Severity:** [ ] Critical [ ] High [ ] Medium [ ] Low  

---

## ✅ Acceptance Criteria

### All Must Pass for Phase 15 Completion

- [ ] Map loads without crashing
- [ ] All 6 pins display with correct locations
- [ ] Each pin is tappable and navigates to details
- [ ] Search filters work in real-time
- [ ] Filter buttons work for all 7 types
- [ ] Zoom in/out buttons work
- [ ] Center button recenters map
- [ ] No duplicate pins on map
- [ ] No error dialogs or crashes
- [ ] Header and legend are clearly visible
- [ ] Legend shows all 6 colors correctly
- [ ] Combined filters (search + type) work
- [ ] Back navigation from details works
- [ ] Performance is smooth (no lag when filtering)
- [ ] All 22 test scenarios pass

---

## 📝 Sign-Off

**Tester Name:** ________________________  
**Date:** ____________  
**Time Spent:** __________ minutes  

**Overall Result:**
- [ ] ✅ PASS - All tests passed, ready for production
- [ ] ⚠️ PASS WITH ISSUES - Minor issues, can deploy
- [ ] ❌ FAIL - Critical issues, needs fixes

**Notes:**  
_________________________________________________________________________

---

## 🎯 Next Steps

If all tests pass:
1. Build in Release mode
2. Deploy to emulator/device
3. Document any issues
4. Plan Phase 16 enhancements

If tests fail:
1. Document all failures
2. Create bug fix tasks
3. Re-run failed tests after fixes
4. Repeat until all pass

---

**Phase 15 Native Map Testing Complete!**
