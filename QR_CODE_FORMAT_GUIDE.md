# Phase 10 QR Code Format & Usage Documentation

## Overview

The CampusGuide QR Scanner uses a unified QR code format that enables students to quickly access campus location information by pointing their phone camera at QR codes placed throughout the campus.

## QR Code Format Specification

### Standard QR Code Format

```
campus://location/{LOCATION_ID}
```

### Format Components

| Component | Description | Example | Notes |
|-----------|-------------|---------|-------|
| **Protocol** | Scheme identifier | `campus://` | Custom deep-link scheme |
| **Type** | Resource type | `location` | Currently only location type supported |
| **Location ID** | Unique identifier | `lib-001` | Alphanumeric, case-sensitive |

### Format Rules

- **Length:** Typically 20-30 characters per full QR code string
- **Case Sensitivity:** Location IDs are case-sensitive (`lib-001` ≠ `LIB-001`)
- **Character Set:** Alphanumeric with hyphens (`[a-zA-Z0-9-]`)
- **Special Characters:** Not recommended in Location IDs
- **URL Encoding:** Not required (campus:// is not HTTP-based)

### Valid Examples

```
campus://location/lib-001
campus://location/cafe-001
campus://location/gym-001
campus://location/lab-001
campus://location/sports-001
campus://location/health-center-001
campus://location/it-support-001
```

### Invalid Examples

```
campus://location/         # Missing Location ID
campus://location          # Missing trailing slash
location/lib-001           # Missing protocol
campus://building/lib-001  # Wrong resource type (use "location")
campus://location/lib 001  # Spaces not allowed
campus://location/lib.001  # Special characters
```

## Supported Location IDs

### Pre-seeded Locations (Firestore)

These locations are automatically added when the app runs for the first time:

#### 1. Main Library
```
Location ID: lib-001
QR Code: campus://location/lib-001
Name: Main Library
Type: library
Building: Engineering Block
Floor: 1-3
Coordinates: (1.3521, 103.8198)
Description: Central library with study areas, computer labs, and quiet zones
Tags: library, study, quiet, computers
```

#### 2. Student Cafeteria
```
Location ID: cafe-001
QR Code: campus://location/cafe-001
Name: Student Cafeteria
Type: cafeteria
Building: Student Center
Floor: G (Ground)
Coordinates: (1.3525, 103.8200)
Description: Main cafeteria with variety of food options and seating areas
Tags: food, cafeteria, seating, halal, vegetarian
```

#### 3. Sports Complex
```
Location ID: gym-001
QR Code: campus://location/gym-001
Name: Sports Complex
Type: sports
Building: Recreation Building
Floor: B1-2 (Basement)
Coordinates: (1.3530, 103.8195)
Description: Multi-purpose sports facility with gym, courts, and pool
Tags: sports, gym, fitness, court, swimming
```

#### 4. Computer Science Lab
```
Location ID: lab-001
QR Code: campus://location/lab-001
Name: Computer Science Lab
Type: lab
Building: Engineering Block
Floor: 2
Coordinates: (1.3518, 103.8202)
Description: State-of-the-art computer lab for programming and AI research
Tags: lab, computer, programming, ai, research
```

### Adding New Locations

To add new campus locations with QR codes:

#### 1. Update Firestore Data Seeder
Edit: `CampusGuide/Services/FirestoreDataSeeder.cs`

Add new location to `SeedLocationsAsync()`:
```csharp
new LocationModel
{
	Id = "new-location-001",                    // Unique ID
	Name = "New Location Name",
	Description = "Detailed description",
	Type = "location_type",                     // Type: library, cafe, sports, lab, etc.
	Building = "Building Name",
	Floor = "Level",
	Coordinates = new Coordinates 
	{ 
		Latitude = 1.3521, 
		Longitude = 103.8198 
	},
	Images = new List<string> { "image.jpg" },
	Tags = new List<string> { "tag1", "tag2" },
	QrCodeId = "campus://location/new-location-001"  // Must match QR code format
}
```

#### 2. Update Location Model (if needed)
Edit: `CampusGuide/Models/Location.cs`

Ensure all required fields are present:
```csharp
public class Location
{
	public string Id { get; set; }              // Unique ID
	public string Name { get; set; }            // Display name
	public string Description { get; set; }
	public string Type { get; set; }            // Category type
	public string Building { get; set; }
	public string Floor { get; set; }
	public Coordinates Coordinates { get; set; }
	public List<string> Images { get; set; }
	public List<string> Tags { get; set; }
	public string QrCodeId { get; set; }        // QR code string
}
```

#### 3. Generate QR Code
Use an online QR code generator or command-line tool:

**Option A: Online Tool**
1. Visit: https://www.qr-code-generator.com
2. Data format: `campus://location/{LOCATION_ID}`
3. Download as PNG
4. Print and place on campus

**Option B: PowerShell (Windows)**
```powershell
# Install QR code generator (requires .NET)
dotnet tool install -g QRCoder.CLI

# Generate QR code
qrcoder -t "campus://location/lib-001" -o "qr_lib_001.png"
```

**Option C: Python**
```python
import qrcode

qr = qrcode.QRCode(version=1, box_size=10, border=5)
qr.add_data('campus://location/lib-001')
qr.make(fit=True)
img = qr.make_image(fill_color="black", back_color="white")
img.save('qr_lib_001.png')
```

## QR Code Generation & Deployment

### Design Specifications

#### Size & Resolution
- **Minimum Size:** 21x21 pixels per QR module
- **Recommended Size:** Print at 2x2 inches (5x5 cm)
- **High Resolution:** 300 DPI for printing
- **Digital Display:** 100x100 pixels minimum on screen

#### Color & Contrast
- **Standard:** Black (foreground) on White (background)
- **Contrast Ratio:** Minimum 7:1 for accessibility
- **Avoid:** Colored QR codes (may reduce scanability)

#### Quiet Zone
- **Margin:** Minimum 4 modules white space around all sides
- **Purpose:** Ensures scanner can detect code
- **Recommendation:** Leave full white space border

### Placement Guidelines

#### Physical Locations (Campus)
- **Entrance Areas:** Place at eye level (1.2-1.5 meters)
- **Information Boards:** Mount centrally, clear of obstructions
- **Multiple Locations:** Use 2-3 copies for high-traffic areas
- **Durability:** Use weatherproof labels or lamination
- **Updates:** Refresh codes annually or when location changes

#### Digital Locations (In-App)
- **Feature Not Yet Implemented** - Reserved for future enhancement
- **Use Case:** Display in event details, location cards
- **Size:** 150x150 pixels minimum

### Distribution

#### Print Template
```
┌─────────────────────┐
│                     │
│    LIBRARY - LIB001 │
│      [QR CODE]      │
│                     │
│ Scan to learn more  │
│                     │
└─────────────────────┘
```

#### Batch Generation Script (PowerShell)

```powershell
$locations = @(
	@{id="lib-001"; name="Main Library"},
	@{id="cafe-001"; name="Cafeteria"},
	@{id="gym-001"; name="Sports Complex"}
)

foreach ($loc in $locations) {
	$qrValue = "campus://location/$($loc.id)"
	Write-Host "Generating QR for $($loc.name): $qrValue"
	# Use your preferred QR generator here
}
```

## QR Code Scanning & Recognition

### Supported QR Code Standards
- **Format:** QR Code (ISO 18004)
- **Version:** 1-40 (automatically determined by data length)
- **Error Correction:** Level L (7% recovery)
- **Encoding:** UTF-8 text

### Scanner Behavior

#### Successful Scan Flow
```
User Points Camera → QR Detected → Format Extracted → 
Location ID Parsed → Firestore Query → Location Found → 
Navigate to Details
```

#### Error Handling

| Error | Cause | Resolution |
|-------|-------|-----------|
| **Invalid Format** | QR code doesn't match campus:// format | Check QR code format and regenerate |
| **Location Not Found** | Location ID not in Firestore | Add location to database |
| **Permission Denied** | Camera permission not granted | Grant permission when prompted |
| **Firestore Offline** | No internet connection | Connect to network and retry |
| **Navigation Error** | Route configuration issue | Verify AppShell routes |

### Testing QR Codes

#### Test Without Physical QR Codes
Use the built-in Test Scan feature:
1. Navigate to QR Scanner
2. Click "Test Scan" button
3. Default test code: `campus://location/lib-001`
4. Verify location details load

#### Modify Test Scan for Other Locations
Edit: `CampusGuide/Views/Home/QRScannerPage.xaml.cs`

In `OnTestScanClicked()` method:
```csharp
var testQRValue = "campus://location/lib-001";  // Change this to test other locations
```

Replace with:
```csharp
var testQRValue = "campus://location/cafe-001";  // Test cafeteria
// or
var testQRValue = "campus://location/gym-001";   // Test sports complex
// or
var testQRValue = "campus://location/lab-001";   // Test computer lab
```

## Integration Points

### Code Integration

#### QRScannerPage Processing
Location: `CampusGuide/Views/Home/QRScannerPage.xaml.cs`

Method: `ExtractLocationIdFromQRCode(string qrValue)`
```csharp
// Handles URL parsing:
// Input: "campus://location/lib-001"
// Output: "lib-001"

private string ExtractLocationIdFromQRCode(string qrValue)
{
	if (string.IsNullOrWhiteSpace(qrValue))
		return string.Empty;

	qrValue = qrValue.Trim();

	// Handle campus:// URL
	if (qrValue.Contains("://"))
	{
		try
		{
			var uri = new Uri(qrValue);
			var segments = uri.Segments;
			if (segments.Length > 0)
			{
				return segments[^1].TrimEnd('/');
			}
		}
		catch { }
	}

	// Return as-is if already just ID
	return qrValue;
}
```

### Firestore Integration

#### Service: IFirestoreService
Location: `CampusGuide/Services/IFirestoreService.cs`

Query method:
```csharp
public interface IFirestoreService
{
	Task<Location> GetLocationByIdAsync(string locationId);
}
```

Usage:
```csharp
var location = await _firestoreService.GetLocationByIdAsync(locationId);
if (location != null)
{
	// Navigate to details
	await Shell.Current.GoToAsync($"location-details?id={locationId}");
}
```

### Navigation Integration

#### Route: scanner
Defined in: `CampusGuide/AppShell.xaml`
```xml
<ShellContent Route="scanner" ContentTemplate="{DataTemplate home:QRScannerPage}" />
```

#### Route: location-details
Defined in: `CampusGuide/AppShell.xaml`
```xml
<ShellContent Route="location-details" ContentTemplate="{DataTemplate map:LocationDetailsPage}" />
```

## Best Practices

### QR Code Design
✅ **DO:**
- Use standard black & white contrast
- Generate at high resolution (300 DPI)
- Include location name label below code
- Test code before printing
- Use weatherproof labels outdoors

❌ **DON'T:**
- Add complex logos or images inside code
- Use colors other than black/white
- Reduce error correction level
- Omit quiet zone (white margin)
- Place in direct sunlight with reflections

### Location ID Naming
✅ **Recommended:**
```
{type}-{number}
lib-001, cafe-001, gym-001
office-a-001, lab-north-001
```

❌ **Avoid:**
```
library, Cafeteria, GYM
Location_1, Place1, Area-A
lib.001, "cafe", <special>
```

### Firestore Data Quality
✅ **Always Include:**
- Unique ID matching QR code
- Display name (friendly)
- Description (helpful context)
- Location type (category)
- Coordinates (for map)
- Tags (for search)

❌ **Never Leave Blank:**
- Name or ID
- QR code ID (must match code content)

## Future Enhancements

### Planned Features
1. **Dynamic QR Code Generation**
   - Generate QR codes from admin panel
   - No manual printing needed
   - Auto-update when location changes

2. **Multiple QR Format Support**
   - HTTP URLs: `https://campusguide.edu/location/lib-001`
   - Barcode support: EAN-13, Code-128
   - NFC tag integration

3. **QR Analytics**
   - Track scan counts by location
   - Heat map of most scanned areas
   - User statistics dashboard

4. **Real-time Camera Integration**
   - Live QR detection (placeholder currently)
   - Auto-navigate on detection
   - Multi-code detection

5. **Accessibility**
   - Voice announcements on scan
   - Haptic feedback on successful scan
   - Text size customization

## Troubleshooting

### QR Code Not Scanning

**Problem:** Camera can't read QR code  
**Solutions:**
1. Check code contrast (black on white)
2. Verify code not damaged
3. Ensure adequate lighting
4. Reduce camera distance
5. Regenerate code if faded

### Location Not Found

**Problem:** QR scans but shows "Location Not Found"  
**Solutions:**
1. Verify Location ID matches QR content
2. Check Firestore database has the location
3. Verify spelling (case-sensitive)
4. Ensure internet connection to Firestore
5. Check app has internet permissions

### Permission Errors

**Problem:** "Permission Denied" alert appears  
**Solutions:**
1. Tap "Allow" when permission dialog appears
2. Check Settings → Apps → CampusGuide → Permissions
3. Enable Camera permission manually
4. Uninstall and reinstall app if stuck

## Reference Resources

### QR Code Standards
- **ISO 18004:2015** - QR Code specification
- **QR Code.com** - Official QR code documentation
- **ZXing Project** - Open-source barcode library used

### Tools & Generators
- **QR Code Generator** - https://www.qr-code-generator.com
- **QR Code Monkey** - https://www.qr-code-monkey.com
- **Segno (Python)** - Python QR code library
- **qrcoder (C#)** - .NET QR code library

### MAUI QR Integration
- **ZXing.Net.MAUI** - NuGet package used (v0.4.0)
- **MAUI Documentation** - https://learn.microsoft.com/maui
- **Shell Navigation** - Deep linking reference

## Support & Contact

### For Issues or Questions
- Check the QR code format first: `campus://location/{ID}`
- Verify location exists in Firestore
- Review logs in Debug output
- Test with "Test Scan" button in app

### For QR Code Printing
- Use high-contrast black & white printing
- Recommended size: 2x2 inches minimum
- Use matte finish (reduces glare)
- Laminate for durability

---

**Documentation Version:** 1.0  
**Last Updated:** January 2025  
**Status:** ✅ Complete  
**Related Files:**
- CampusGuide/Views/Home/QRScannerPage.xaml.cs
- CampusGuide/Services/FirestoreDataSeeder.cs
- CampusGuide/Models/Location.cs
- CampusGuide/AppShell.xaml

**QR Format:** `campus://location/{LOCATION_ID}`  
**Example:** `campus://location/lib-001`
