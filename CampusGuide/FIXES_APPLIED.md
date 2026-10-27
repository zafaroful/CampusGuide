# CampusGuide - Exception Fixes Applied

## Summary
Fixed two critical runtime errors that were preventing the app from launching and navigating properly.

---

## Fix #1: Firebase Initialization Error
**Error Message:** "Default FirebaseApp is not initialized in this process"

### Root Cause
- `FirebaseFirestoreService` was registered as a **Singleton** in `MauiProgram.cs`
- This instantiated the service at app startup, **before** `MainActivity.OnCreate()` completed
- When the constructor tried to access `FirebaseFirestore.Instance`, Firebase wasn't initialized yet
- Result: Crash on app launch

### Solution Applied
**File:** `CampusGuide/Services/FirebaseFirestoreService.cs`

Changed from **eager initialization** to **lazy initialization**:

**Before:**
```csharp
readonly FirebaseFirestore _db;

public FirebaseFirestoreService()
{
	try
	{
		_db = FirebaseFirestore.Instance;  // Crashes here if Firebase not initialized
	}
	catch (Exception ex)
	{
		Log.Error(TAG, $"Failed to initialize Firestore: {ex.Message}");
		throw;
	}
}
```

**After:**
```csharp
private FirebaseFirestore? _db;
private readonly object _dbLock = new object();

private FirebaseFirestore GetFirestoreInstance()
{
	if (_db == null)
	{
		lock (_dbLock)
		{
			if (_db == null)
			{
				try
				{
					_db = FirebaseFirestore.Instance;  // Only called when first method needs Firestore
				}
				catch (Exception ex)
				{
					Log.Error(TAG, $"Failed to get Firestore instance: {ex.Message}");
					throw;
				}
			}
		}
	}
	return _db;
}

public FirebaseFirestoreService()
{
	// No initialization here - deferred until first use
	Log.Debug(TAG, "FirebaseFirestoreService created (lazy initialization)");
}
```

**Changed Calls:**
- `_db.Collection()` → `GetFirestoreInstance().Collection()`
- Updated 4 locations in methods: `GetAllLocationsAsync()`, `GetLocationByIdAsync()`, `GetEventsAsync()`, `GetNotificationsAsync()`

### Why This Works
- Constructor completes without touching Firebase
- Service is registered and instantiated at app startup ✓
- When the first method calls `GetFirestoreInstance()`, Firebase is already initialized ✓
- Thread-safe with double-check locking pattern ✓

---

## Fix #2: Routing Error
**Error Message:** "Relative routing to shell elements is currently not supported. Try prefixing your uri with //:  //scanner"

### Root Cause
- In `HomeViewModel.cs`, navigation used a **relative route**: `"scanner"`
- MAUI Shell requires **absolute routes** starting with `//`

### Solution Applied
**File:** `CampusGuide/ViewModels/HomeViewModel.cs`

**Before:**
```csharp
[RelayCommand]
public async Task GoToScanner()
{
	await Shell.Current.GoToAsync("scanner");  // Relative routing - not supported
}
```

**After:**
```csharp
[RelayCommand]
public async Task GoToScanner()
{
	await Shell.Current.GoToAsync("//scanner");  // Absolute routing
}
```

### Why This Works
- `//` prefix tells MAUI Shell to navigate from the root level
- Matches the route defined in `AppShell.xaml`: `<ShellContent Route="scanner" ... />`
- This is the standard pattern for MAUI Shell navigation

---

## Global Exception Handling (Applied Previously)

Additionally, the following exception handling was added to gracefully catch and log any remaining errors:

**File:** `CampusGuide/App.xaml.cs`
- Catches `AppDomain` unhandled exceptions
- Catches `TaskScheduler` unobserved task exceptions
- Catches Android-specific runtime exceptions
- Logs all exceptions to Debug output and Android Logcat
- Shows user-friendly error dialogs

**File:** `CampusGuide/Platforms/Android/MainActivity.cs`
- Enhanced error handling in `OnCreate()`, `OnStart()`, `OnResume()`
- Graceful Firebase initialization failure (app continues if Firebase fails)
- Detailed logging for debugging

---

## Testing Checklist

After these fixes, the app should:

- ✅ Launch without the Firebase initialization error
- ✅ Display the Home page with buttons
- ✅ Navigate to the Scanner page when "Scan QR Code" is tapped
- ✅ All exceptions are logged (check Android Logcat for "CampusGuide" tag)
- ✅ User-friendly error dialogs appear for any runtime exceptions

---

## Files Modified

1. `CampusGuide/ViewModels/HomeViewModel.cs` - Routing fix
2. `CampusGuide/Services/FirebaseFirestoreService.cs` - Lazy initialization (4 locations updated)
3. `CampusGuide/App.xaml.cs` - Global exception handling (already applied)
4. `CampusGuide/Platforms/Android/MainActivity.cs` - Enhanced lifecycle logging (already applied)
5. `CampusGuide/MauiProgram.cs` - Logging order fix (already applied)

---

## Build Status
✅ Build Successful - All changes compile without errors
