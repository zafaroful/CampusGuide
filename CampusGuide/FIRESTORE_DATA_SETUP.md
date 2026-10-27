# 📊 How to Populate Firestore with Test Data

## Option A: Using Firebase Console (Easiest - 5 minutes)

### Step 1: Open Firebase Console
1. Go to https://console.firebase.google.com/
2. Select your "campusguide" project
3. Click **Firestore Database** in the left menu

### Step 2: Create Collections

#### Create "locations" Collection
1. Click **Create Collection**
2. Name: `locations`
3. Add First Document with ID: `lib-001`
4. Add these fields:

| Field | Type | Value |
|-------|------|-------|
| id | string | lib-001 |
| name | string | Central Library |
| description | string | Main library with study areas, computer labs, and quiet zones |
| type | string | library |
| building | string | Academic Block A |
| floor | string | 3 |
| coordinates | map | { latitude: 1.3521, longitude: 103.8198 } |
| images | array | ["library1.jpg", "library2.jpg"] |
| tags | array | ["library", "study", "quiet"] |
| qrCodeId | string | campus://location/lib-001 |

5. Click **Save**
6. Repeat for other locations (see FirestoreTestData.json for all locations)

#### Create "events" Collection
1. Click **Create Collection**
2. Name: `events`
3. Add First Document with ID: `evt-001`
4. Copy fields from FirestoreTestData.json

#### Create "notifications" Collection
1. Click **Create Collection**
2. Name: `notifications`
3. Add documents from FirestoreTestData.json

---

## Option B: Import JSON Data (Fastest - Using ExtensionScript)

Unfortunately, Firestore doesn't have direct JSON import in console, but you can use:

### Firebase Local Emulator Suite (For Development)
```bash
firebase init
firebase emulators:start --import=firestore-data
```

Then connect your app to the emulator.

---

## Option C: Programmatic Seeding (In-App)

A `FirestoreDataSeeder.cs` service is already in your project.

### To enable it:

1. **Modify `App.xaml.cs`** to seed data on first app launch:

```csharp
protected override async void OnStart()
{
	base.OnStart();
	SetupExceptionHandlers();

	// Seed test data (comment out after first run if desired)
	await SeedFirestoreIfEmpty();
}

private async Task SeedFirestoreIfEmpty()
{
	try
	{
		var firestoreService = ServiceHelper.GetService<IFirestoreService>();
		if (firestoreService != null)
		{
			var locations = await firestoreService.GetAllLocationsAsync();

			// Only seed if empty
			if (locations == null || locations.Count == 0)
			{
				var seeder = new FirestoreDataSeeder(firestoreService);
				await seeder.SeedTestDataAsync();
				System.Diagnostics.Debug.WriteLine("✓ Firestore data seeded");
			}
		}
	}
	catch (Exception ex)
	{
		System.Diagnostics.Debug.WriteLine($"Seeding error: {ex.Message}");
	}
}
```

---

## 📋 Test Data Summary

### Locations (5 total)
- **lib-001** - Central Library (Academic Block A, Floor 3)
- **cafe-001** - Main Cafeteria (Food Court, Floor 1)
- **gym-001** - Sports Complex (Athletic Building, Ground)
- **lab-001** - Computer Science Lab (Engineering Block B, Floor 2)
- **aud-001** - Lecture Hall A (Academic Block C, Floor 1)

### Events (4 total)
- **evt-001** - Campus Orientation (Nov 20)
- **evt-002** - Tech Talks 2024 (Nov 22)
- **evt-003** - Hackathon 2024 (Nov 25-26)
- **evt-004** - Sports Day (Nov 28)

### Notifications (4 total)
- **notif-001** - Library Maintenance
- **notif-002** - Cafeteria Menu Updated
- **notif-003** - Sports Complex Hours Extended
- **notif-004** - Campus Orientation Reminder

---

## ✅ After Adding Data

Once data is added:

1. ✅ **Search page** will show locations
2. ✅ **QR Scanner** can find locations by ID
3. ✅ **Events section** will display upcoming events
4. ✅ **Alerts/Notifications** will show announcements
5. ✅ **Map** can display location pins

---

## 🔧 Recommended Approach

**For Development:**
- Use **Option C (Programmatic Seeding)** - automatic and clean
- Uncomment the seeding call in App.xaml.cs when needed

**For Production:**
- Use **Option A (Firebase Console)** - manual entry of real data
- Disable programmatic seeding

---

## 📝 JSON Format Reference

See `FirestoreTestData.json` in the project root for the complete JSON structure ready to copy.

---

## ❓ Questions?

- Still stuck? Try the Firebase documentation: https://firebase.google.com/docs/database/start
- Issues with Firestore? Check Android Logcat for errors
