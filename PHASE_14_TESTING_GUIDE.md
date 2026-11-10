# Phase 14 Map Feature Testing Guide

## Pre-Test Checklist
- [ ] Emulator is running (pixel_7 - api_36 or similar)
- [ ] App is deployed
- [ ] Network connection available (for Firebase Firestore)
- [ ] Device has at least 100MB free space

---

## Test Scenarios

### **Test 1: Initial Load & Display All Locations**
**Expected:** Map page shows all 6 campus locations in a scrollable list

**Steps:**
1. Launch the app
2. Tap **"Map"** in bottom navigation
3. Scroll down to see all location cards

**Expected Results:**
- [ ] Page title "Campus Map" visible with purple header
- [ ] Search bar visible below title
- [ ] Filter buttons visible (All, library, cafeteria, sports, lab, auditorium, health)
- [ ] 6 location cards displayed:
  1. Central Library (Academic Block A, Floor 3)
  2. Student Cafeteria (Student Center, Floor G)
  3. Sports Complex (Recreation Building, Floor B1)
  4. Computer Science Lab (Engineering Block, Floor 2)
  5. Main Auditorium (Academic Block, Floor G)
  6. Health Center (Medical Building, Floor 1)
- [ ] Each card shows: Name, Type (uppercase), Description, Building, Coordinates
- [ ] Coordinates display as "📌 Lat: X.XXXX, Lon: X.XXXX"

---

### **Test 2: "All" Filter Button**
**Expected:** Clicking "All" shows all locations

**Steps:**
1. From Test 1 position, scroll up to see filter buttons
2. Click the **"All"** button (should appear selected/white)
3. Observe the location list

**Expected Results:**
- [ ] "All" button appears highlighted/white
- [ ] All 6 locations are visible
- [ ] No locations are hidden
- [ ] No "No locations found" message appears

---

### **Test 3: Type Filter - "library"**
**Expected:** Only library locations show

**Steps:**
1. From Test 2, click the **"library"** filter button
2. Observe location list update

**Expected Results:**
- [ ] "library" button appears selected
- [ ] Only 1 location visible: **Central Library**
- [ ] Other 5 locations are hidden
- [ ] Location cards update immediately (no lag)

---

### **Test 4: Type Filter - "cafeteria"**
**Expected:** Only cafeteria locations show

**Steps:**
1. Click the **"cafeteria"** filter button
2. Observe location list

**Expected Results:**
- [ ] Only 1 location visible: **Student Cafeteria**
- [ ] All other locations hidden

---

### **Test 5: Type Filter - "sports"**
**Expected:** Only sports locations show

**Steps:**
1. Click the **"sports"** filter button

**Expected Results:**
- [ ] Only 1 location visible: **Sports Complex**

---

### **Test 6: Type Filter - "lab"**
**Expected:** Only lab locations show

**Steps:**
1. Click the **"lab"** filter button

**Expected Results:**
- [ ] Only 1 location visible: **Computer Science Lab**

---

### **Test 7: Type Filter - "auditorium"**
**Expected:** Only auditorium locations show

**Steps:**
1. Click the **"auditorium"** filter button

**Expected Results:**
- [ ] Only 1 location visible: **Main Auditorium**

---

### **Test 8: Type Filter - "health"**
**Expected:** Only health locations show

**Steps:**
1. Click the **"health"** filter button

**Expected Results:**
- [ ] Only 1 location visible: **Health Center**

---

### **Test 9: Search - By Location Name**
**Expected:** Search finds location by name

**Steps:**
1. Click **"All"** to reset filters
2. Tap the search entry field
3. Type: **"library"**
4. Observe results update

**Expected Results:**
- [ ] Only **Central Library** visible
- [ ] Other locations hidden
- [ ] Results update as you type
- [ ] Search is case-insensitive (works with "LIBRARY", "Library", "library")

---

### **Test 10: Search - By Building Name**
**Expected:** Search finds location by building

**Steps:**
1. Clear search field (tap X or select all and delete)
2. Type: **"student"**
3. Observe results

**Expected Results:**
- [ ] Only **Student Cafeteria** visible (matches "Student Center" building)
- [ ] Other locations hidden

---

### **Test 11: Search - By Type**
**Expected:** Search finds location by type category

**Steps:**
1. Clear search
2. Type: **"cafeteria"**

**Expected Results:**
- [ ] **Student Cafeteria** visible (type = "cafeteria")
- [ ] Only 1 result shown

---

### **Test 12: Search - By Description**
**Expected:** Search finds location by description text

**Steps:**
1. Clear search
2. Type: **"multi-purpose"**

**Expected Results:**
- [ ] Only **Sports Complex** visible (description contains "Multi-purpose sports facility")
- [ ] Other locations hidden

---

### **Test 13: Search - No Results**
**Expected:** Shows message when search returns no results

**Steps:**
1. Clear search
2. Type: **"xyz123nonexistent"**
3. Observe results

**Expected Results:**
- [ ] All location cards hidden
- [ ] Message appears: **"No locations found for filter: xyz123nonexistent"**
- [ ] Message is centered and gray

---

### **Test 14: Search - Clear Results**
**Expected:** Clearing search shows all locations again

**Steps:**
1. Select all text in search (Ctrl+A)
2. Delete text
3. Observe results

**Expected Results:**
- [ ] All 6 locations appear again
- [ ] No "no results" message
- [ ] Instant update

---

### **Test 15: Tap Location Card - Navigate to Details**
**Expected:** Tapping a location card navigates to LocationDetails page

**Steps:**
1. From any map view, scroll to see location cards
2. Tap the **"Central Library"** card
3. Wait for navigation

**Expected Results:**
- [ ] App navigates to location details page
- [ ] Details page shows:
  - [ ] Location name: "Central Library"
  - [ ] Full details, description, images (if available)
  - [ ] Options to add to favorites, view events, etc.
- [ ] Back navigation works (go back to map)

---

### **Test 16: Tap Different Location Cards**
**Expected:** Each location card navigates to correct location

**Steps:**
1. Return to map
2. Tap **"Student Cafeteria"** card
3. Verify details page shows correct location
4. Return to map
5. Tap **"Sports Complex"** card
6. Verify details page shows correct location

**Expected Results:**
- [ ] Each tap navigates to correct location page
- [ ] Navigation is consistent
- [ ] Back button works to return to map

---

### **Test 17: Filter Persistence**
**Expected:** Switching between filters maintains state

**Steps:**
1. Click "library" filter
2. See only Central Library
3. Click "cafeteria" filter
4. See only Student Cafeteria
5. Click "All" filter
6. See all 6 locations again

**Expected Results:**
- [ ] Each filter change updates results immediately
- [ ] No flickering or delays
- [ ] All transitions are smooth

---

### **Test 18: Combined Search + Filter**
**Expected:** Search works together with type filters

**Steps:**
1. Click "sports" filter (only Sports Complex shows)
2. Search entry: type **"recreation"**
3. Observe results

**Expected Results:**
- [ ] Only **Sports Complex** visible (matches both filter AND search)
- [ ] Search and filter work together correctly

---

### **Test 19: UI Visual Inspection**
**Expected:** UI is clean and professional

**Check the following:**
- [ ] Purple header (#512BD4) looks good
- [ ] Text is readable (white on purple)
- [ ] Search field has white background, dark text
- [ ] Filter buttons are well-spaced, clickable size (at least 50px tall)
- [ ] Location cards have borders and shadows
- [ ] Icons (📍, 📌) display correctly
- [ ] No text is cut off or overlapping
- [ ] Layout is responsive (portrait and landscape if tilting device)

---

### **Test 20: Performance Check**
**Expected:** App responds quickly to user interactions

**Steps:**
1. Tap rapidly between filters
2. Type rapidly in search field
3. Scroll location list quickly
4. Tap multiple location cards

**Expected Results:**
- [ ] No lag or stuttering
- [ ] Filters update within 100ms
- [ ] Search filters within 200ms
- [ ] Navigation is instant
- [ ] No crashes or errors in logcat

---

## Issue Tracking

### **If any test FAILS:**

#### Issue Template:
```
TEST FAILED: [Test Number & Name]
Status: ❌ Failed
Expected: [What should happen]
Actual: [What actually happened]
Steps to Reproduce: [Exact steps]
Screenshots/Logs: [Include if possible]
Device: Emulator - [pixel_7 - api_36]
Timestamp: [When it failed]
```

### **Common Issues & Solutions:**

| Issue | Possible Cause | Solution |
|-------|---|---|
| Map page doesn't load | ViewModel not initialized | Restart app, check DI registration in MauiProgram |
| Locations don't show | Firestore read issue | Check Firebase connection, verify test data loads |
| Search not working | TextChanged event not firing | Rebuild app, check Entry binding |
| Navigation fails | Route not registered | Check AppShell.xaml routes |
| Filters don't work | IsVisible property not set | Rebuild, check Location.cs property |

---

## Post-Test Summary

After completing all 20 tests, answer these questions:

1. **Overall Stability:** Did the app crash or freeze? `Yes / No`
2. **Search Quality:** Does search find locations accurately? `Yes / No`
3. **Filter Accuracy:** Do filters show correct results? `Yes / No`
4. **Navigation:** Does tapping cards navigate correctly? `Yes / No`
5. **UI/UX:** Is the interface intuitive and attractive? `Yes / No`
6. **Performance:** Does the app respond quickly? `Yes / No`
7. **Bugs Found:** How many bugs/issues encountered? `[Number]`
8. **Feature Completeness:** Is Phase 14 feature-complete? `Yes / No`

---

## Success Criteria

✅ **Phase 14 Testing is SUCCESSFUL if:**
- [ ] All 20 tests pass
- [ ] No crashes or freezes
- [ ] Search and filters work as expected
- [ ] Navigation is smooth
- [ ] UI is clean and professional
- [ ] Performance is acceptable

---

## Next Steps After Testing

**If all tests pass:**
→ Phase 14 is production-ready  
→ Proceed to Phase 15 or continue with next enhancement  

**If issues found:**
→ Log issues in tracker  
→ Fix bugs  
→ Re-run failing tests  
→ Mark as resolved when fixed  

---

## Test Environment Details

- **Device:** Android Emulator (pixel_7 - api_36)
- **App Version:** CampusGuide v1.0
- **Target Framework:** .NET 10
- **Deployment:** Release build
- **Date Tested:** [Today's Date]
- **Tester:** [Your Name]

---

**Good luck testing! 🚀**
