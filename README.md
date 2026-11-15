# CampusGuide - Campus Navigation & Discovery Application

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-.NET%2010%20MAUI-purple.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

CampusGuide is a mobile application that helps students, faculty, and visitors navigate and explore the campus. It provides an interactive campus map, location search, QR code scanning, floor plans, campus events, favorites, and campus alerts.

## 🌟 Key Features

- **Interactive Campus Map** - Campus map with location clustering and custom markers
- **Location Search** - Search campus locations by name or keyword
- **Floor Plans** - View floor plan images for campus buildings
- **QR Code Scanning** - Scan QR codes in the `campus://location/LOCATION_ID` format to open a location
- **Event Calendar** - Browse upcoming campus events
- **Favorites** - Save locations for quick access on the device
- **Campus Alerts** - View campus notifications

## 📱 Platform Support

- **Android** (API 23+): Full functionality, including Firestore data
- **iOS** (15.0+) and **Mac Catalyst** (15.0+): Targeted by the project, but campus data is not loaded (see Firebase Setup)
- **Windows** (10.0.17763.0+): Targeted by the project, but campus data is not loaded

Developed with .NET 10 MAUI (Multi-platform App UI).

## 🏗️ Project Architecture

### Technology Stack

```
Framework:     .NET 10 MAUI
Database:      Google Firebase Firestore (REST API, Android only)
Auth:          Firebase Authentication anonymous sign-in (REST API)
Maps:          Microsoft.Maui.Controls.Maps (Google Maps on Android)
QR Scanning:   ZXing.Net.MAUI 0.4.0
MVVM:          CommunityToolkit.Mvvm 8.2.0
HTTP:          HttpClient (built-in)
Logging:       Microsoft.Extensions.Logging.Debug (Debug builds)
Favorites:     Local JSON file in the app data directory
```

### Directory Structure

```
CampusGuide/                     # Repository root
├── CampusGuide.slnx             # Solution file
├── FIRESTORE_SECURITY_RULES.txt # Firestore security rules
├── LICENSE
├── README.md
└── CampusGuide/                 # MAUI project
    ├── CampusGuide.csproj       # Target frameworks, packages, and build configuration
    ├── MauiProgram.cs           # DI, Maps, logging, and service registration
    ├── AppShell.xaml            # Tab and route definitions
    ├── Views/
    │   ├── Home/                # HomePage, QRScannerPage
    │   ├── Map/                 # CampusMapPage, FloorPlanPage
    │   ├── Search/              # SearchPage
    │   ├── Events/              # EventsPage, EventDetailsPage
    │   ├── Favorites/           # FavoritesPage
    │   ├── Notifications/       # NotificationsPage
    │   └── AboutPage.xaml
    ├── ViewModels/              # CommunityToolkit.Mvvm view models
    ├── Models/                  # Location, event, notification, and favorite models
    ├── Services/                # Firestore, favorites, notification, and QR scanner services
    ├── Platforms/Android/       # google-services.json
    └── Resources/               # Images, fonts, icons, FloorPlans
```

### MVVM Architecture

The application follows the **Model-View-ViewModel (MVVM)** pattern:

- **Views** (XAML + Code-behind): User interface pages
- **ViewModels**: Business logic and data management using `CommunityToolkit.Mvvm`
- **Models**: Data structures for locations, events, notifications, and favorites
- **Services**: Firebase integration, local storage, QR scanning, and notifications

### Service Layer

**Key Services (registered in `MauiProgram.cs`):**
- `FirebaseFirestoreService`: Firestore REST data access on Android (`NullFirestoreService` on other platforms)
- `ZXingQRCodeScannerService`: QR code scanning
- `PreferencesFavoritesService`: Local favorites storage
- `NotificationService`: Campus notifications

## 🔧 Setup & Installation

### Prerequisites

- .NET 10 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- Visual Studio 2026 Community or later (with MAUI workload installed)
- Android SDK (API 23+) for Android development
- Xcode (for iOS development, macOS only)
- Git

### Build & Run

#### 1. Clone the Repository

```sh
git clone https://github.com/zafaroful/CampusGuide.git
cd CampusGuide
```

#### 2. Restore Dependencies

```sh
dotnet restore
```

#### 3. Configure Keys

**Google Maps (Android):**
- Create `CampusGuide/google-maps.key` containing only the API key. This file is git-ignored.
- If it is missing, the build uses the placeholder `YOUR_GOOGLE_MAPS_API_KEY` and the map will not load.

**Firebase (Android):**
- Place `google-services.json` in `CampusGuide/Platforms/Android/`
- [Obtain from Firebase Console](https://firebase.google.com/docs/android/setup)
- The Firestore project ID and web API key are defined in `Services/FirebaseFirestoreService.cs`.

**iOS and Windows:**
- The project does not load campus data on these platforms. `GoogleService-Info.plist` is not used by the project.

#### 4. Build & Run

```sh
# Debug build for Android
dotnet build -f net10.0-android

# Debug build for iOS (requires macOS)
dotnet build -f net10.0-ios

# Debug build for Windows
dotnet build -f net10.0-windows10.0.19041.0

# Or use Visual Studio IDE to build/debug directly
```

## 🗺️ Firebase Setup

### Firestore Collections

```
locations/           # Campus locations (read by map, search, and QR scanner)
├── {locationId}
│   ├── name: string
│   ├── category: string
│   ├── latitude: number
│   ├── longitude: number
│   ├── description: string
│   ├── imageUrl: string
│   ├── amenities: array
│   ├── building: string
│   └── floor: number

events/              # Campus events (published events are shown)
├── {eventId}
│   ├── title: string
│   ├── description: string
│   ├── startDate: timestamp
│   ├── endDate: timestamp
│   ├── location: string
│   ├── imageUrl: string
│   └── category: string

notifications/       # Campus alerts (fields defined by the NotificationItem model)
```

Favorites are not stored in Firestore. They are saved locally on the device.

### Security Rules

See `FIRESTORE_SECURITY_RULES.txt` for Firestore security rules.

**Important:**
- Review rules before deploying to production
- Ensure anonymous authentication is enabled for the Firebase project
- Use role-based access control for administrative operations
- Never expose private/sensitive location data without proper authorization

## 🧪 Testing

### Unit Tests

No unit test project is included in this solution yet. Add a test project before running `dotnet test`.

### Manual Testing

1. **Map Navigation**: Open CampusMapPage and verify markers, clustering, and interaction
2. **Location Search**: Test search results and selecting a result
3. **QR Scanning**: Scan test QR codes linked to locations, or use the Test Scan button
4. **Floor Plans**: Open a floor plan and verify display
5. **Events**: Check event listing
6. **Favorites**: Save/remove favorites and verify persistence after restarting the app
7. **Notifications**: Check the Alerts tab for campus notifications

### Testing with Emulator

**Android Emulator:**
```sh
dotnet run -f net10.0-android
```

## 🔐 Security Considerations

### Authentication

- The app signs in anonymously through the Firebase Authentication REST API to obtain an ID token for Firestore requests
- User sign-up and login are not implemented

### Data Privacy

- Locations, events, and notifications are read from Firestore
- Favorites are stored locally on the device (`favorites.json` in the app data directory) and are not sent to Firestore
- Implement proper Firestore security rules (see `FIRESTORE_SECURITY_RULES.txt`)

### API Security

- The Firebase web API key is defined in `Services/FirebaseFirestoreService.cs`. Firebase API keys identify the project and are not secret, but they should be restricted in Google Cloud to the Android package name and signing certificate
- The Google Maps key is read from the git-ignored `google-maps.key` file and should be restricted in the same way
- Use HTTPS for all external API calls
- Validate and sanitize user input before Firestore queries

### QR Code Scanning

- Validate QR code content before navigation. The expected format is `campus://location/LOCATION_ID`
- Planned, not yet implemented: URL whitelisting and logging of suspicious QR codes

## 📦 Deployment

### Release Build

```sh
# Clean build for release
dotnet clean
dotnet build -c Release

# Or use Visual Studio: Build > Configuration Manager > Release
```

### APK Generation (Android)

```sh
dotnet publish -f net10.0-android -c Release
# Output location: bin/Release/net10.0-android/publish/
```

### App Store Submission

**Android (Google Play):**
1. Generate a signed AAB or APK
2. Configure app signing in Visual Studio
3. Upload to Google Play Console

**iOS and Windows:**
- Store packaging is not configured in this project yet (`WindowsPackageType` is `None` and no iOS signing profile is set up)

## 🐛 Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| "Could not find google-services.json" | Place file in `Platforms/Android/` |
| "XAML compilation error" | Clean solution, rebuild, check namespace/type names |
| "Map not displaying" | Verify `google-maps.key` exists in `CampusGuide/`, check the key and its Android restrictions, and confirm `.UseMauiMaps()` is in MauiProgram |
| "Firestore connection timeout" | Check internet connection and Firebase project settings |
| "No campus data on iOS or Windows" | Firestore data is only loaded on Android |
| "QR scanner not working" | Ensure camera permission is granted; the scanner uses ZXing.Net.MAUI |

### Debug Logging

Logging is configured in `MauiProgram.cs` for Debug builds:
```csharp
#if DEBUG
builder.Logging.AddDebug();
#endif
```
Output appears in the Visual Studio **Output** window.

## 📄 License

CampusGuide is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Commit changes (`git commit -m 'Add YourFeature'`)
4. Push to branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

### Code Standards

- Follow C# naming conventions (PascalCase for public, camelCase for private)
- Use MVVM pattern for all pages/features
- Include XML documentation comments for public APIs
- Write unit tests for business logic
- Keep XAML clean and use consistent styling

## 📞 Support & Contact

- **Issues**: [GitHub Issues](https://github.com/zafaroful/CampusGuide/issues)
- **Discussions**: [GitHub Discussions](https://github.com/zafaroful/CampusGuide/discussions)

## 🙏 Acknowledgments

- Built with [.NET MAUI](https://github.com/dotnet/maui)
- Maps powered by [Microsoft.Maui.Controls.Maps](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/map)
- QR code scanning via [ZXing.Net.MAUI](https://github.com/Redth/ZXing.Net.MAUI)
- Community toolkit: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- Firebase integration via [Google Firebase](https://firebase.google.com/)

---

**Last Updated:** November 2026  
**Version:** 1.0.0
