# CampusGuide Release Checklist v1.0.0

**Release Date:** November 2026  
**Version:** 1.0.0  
**Status:** Ready for Production Review

---

## 📋 Pre-Release Verification

### Code Quality & Build

- [ ] **Clean Build Succeeded**
  - [ ] `dotnet clean` completed without errors
  - [ ] `dotnet build -c Release` succeeded for all target frameworks
  - [ ] No compiler warnings (or all warnings documented/justified)
  - [ ] Build time acceptable (< 5 minutes)

- [ ] **Static Analysis**
  - [ ] No critical code analysis violations
  - [ ] Code style consistency verified
  - [ ] All TODOs/FIXMEs resolved or tracked as issues
  - [ ] Dead code removed

- [ ] **XAML Compilation**
  - [ ] All XAML pages compile without errors
  - [ ] XAML namespaces correctly configured
  - [ ] Data bindings validated
  - [ ] Resource dictionary merges successful

### Dependencies & Packages

- [ ] **NuGet Packages**
  - [ ] All packages updated to stable versions
  - [ ] No deprecated packages in use
  - [ ] Dependency conflicts resolved
  - [ ] License compliance verified for all packages

- [ ] **Framework Versions**
  - [ ] .NET 10 SDK properly installed
  - [ ] MAUI workload up-to-date
  - [ ] Android SDK / iOS SDK versions correct
  - [ ] Target frameworks match requirements:
	- [ ] `net10.0-android` (API 24+)
	- [ ] `net10.0-ios` (iOS 13+)
	- [ ] `net10.0-windows` (Windows 11+)

### Configuration

- [ ] **Firebase Setup**
  - [ ] `google-services.json` present and valid (Android)
  - [ ] `GoogleService-Info.plist` present and valid (iOS)
  - [ ] Firebase API keys configured correctly
  - [ ] Firestore project ID verified
  - [ ] Authentication enabled

- [ ] **API Endpoints**
  - [ ] All external API endpoints configured
  - [ ] API keys/tokens not hardcoded in source
  - [ ] Development vs. production URLs correct
  - [ ] SSL/TLS verification enabled

- [ ] **Build Configuration**
  - [ ] Release build is optimized (tier 1 trim, linking enabled)
  - [ ] Debug symbols removed from release builds
  - [ ] AOT compilation settings appropriate
  - [ ] Platform-specific settings validated

---

## 🔐 Security Review

### Data & Privacy

- [ ] **Firestore Security Rules**
  - [ ] Rules reviewed by security team
  - [ ] Public data (locations, events) properly read-only
  - [ ] User data (favorites, preferences) owner-only
  - [ ] Admin operations properly restricted
  - [ ] Rules tested in Firestore Rules Simulator

- [ ] **Authentication**
  - [ ] Authentication flow tested end-to-end
  - [ ] User sessions properly managed
  - [ ] Logout properly clears sensitive data
  - [ ] Password requirements (if applicable) enforced
  - [ ] Multi-factor authentication supported

- [ ] **Data Protection**
  - [ ] Sensitive data (API keys) not logged
  - [ ] User passwords never stored in logs
  - [ ] Encryption enabled for data at rest
  - [ ] HTTPS/TLS enforced for all network calls
  - [ ] Local storage permissions properly requested

### API Security

- [ ] **Input Validation**
  - [ ] User input sanitized before Firestore queries
  - [ ] Search queries escaped properly
  - [ ] URL parameters validated
  - [ ] QR code content validated before navigation

- [ ] **API Key Management**
  - [ ] API keys restricted in Firebase Console
  - [ ] API key permissions scoped appropriately
  - [ ] No test/debug keys in production build
  - [ ] Key rotation strategy documented

- [ ] **Network Security**
  - [ ] Certificate pinning enabled (if applicable)
  - [ ] SSL verification enabled
  - [ ] No hardcoded credentials
  - [ ] Rate limiting configured
  - [ ] CORS headers properly set

### Permissions & Privacy

- [ ] **Requested Permissions**
  - [ ] Camera (QR scanning) justified
  - [ ] Location (maps) justified
  - [ ] Storage permissions minimized
  - [ ] Runtime permissions properly implemented
  - [ ] Permission denials handled gracefully

- [ ] **Privacy Policy**
  - [ ] Privacy policy written and reviewed
  - [ ] Privacy policy displayed in app (if required)
  - [ ] GDPR compliance verified
  - [ ] Data retention policies defined
  - [ ] User data export/deletion supported

---

## 🧪 Functional Testing

### Core Features

- [ ] **Map & Navigation**
  - [ ] Map displays campus locations correctly
  - [ ] Map clustering works on zoom
  - [ ] Pin tapping shows location details
  - [ ] Map responds to rotation/gestures

- [ ] **Location Search**
  - [ ] Search returns relevant results
  - [ ] Filters (category, distance, amenities) work
  - [ ] Search is responsive and performant
  - [ ] No results handled gracefully

- [ ] **Floor Plans**
  - [ ] Floor plans load for all buildings
  - [ ] Building/floor selector functions
  - [ ] Location overlays display correctly
  - [ ] Navigation between floors smooth
  - [ ] Fallback for missing floor plans works

- [ ] **QR Scanning**
  - [ ] Camera permission requested properly
  - [ ] QR codes scanned successfully
  - [ ] Scanned data validated before use
  - [ ] Navigation to location works
  - [ ] Invalid QR codes handled gracefully

- [ ] **Events & Calendar**
  - [ ] Events display with all information
  - [ ] Filtering by category/date works
  - [ ] Event details page complete
  - [ ] No events state handled

- [ ] **Favorites & Bookmarks**
  - [ ] Saving favorites works
  - [ ] Favorites persist across sessions
  - [ ] Removing favorites works
  - [ ] Favorites page displays all saved items

- [ ] **Notifications**
  - [ ] Alert notifications display
  - [ ] Notifications not shown for disabled alerts
  - [ ] Notification tapping navigates correctly
  - [ ] Notification permissions requested

### Platform-Specific

- [ ] **Android**
  - [ ] App installs from APK/AAB
  - [ ] Touches and gestures responsive
  - [ ] Back button handled correctly
  - [ ] System back button works
  - [ ] Rotation/orientation changes handled
  - [ ] Notifications display in notification tray
  - [ ] Tested on API 24+ devices

- [ ] **iOS**
  - [ ] App installs and runs
  - [ ] Touch gestures responsive
  - [ ] Status bar styled correctly
  - [ ] Safe area respected
  - [ ] Rotation handled gracefully
  - [ ] Tested on iOS 13+ devices

- [ ] **Windows**
  - [ ] App window resizes properly
  - [ ] Keyboard navigation works
  - [ ] Mouse/trackpad gestures work
  - [ ] Window close handled

### Performance

- [ ] **Load Times**
  - [ ] App startup < 3 seconds
  - [ ] Map loads within 2 seconds
  - [ ] Search results within 1 second
  - [ ] Floor plans load within 2 seconds

- [ ] **Memory Usage**
  - [ ] No memory leaks after 30 min usage
  - [ ] Memory stable during map panning
  - [ ] Images cached efficiently
  - [ ] Collection views virtualized

- [ ] **Battery/Network**
  - [ ] Location tracking (if any) efficient
  - [ ] Network requests batched where possible
  - [ ] No excessive API calls
  - [ ] Background operations minimized

---

## 📱 Device Testing

### Test Devices

- [ ] **Android**
  - Device Model: _________________ API Level: _____
  - Device Model: _________________ API Level: _____
  - Device Model: _________________ API Level: _____

- [ ] **iOS**
  - Device Model: _________________ iOS Version: _____
  - Device Model: _________________ iOS Version: _____

- [ ] **Windows**
  - OS Version: _________________ Screen: _____

### Connectivity Scenarios

- [ ] **Network Conditions**
  - [ ] Tested on WiFi
  - [ ] Tested on 4G/LTE
  - [ ] Tested on slow network (throttled)
  - [ ] Tested offline with graceful degradation

---

## 📦 Build Artifacts

### APK/AAB (Android)

- [ ] **APK Generation**
  - [ ] Signed APK generated
  - [ ] APK size acceptable (< 150 MB)
  - [ ] Signing certificate valid
  - [ ] APK installs and runs on test device
  - [ ] ProGuard/R8 rules correct

- [ ] **AAB Generation**
  - [ ] Android App Bundle generated
  - [ ] Size acceptable (< 100 MB base)
  - [ ] Dynamic delivery tested
  - [ ] All configurations included

### IPA (iOS)

- [ ] **Code Signing**
  - [ ] Development certificate valid
  - [ ] Provisioning profile current
  - [ ] Bundle ID matches configuration
  - [ ] Build configuration correct

- [ ] **App Store Package**
  - [ ] IPA generated successfully
  - [ ] IPA size acceptable
  - [ ] Bitcode handling correct
  - [ ] App thins properly

### Windows MSIX

- [ ] **Package Generation**
  - [ ] MSIX generated successfully
  - [ ] Certificate configured
  - [ ] Package size acceptable
  - [ ] App installs from MSIX

---

## 📚 Documentation

- [ ] **README.md**
  - [ ] Setup instructions complete and tested
  - [ ] Feature descriptions accurate
  - [ ] Architecture documented
  - [ ] Troubleshooting section helpful
  - [ ] Links functional (no 404s)

- [ ] **Security Rules**
  - [ ] FIRESTORE_SECURITY_RULES.txt reviewed
  - [ ] Rules deployment process documented
  - [ ] Admin setup instructions clear
  - [ ] Testing procedures included

- [ ] **Release Notes**
  - [ ] Version 1.0.0 features listed
  - [ ] Known limitations documented
  - [ ] Bug fixes noted
  - [ ] Contributors credited

- [ ] **Code Documentation**
  - [ ] Public APIs have XML doc comments
  - [ ] Complex logic has explanatory comments
  - [ ] ViewModels document properties
  - [ ] Services document contracts

---

## 🔄 Git & Version Control

- [ ] **Repository State**
  - [ ] All changes committed
  - [ ] No uncommitted files (`git status` clean)
  - [ ] Latest changes pushed to origin
  - [ ] Release branch created or main branch prepared

- [ ] **Tagging**
  - [ ] Release tag created: `v1.0.0`
  - [ ] Tag message includes release notes
  - [ ] Tag pushed to remote
  - [ ] Release notes published on GitHub

- [ ] **Versioning**
  - [ ] Version number updated in:
	- [ ] CampusGuide.csproj (Version element)
	- [ ] App.xaml.cs (AppVersion if exists)
	- [ ] About page version label
  - [ ] Version consistent across all files
  - [ ] Previous versions documented

---

## 🚀 Deployment

### Pre-Deployment

- [ ] **Final Review**
  - [ ] Product Manager approved
  - [ ] Security team approved
  - [ ] Architecture team approved
  - [ ] All blocking issues resolved

- [ ] **Backup & Rollback**
  - [ ] Database backup taken
  - [ ] Previous version buildable and deployable
  - [ ] Rollback procedure documented
  - [ ] Communication plan prepared

### Google Play Store (Android)

- [ ] **Store Setup**
  - [ ] Developer account active
  - [ ] App listing complete with:
	- [ ] Screenshots (3-5)
	- [ ] Feature graphic
	- [ ] Description
	- [ ] Privacy policy link
	- [ ] Support email
  - [ ] Pricing and distribution configured
  - [ ] Content rating completed

- [ ] **Upload & Review**
  - [ ] AAB uploaded to internal testing
  - [ ] Internal test passed
  - [ ] AAB uploaded to closed testing (alpha)
  - [ ] Beta review completed
  - [ ] AAB uploaded to production
  - [ ] Store listing review completed
  - [ ] App approved and live

### Apple App Store (iOS)

- [ ] **Store Setup**
  - [ ] App ID created in App Store Connect
  - [ ] Bundle ID matches signing
  - [ ] App information complete
  - [ ] Privacy policy linked
  - [ ] Support contact provided
  - [ ] Category selected

- [ ] **TestFlight & Review**
  - [ ] TestFlight build uploaded
  - [ ] Internal testers tested
  - [ ] External testers invited
  - [ ] Beta feedback incorporated
  - [ ] Production build uploaded
  - [ ] App review metadata complete
  - [ ] App submitted for review
  - [ ] App approved (Apple's timeline ~24-48 hours)

### Windows Store (Microsoft Store)

- [ ] **Partner Center Setup**
  - [ ] App created in Partner Center
  - [ ] App information complete
  - [ ] Certification process understood
  - [ ] Privacy policy provided

- [ ] **Submission**
  - [ ] MSIX package uploaded
  - [ ] Store listing complete
  - [ ] Screenshots provided
  - [ ] Submission reviewed and uploaded

---

## ✅ Post-Release

- [ ] **Monitoring**
  - [ ] Error logs monitored
  - [ ] Crash reports reviewed
  - [ ] User feedback collected
  - [ ] Performance metrics tracked

- [ ] **Support**
  - [ ] Support team notified
  - [ ] FAQ updated
  - [ ] Known issues documented
  - [ ] Contact channels monitored

- [ ] **Analytics**
  - [ ] Google Analytics events configured
  - [ ] Firebase Analytics events firing
  - [ ] User engagement metrics tracked
  - [ ] Conversion funnels defined

---

## 📝 Sign-Off

| Role | Name | Date | Status |
|------|------|------|--------|
| Developer | ________________ | __________ | ☐ Approved |
| QA Lead | ________________ | __________ | ☐ Approved |
| Security | ________________ | __________ | ☐ Approved |
| Product Owner | ________________ | __________ | ☐ Approved |
| Release Manager | ________________ | __________ | ☐ Approved |

---

## 🔗 Related Documents

- [README.md](README.md) - Project overview and setup
- [FIRESTORE_SECURITY_RULES.txt](FIRESTORE_SECURITY_RULES.txt) - Security rules
- [PHASE_18_FLOOR_PLANS_DOCUMENTATION.md](PHASE_18_FLOOR_PLANS_DOCUMENTATION.md) - Phase 18 details
- GitHub Repository: https://github.com/zafaroful/CampusGuide

---

**Release Version:** 1.0.0  
**Release Manager:** ___________________  
**Release Date:** ___________________  
**Status:** ☐ Released ☐ Pending ☐ On Hold
