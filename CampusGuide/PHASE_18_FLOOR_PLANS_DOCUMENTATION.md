# Phase 18: Floor Plans & Indoor Mapping - Implementation Summary

## Overview

Phase 18 implements indoor floor plan visualization and navigation within the CampusGuide application. Users can now view detailed floor layouts for campus buildings, select specific floors, and navigate to location details from floor plan views. This phase complements the building-level navigation from Phase 17 with floor-level granularity.

## Architecture & Design

### Core Components

#### 1. **FloorPlan Model** (`CampusGuide/Models/FloorPlan.cs`)
Represents a single floor plan for a building, encapsulating metadata and location positioning.

**Key Properties:**
- `FloorPlanId`: Unique identifier
- `BuildingName`, `FloorLevel`: Building and floor identifiers
- `ImagePath`: Resource path to floor plan image (e.g., `floorplans/academic_block_a_1.svg`)
- `IsAvailable`: Whether the floor plan image exists in resources
- `ImageWidth`, `ImageHeight`: Dimensions for coordinate calculations
- `AspectRatio`: Width/height ratio
- `LocationPositions`: Dictionary mapping Location IDs to normalized (0-1) X,Y positions
- `LocationIds`, `LocationCount`: Location collection metadata
- `DisplayName`, `Description`: UI-friendly labels

**Key Methods:**
- `Create(buildingName, floorLevel)`: Factory method generating normalized image path
- `AddLocation(locationId, position)`: Register location with normalized coordinates
- `GetLocationPosition(locationId)`: Retrieve normalized position
- `GetPixelPosition(locationId, width, height)`: Convert to image pixel coordinates
- `IsValid()`: Validate floor plan integrity

**Image Path Convention:**
- Format: `floorplans/{building}_{floor}.svg`
- Example: `floorplans/academic_block_a_1.svg`
- Building/floor names normalized to lowercase with underscores

#### 2. **FloorPlanService** (`CampusGuide/Services/FloorPlanService.cs`)
Service for retrieving, creating, and managing floor plan data with caching and resource handling.

**Key Responsibilities:**
- **Caching**: Dictionary-based cache keyed by `{buildingName}_{floorLevel}`
- **Floor Plan Retrieval**: `GetFloorPlansForBuilding()` discovers unique floors from location data
- **Location Filtering**: `GetLocationsOnFloor()` filters locations by floor
- **Image Availability**: `CheckFloorPlanImageExists()` validates resource availability with fallback paths
- **Position Generation**: `GenerateLocationPosition()` creates deterministic pseudo-random positions from location ID hash
- **Statistics**: `GetStatistics()` computes availability metrics
- **Formatting**: Floor display helpers for UI

**Method Signatures:**
```csharp
List<FloorPlan> GetFloorPlansForBuilding(string buildingName, List<Location> locations)
FloorPlan? GetFloorPlan(string buildingName, string floorLevel, List<Location> locations)
FloorPlan? GetOrCreateFloorPlan(string buildingName, string floorLevel, List<Location> locations)
List<Location> GetLocationsOnFloor(List<Location> locations, string buildingName, string floorLevel)
bool CheckFloorPlanImageExists(string imagePath)
void ClearCache()
```

#### 3. **FloorPlanViewModel** (`CampusGuide/ViewModels/FloorPlanViewModel.cs`)
MVVM ViewModel coordinating floor plan navigation and location display.

**Observable Properties** (CommunityToolkit.Mvvm):
- `SelectedBuildingName`: Current building context
- `SelectedFloorLevel`: Currently selected floor
- `AvailableFloorPlans`: ObservableCollection of floor plans for building
- `CurrentFloorPlan`: Currently displayed floor plan
- `LocationsOnFloor`: ObservableCollection of locations on selected floor
- `IsLoading`: Async operation state
- `FloorPlanAvailable`: Whether current floor plan image is available

**Key Methods:**
- `LoadBuildingFloorPlans(string buildingName, List<Location> locations)`: Async loader for building floors
- `SelectFloor(string floorLevel)`: RelayCommand to switch floor and update display
- `GetFloorPlanImage()`: Load image source for current floor
- `GetLocationPinPosition()`: Calculate pin position for overlay rendering
- `GetLocationAtPosition()`: Tap detection for location selection (placeholder)
- `Clear()`: Clean up data on page close

#### 4. **FloorPlanPage** (`CampusGuide/Views/Map/FloorPlanPage.xaml` + `.xaml.cs`)
Native MAUI UI for floor plan display and interaction.

**XAML Layout:**
- **Header**: Building name, floor count, dynamic floor selector buttons
- **Floor Container**: Scrollable floor plan image with overlay canvas (GraphicsView)
- **Loading State**: Activity indicator during data load
- **Unavailable State**: Graceful fallback showing location list when floor plan image missing
- **Footer**: Floor info and location count labels

**Code-Behind Responsibilities:**
- `LoadBuildingFloorPlans()`: Initialize page with building/location data
- `CreateFloorSelectorButtons()`: Dynamically generate floor choice buttons
- `UpdateFloorPlanDisplay()`: Refresh UI based on floor selection
- `UpdateUIVisibility()`: Show/hide containers based on availability
- `LocationsCollectionView_SelectionChanged()`: Navigate to location details on selection
- `ViewModel_PropertyChanged()`: React to ViewModel state changes
- `AddFloorPlanInteraction()`: Gesture recognizer setup for future tap detection

**Navigation Flow:**
1. User selects building on CampusMapPage
2. Clicks "📐 Floor Plans" button
3. FloorPlanPage loads with available floors
4. User selects floor via button
5. ViewModel fetches locations on floor
6. UI displays image (if available) or location list
7. Tap location to navigate to LocationDetailsPage

### Resource Management

**Floor Plan Images:**
- Location: `CampusGuide/Resources/FloorPlans/`
- Format: SVG (scalable, lightweight, platform-agnostic)
- Naming: `{building}_{floor}.svg` (lowercase, underscores)
- Included in `.csproj` as `MauiAsset` with logical name `floorplans/`

**Included Placeholders:**
- `academic_block_a_1.svg`: Floor 1 layout with classrooms, labs, offices
- `academic_block_a_g.svg`: Ground floor layout with common areas, auditorium

**Image Availability Handling:**
- `FloorPlanService.CheckFloorPlanImageExists()` validates resource presence
- If missing: UI shows "⚠️ Floor Plan Unavailable" with location list
- If present: Displays image with optional pin overlay
- Fallback position generation ensures app functions without resources

### Integration Points

#### 1. **CampusMapPage Integration** (`CampusGuide/Views/Map/CampusMapPage.xaml.cs`)
- Added "📐 Floor Plans" button to map controls toolbar
- `OnFloorPlansClicked()` handler validates building selection, passes data to FloorPlanPage
- Navigates via `Shell.Current.GoToAsync("floorplan")`

#### 2. **Service Registration** (`CampusGuide/MauiProgram.cs`)
```csharp
builder.Services.AddSingleton<FloorPlanViewModel>();
builder.Services.AddSingleton<FloorPlanPage>();
```

#### 3. **Navigation Route** (`CampusGuide/AppShell.xaml`)
```xaml
<ShellContent Route="floorplan" ContentTemplate="{DataTemplate map:FloorPlanPage}" />
```

## Data Flow

### Loading Sequence
1. User selects building on map and clicks "Floor Plans" button
2. CampusMapPage retrieves SelectedBuilding and Locations list
3. FloorPlanPage.LoadBuildingFloorPlans() called with building + locations
4. FloorPlanViewModel.LoadBuildingFloorPlans() invoked asynchronously
5. FloorPlanService.GetFloorPlansForBuilding() extracts unique floors from locations
6. For each floor, creates/caches FloorPlan with location positions
7. AvailableFloorPlans collection populated; first floor auto-selected
8. UI updates to show floor selector buttons and initial floor plan

### Floor Selection Flow
1. User taps floor selector button
2. SelectFloor(floorLevel) RelayCommand executed
3. ViewModel calls FloorPlanService.GetFloorPlan()
4. Service filters locations by floor
5. CurrentFloorPlan and LocationsOnFloor collections updated
6. Property change notifications trigger UI refresh
7. Floor buttons styled to highlight selection
8. Display switches between image (if available) and location list

### Location Navigation
1. Floor plan unavailable OR user selects from list
2. LocationsCollectionView_SelectionChanged() fired
3. Selected location cast from CollectionView
4. Shell navigates to `locationdetails?id={locationId}`
5. LocationDetailsPage loads location from Firestore
6. Back button returns to FloorPlanPage

## Limitations & Future Enhancements

### Current Limitations
- **Tap Detection**: Pin tapping on floor plan image not yet implemented due to coordinate mapping complexity
- **Graphic Overlay**: GraphicsView pin drawing placeholder for future implementation
- **Resource Validation**: File.Exists() fallback may not reliably detect MAUI embedded resources at runtime
- **No Floor Plan Editing**: Admin interface for uploading floor plan images not included
- **Static Positions**: Location positions generated deterministically from ID hash; not editable

### Planned Enhancements
1. **Pin Tap Interaction**: Implement coordinate-based location detection on floor plan tap
2. **Custom Pin Rendering**: Draw visual pins/markers on floor plan overlay
3. **Real Floor Plans**: Replace placeholder SVGs with actual campus floor layouts
4. **Position Calibration**: Admin tool to calibrate and save actual location positions
5. **Floor Plan Search**: Search locations within selected floor plan
6. **Multi-Floor Navigation**: Quick navigation between floors with mini-map
7. **Accessibility**: Voice descriptions of floor layouts

## Testing Considerations

### Build-Only Validation (Deferred Runtime Testing)
✅ **Phase 18 Build Status**: 0 errors, 225+ warnings (consistent with Phase 17)
- Compilation verified
- All services registered
- Navigation routes configured
- Type safety confirmed

### Test Data
Test locations from CampusMapViewModel include:
- Library (academic_block_a, floor 3)
- Cafeteria (student_center, floor G)
- Gym (sports_complex, floor 1)
- Lab (academic_block_a, floor 1)
- Auditorium (student_center, floor G)
- Health Center (health_building, floor 1)

**Note:** Runtime testing deferred pending emulator/device deployment. Floor plan functionality relies on:
- Firestore location data availability
- MAUI Maps/resource loading
- Native gesture recognition
- Shell navigation stack

### Manual Testing Checklist (When Runtime Testing Enabled)
- [ ] Select building from map → "Floor Plans" button visible
- [ ] Click "Floor Plans" → FloorPlanPage loads with floor selector
- [ ] Floor selector shows correct floors for building
- [ ] Click floor button → floor plan image loads or location list displays
- [ ] Location list items → tap to navigate to LocationDetailsPage
- [ ] Back button → returns to CampusMapPage with building still selected
- [ ] No floor plan image → graceful degradation to location list
- [ ] Multiple building cycles → cache prevents duplicate service calls
- [ ] Locations display correct building/floor metadata

## File Structure

```
CampusGuide/
├── Models/
│   ├── FloorPlan.cs                           # NEW: Floor plan metadata & positions
│   ├── Location.cs                            # Uses Floor, Building fields
│   └── Building.cs                            # Provides floor list
├── Services/
│   ├── FloorPlanService.cs                    # NEW: Floor plan retrieval & caching
│   ├── BuildingService.cs                     # Complements with building context
│   └── IFirestoreService.cs                   # Location data source
├── ViewModels/
│   ├── FloorPlanViewModel.cs                  # NEW: Floor plan coordination
│   └── Map/CampusMapViewModel.cs              # Provides SelectedBuilding & Locations
├── Views/
│   └── Map/
│       ├── FloorPlanPage.xaml                 # NEW: Floor plan UI
│       ├── FloorPlanPage.xaml.cs              # NEW: Floor plan logic
│       ├── CampusMapPage.xaml.cs              # Updated: Added Floor Plans button
│       └── LocationDetailsPage.xaml.cs        # Navigation target
├── Resources/
│   └── FloorPlans/                            # NEW: Floor plan resources
│       ├── academic_block_a_1.svg             # NEW: Sample floor 1 layout
│       └── academic_block_a_g.svg             # NEW: Sample ground floor layout
├── MauiProgram.cs                             # Updated: DI registration
├── AppShell.xaml                              # Updated: Navigation route
└── CampusGuide.csproj                         # Updated: Asset configuration
```

## Summary

Phase 18 successfully implements indoor floor plan visualization as a natural extension of Phase 17's building navigation. By providing location-specific floor context, users can more intuitively explore the campus at both building and floor levels. The implementation prioritizes graceful degradation (showing location lists when images unavailable) and leverages existing models/services to minimize duplication.

**Key Achievements:**
✅ Floor plan model & service with caching
✅ MVVM ViewModel for coordinated state
✅ Native MAUI UI with dynamic floor selection
✅ Resource integration and fallback handling
✅ Navigation from CampusMapPage
✅ Location selection and routing to details
✅ Clean compilation (0 errors)
✅ Extensible architecture for future enhancements

**Build Status:** ✅ Verified (0 errors, 225 warnings)
**Runtime Status:** ⏸️ Deferred (awaiting emulator/device deployment)
