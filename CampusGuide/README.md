# CampusGuide - Campus Navigation & Discovery Application

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-.NET%2010%20MAUI-purple.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

CampusGuide is a comprehensive mobile application designed to help students, faculty, and visitors navigate and explore the campus efficiently. With interactive maps, location search, QR code scanning, indoor floor planning, and event discovery, CampusGuide transforms campus exploration into an intuitive, user-friendly experience.

## 🌟 Key Features

- **Interactive Campus Map** - Real-time campus map with location clustering and custom markers
- **Advanced Location Search** - Multi-criteria search with filters by category, distance, and amenities
- **Indoor Floor Plans** - Detailed building floor plans with interactive location overlays
- **QR Code Scanning** - Scan QR codes to quickly access location details and information
- **Event Calendar** - Browse and filter upcoming campus events and activities
- **Favorites & Bookmarks** - Save frequently visited locations for quick access
- **Real-time Notifications** - Stay updated with campus alerts and event announcements
- **Building & Floor Filtering** - Organize locations by building and floor level
- **Responsive Design** - Optimized for mobile, tablet, and desktop platforms

## 📱 Platform Support

- **Android** (API 24+)
- **iOS** (13.0+)
- **Windows** (11+)

Developed with .NET 10 MAUI (Multi-platform App UI) for a native, performant user experience across all platforms.

## 🏗️ Project Architecture

### Technology Stack

```
Framework:     .NET 10 MAUI
Database:      Google Firebase (Firestore)
Maps:          MAUI.Controls.Maps (Native)
QR Scanning:   ZXing.Net.MAUI + Google ML Kit
State:         CommunityToolkit.Mvvm
HTTP:          HttpClient (built-in)
Logging:       System.Diagnostics
Preferences:   MAUI.Storage
```

### Directory Structure

```
CampusGuide/
├── App.xaml[.cs]              # MAUI App entry point
├── AppShell.xaml[.cs]         # Shell navigation container
├── MauiProgram.cs             # DI & service registration
├── Views/
│   ├── Home/
│   │   ├── HomePage.xaml[.cs]
│   │   └── QRScannerPage.xaml[.cs]
│   ├── Map/
│   │   ├── CampusMapPage.xaml[.cs]
│   │   └── FloorPlanPage.xaml[.cs]
│   ├── Search/
│   │   └── SearchPage.xaml[.cs]
│   ├── Events/
│   │   ├── EventsPage.xaml[.cs]
│   │   └── EventDetailsPage.xaml[.cs]
│   ├── Favorites/
│   │   └── FavoritesPage.xaml[.cs]
│   ├── Notifications/
│   │   └── NotificationsPage.xaml[.cs]
│   └── AboutPage.xaml[.cs]
├── ViewModels/
│   ├── HomeViewModel.cs
│   ├── Map/
│   │   ├── CampusMapViewModel.cs
│   │   └── FloorPlanViewModel.cs
│   ├── Search/
│   │   └── SearchViewModel.cs
│   └── [Other ViewModels]
├── Models/
│   ├── Location.cs
│   ├── Event.cs
│   ├── Building.cs
│   ├── FloorPlan.cs
│   └── [Other Models]
├── Services/
│   ├── IFirestoreService.cs
│   ├── FirebaseFirestoreService.cs
│   ├── LocationService.cs
│   ├── EventService.cs
│   ├── FloorPlanService.cs
│   └── [Other Services]
├── Resources/
│   ├── Styles/
│   │   └── Styles.xaml
│   ├── Images/
│   ├── Fonts/
│   └── floorplans/
│       └── [Building floor plan SVG files]
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   └── Windows/
├── CampusGuide.csproj          # Project configuration
├── CampusGuide.slnx            # Solution file
├── google-services.json        # Firebase configuration (Android)
├── GoogleService-Info.plist    # Firebase configuration (iOS)
├── README.md                   # This file
├── FIRESTORE_SECURITY_RULES.txt
└── RELEASE_CHECKLIST.md
```

### MVVM Architecture

The application follows the **Model-View-ViewModel (MVVM)** pattern:

- **Views** (XAML + Code-behind): User interface pages
- **ViewModels**: Business logic and data management using `CommunityToolkit.Mvvm`
- **Models**: Data structures for locations, events, buildings, etc.
- **Services**: Firebase integration, data access, and external APIs

### Service Layer

**Key Services:**
- `FirebaseFirestoreService`: Firestore data access (read/write)
- `LocationService`: Campus location management and search
- `EventService`: Event retrieval and filtering
- `FloorPlanService`: Floor plan data and location mapping

## 🔧 Setup & Installation

### Prerequisites

- .NET 10 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- Visual Studio 2026 Community or later (with MAUI workload installed)
- Android SDK (API 24+) for Android development
- Xcode (for iOS development)
- Git

### Build & Run

#### 1. Clone the Repository

```bash
git clone https://github.com/zafaroful/CampusGuide.git
cd CampusGuide
```

#### 2. Restore Dependencies

```bash
dotnet restore
```

#### 3. Configure Firebase

**Android:**
- Place `google-services.json` in `CampusGuide/Platforms/Android/`
- [Obtain from Firebase Console](https://firebase.google.com/docs/android/setup)

**iOS:**
- Place `GoogleService-Info.plist` in `CampusGuide/Platforms/iOS/`
- [Obtain from Firebase Console](https://firebase.google.com/docs/ios/setup)

**Windows:**
- Update Firebase REST endpoint in configuration or use Android/iOS credentials

#### 4. Build & Run

```bash
# Debug build for Android
dotnet build -f net10.0-android

# Debug build for iOS
dotnet build -f net10.0-ios

# Debug build for Windows
dotnet build -f net10.0-windows

# Or use Visual Studio IDE to build/debug directly
```

## 🗺️ Firebase Setup

### Firestore Collections

```
locations/           # Campus locations
├── {locationId}
│   ├── name: string
│   ├── category: string
│   ├── latitude: number
│   ├── longitude: number
│   ├── description: string
│   ├── imageUrl: string
│   ├── amenities: array
│   ├── building: string
│   ├── floor: number
│   └── coordinates: array (for map clustering)

events/              # Campus events
├── {eventId}
│   ├── title: string
│   ├── description: string
│   ├── startDate: timestamp
│   ├── endDate: timestamp
│   ├── location: string
│   ├── imageUrl: string
│   ├── category: string
│   └── capacity: number

buildings/           # Building metadata
├── {buildingId}
│   ├── name: string
│   ├── shortCode: string
│   ├── floors: number
│   ├── latitude: number
│   └── longitude: number

users/{uid}/favorites/  # User bookmarks
└── {locationId}: boolean
```

### Security Rules

See `FIRESTORE_SECURITY_RULES.txt` for production-ready Firestore security rules.

**Important:** 
- Review rules before deploying to production
- Ensure authentication is properly configured
- Use role-based access control for administrative operations
- Never expose private/sensitive location data without proper authorization

## 🧪 Testing

### Unit Tests

```bash
# Run all tests
dotnet test

# Run tests from specific project
dotnet test CampusGuide.Tests
```

### Manual Testing

1. **Map Navigation**: Open CampusMapPage and verify markers, clustering, and interaction
2. **Location Search**: Test search filters and result accuracy
3. **QR Scanning**: Scan test QR codes linked to locations
4. **Floor Plans**: Navigate buildings and verify floor plan display
5. **Events**: Check event listing and filtering
6. **Favorites**: Save/remove favorites and verify persistence
7. **Notifications**: Verify alert delivery and display

### Testing with Emulator

**Android Emulator:**
```bash
dotnet run -f net10.0-android
```

**iOS Simulator:**
```bash
dotnet run -f net10.0-ios
```

## 🔐 Security Considerations

### Authentication

- User authentication is delegated to Firebase Authentication
- Implement user sign-up/login via `FirebaseAuth` for personalized features
- Store sensitive user data securely using MAUI.Storage

### Data Privacy

- Locations and events are public by default
- User favorites are stored locally and in Firestore under `users/{uid}/favorites`
- Implement proper Firestore security rules (see `FIRESTORE_SECURITY_RULES.txt`)

### API Security

- Firebase API keys are embedded in `google-services.json` / `GoogleService-Info.plist`
- Restrict API key usage in Firebase Console to prevent abuse
- Use HTTPS for all external API calls
- Validate and sanitize user input before Firestore queries

### QR Code Scanning

- Validate QR code content before navigation
- Prevent malicious redirection by whitelisting allowed URLs
- Log suspicious QR codes for analysis

## 📦 Deployment

### Release Build

```bash
# Clean build for release
dotnet clean
dotnet build -c Release

# Or use Visual Studio: Build > Configuration Manager > Release
```

### APK Generation (Android)

```bash
dotnet publish -f net10.0-android -c Release
# APK location: bin/Release/net10.0-android/
```

### App Store Submission

**Android (Google Play):**
1. Generate signed APK/AAB
2. Configure app signing in Visual Studio
3. Upload to Google Play Console

**iOS (Apple App Store):**
1. Configure code signing and provisioning profiles
2. Archive and upload via Xcode/Transporter
3. Submit for review

**Windows (Microsoft Store):**
1. Configure MSIX packaging
2. Upload to Partner Center
3. Submit for certification

## 🐛 Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| "Could not find google-services.json" | Place file in `Platforms/Android/` |
| "XAML compilation error" | Clean solution, rebuild, check namespace/type names |
| "Map not displaying" | Verify `.UseMauiMaps()` in MauiProgram, check API keys |
| "Firestore connection timeout" | Check internet connection, Firebase project settings |
| "QR scanner not working" | Ensure camera permission granted, verify ZXing/MLKit packages |

### Debug Logging

Enable verbose logging:
```csharp
// In App.xaml.cs or MauiProgram.cs
#if DEBUG
	MauiProgram.CreateMauiApp()
		.ConfigureLogging(logging => logging.AddDebug());
#endif
```

## 📄 License

CampusGuide is licensed under the **MIT License**. See LICENSE file for details.

```
MIT License

Copyright (c) 2026 CampusGuide Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

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
- **Email**: feedback@campusguide.dev

## 🙏 Acknowledgments

- Built with [.NET MAUI](https://github.com/dotnet/maui)
- Maps powered by [MAUI.Controls.Maps](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/map)
- QR Code scanning via [ZXing.Net.MAUI](https://github.com/thudugalik/ZXing.Net.MAUI)
- Community toolkit: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- Firebase integration via [Google Firebase](https://firebase.google.com/)

---

**Last Updated:** November 2026  
**Version:** 1.0.0  
**Status:** Production Ready
