# Firestore Test Data - Manual Import Guide

## Overview
This document contains all test data that should be added to your Firestore database.
You can either:
1. **Manual Import** (recommended for quick setup) - Copy-paste JSON documents via Firebase Console
2. **Programmatic Import** - Use the `FirestoreDataSeeder` service

---

## 📍 Collection: `locations`

### Document 1: lib-001 (Main Library)
```json
{
  "id": "lib-001",
  "name": "Main Library",
  "description": "Central library with study areas, computer labs, and quiet zones",
  "type": "library",
  "building": "Engineering Block",
  "floor": "1-3",
  "coordinates": {
	"latitude": 1.3521,
	"longitude": 103.8198
  },
  "images": [
	"library1.jpg",
	"library2.jpg"
  ],
  "tags": [
	"library",
	"study",
	"quiet",
	"computers"
  ],
  "qrCodeId": "campus://location/lib-001"
}
```

### Document 2: cafe-001 (Student Cafeteria)
```json
{
  "id": "cafe-001",
  "name": "Student Cafeteria",
  "description": "Main cafeteria with variety of food options and seating areas",
  "type": "cafeteria",
  "building": "Student Center",
  "floor": "G",
  "coordinates": {
	"latitude": 1.3525,
	"longitude": 103.8200
  },
  "images": [
	"cafeteria.jpg"
  ],
  "tags": [
	"food",
	"cafeteria",
	"seating",
	"halal",
	"vegetarian"
  ],
  "qrCodeId": "campus://location/cafe-001"
}
```

### Document 3: gym-001 (Sports Complex)
```json
{
  "id": "gym-001",
  "name": "Sports Complex",
  "description": "Multi-purpose sports facility with gym, courts, and pool",
  "type": "sports",
  "building": "Recreation Building",
  "floor": "B1-2",
  "coordinates": {
	"latitude": 1.3530,
	"longitude": 103.8195
  },
  "images": [
	"gym.jpg",
	"courts.jpg"
  ],
  "tags": [
	"sports",
	"gym",
	"fitness",
	"court",
	"swimming"
  ],
  "qrCodeId": "campus://location/gym-001"
}
```

### Document 4: lab-001 (Computer Science Lab)
```json
{
  "id": "lab-001",
  "name": "Computer Science Lab",
  "description": "State-of-the-art computer lab for programming and AI research",
  "type": "lab",
  "building": "Engineering Block",
  "floor": "2",
  "coordinates": {
	"latitude": 1.3518,
	"longitude": 103.8202
  },
  "images": [
	"lab.jpg"
  ],
  "tags": [
	"lab",
	"computers",
	"programming",
	"research",
	"ai"
  ],
  "qrCodeId": "campus://location/lab-001"
}
```

### Document 5: aud-001 (Main Auditorium)
```json
{
  "id": "aud-001",
  "name": "Main Auditorium",
  "description": "Large auditorium for lectures, seminars, and events",
  "type": "auditorium",
  "building": "Academic Block",
  "floor": "G",
  "coordinates": {
	"latitude": 1.3523,
	"longitude": 103.8205
  },
  "images": [
	"auditorium.jpg"
  ],
  "tags": [
	"auditorium",
	"lecture",
	"events",
	"seating"
  ],
  "qrCodeId": "campus://location/aud-001"
}
```

### Document 6: health-001 (Health Center)
```json
{
  "id": "health-001",
  "name": "Health Center",
  "description": "Student health services and medical clinic",
  "type": "health",
  "building": "Medical Building",
  "floor": "1",
  "coordinates": {
	"latitude": 1.3528,
	"longitude": 103.8190
  },
  "images": [
	"health.jpg"
  ],
  "tags": [
	"health",
	"medical",
	"clinic",
	"doctor"
  ],
  "qrCodeId": "campus://location/health-001"
}
```

---

## 📅 Collection: `events`

### Document 1: evt-001 (Campus Tour 2024)
```json
{
  "id": "evt-001",
  "title": "Campus Tour 2024",
  "description": "Welcome tour for new students exploring campus facilities",
  "startTime": "2024-11-23T10:00:00Z",
  "endTime": "2024-11-23T12:00:00Z",
  "locationId": "aud-001",
  "coordinates": {
	"latitude": 1.3523,
	"longitude": 103.8205
  },
  "imageUrl": "tour.jpg",
  "isPublished": true
}
```

### Document 2: evt-002 (Tech Talk)
```json
{
  "id": "evt-002",
  "title": "Tech Talk: AI in Campus Life",
  "description": "Special lecture on artificial intelligence applications in education",
  "startTime": "2024-11-25T14:00:00Z",
  "endTime": "2024-11-25T15:00:00Z",
  "locationId": "aud-001",
  "coordinates": {
	"latitude": 1.3523,
	"longitude": 103.8205
  },
  "imageUrl": "tech_talk.jpg",
  "isPublished": true
}
```

### Document 3: evt-003 (Sports Day 2024)
```json
{
  "id": "evt-003",
  "title": "Sports Day 2024",
  "description": "Annual inter-school sports competition and games",
  "startTime": "2024-11-27T08:00:00Z",
  "endTime": "2024-11-27T14:00:00Z",
  "locationId": "gym-001",
  "coordinates": {
	"latitude": 1.3530,
	"longitude": 103.8195
  },
  "imageUrl": "sports_day.jpg",
  "isPublished": true
}
```

### Document 4: evt-004 (Library Workshop)
```json
{
  "id": "evt-004",
  "title": "Library Workshop: Research Skills",
  "description": "Learn effective research methods and library resources",
  "startTime": "2024-11-22T15:00:00Z",
  "endTime": "2024-11-22T16:30:00Z",
  "locationId": "lib-001",
  "coordinates": {
	"latitude": 1.3521,
	"longitude": 103.8198
  },
  "imageUrl": "workshop.jpg",
  "isPublished": true
}
```

### Document 5: evt-005 (Networking Lunch)
```json
{
  "id": "evt-005",
  "title": "Networking Lunch with Alumni",
  "description": "Meet and greet with successful alumni professionals",
  "startTime": "2024-12-02T12:00:00Z",
  "endTime": "2024-12-02T14:00:00Z",
  "locationId": "cafe-001",
  "coordinates": {
	"latitude": 1.3525,
	"longitude": 103.8200
  },
  "imageUrl": "networking.jpg",
  "isPublished": true
}
```

---

## 🔔 Collection: `notifications`

### Document 1: notif-001
```json
{
  "id": "notif-001",
  "title": "Library Maintenance Notice",
  "body": "3rd floor library will be closed for maintenance on Nov 20th",
  "type": "announcement",
  "relatedLocationId": "lib-001",
  "relatedEventId": "",
  "createdAt": "2024-11-19T22:00:00Z"
}
```

### Document 2: notif-002
```json
{
  "id": "notif-002",
  "title": "Cafeteria Menu Update",
  "body": "Check new halal and vegetarian options available this week",
  "type": "announcement",
  "relatedLocationId": "cafe-001",
  "relatedEventId": "",
  "createdAt": "2024-11-19T18:00:00Z"
}
```

### Document 3: notif-003
```json
{
  "id": "notif-003",
  "title": "Campus Tour Registration Open",
  "body": "Don't miss the welcome tour! Register now for limited spots",
  "type": "event_reminder",
  "relatedLocationId": "aud-001",
  "relatedEventId": "evt-001",
  "createdAt": "2024-11-18T18:00:00Z"
}
```

### Document 4: notif-004
```json
{
  "id": "notif-004",
  "title": "Wi-Fi Upgrade Complete",
  "body": "Campus-wide Wi-Fi speed improved by 50%. Reconnect to enjoy!",
  "type": "update",
  "relatedLocationId": "",
  "relatedEventId": "",
  "createdAt": "2024-11-17T08:00:00Z"
}
```

### Document 5: notif-005
```json
{
  "id": "notif-005",
  "title": "Sports Complex Extended Hours",
  "body": "Gym now open until 10 PM on weekdays due to high demand",
  "type": "announcement",
  "relatedLocationId": "gym-001",
  "relatedEventId": "",
  "createdAt": "2024-11-16T10:00:00Z"
}
```

---

## 📝 How to Import via Firebase Console

### Step-by-Step Instructions:

1. **Go to Firebase Console**
   - Visit https://console.firebase.google.com/
   - Select your CampusGuide project
   - Go to **Firestore Database**

2. **Create Collections** (if not already created)
   - Click **+ Start Collection**
   - Name: `locations` → **Next**
   - For first document, you can upload manually

3. **Import Documents**
   - Click **+ Add Document** in each collection
   - For Document ID, use the `id` field from above (e.g., `lib-001`)
   - Copy each field from the JSON structure
   - Click **Save**

4. **Repeat** for all collections (locations → events → notifications)

---

## ✅ Verification

After importing, you should see:
- ✅ 6 documents in `locations` collection
- ✅ 5 documents in `events` collection
- ✅ 5 documents in `notifications` collection

---

## 🔄 Alternative: Use Firestore Bulk Import

If Firebase Console supports CSV/JSON import:
1. Export this data as JSON
2. Use Firebase CLI: `firebase firestore:delete` and import
3. Or use Admin SDK for bulk operations

---

## 📊 Test Data Summary

| Locations | Events | Notifications |
|-----------|--------|----------------|
| 6 items | 5 events | 5 announcements |
| Libraries, Cafes,Gyms, Labs, Auditoriums, Health Centers | Upcoming campus events | Maintenance, Updates, Reminders |

After adding this data, the CampusGuide app will:
- ✅ Show locations in search/map
- ✅ Display events in events section
- ✅ Show notifications/alerts
- ✅ Allow QR code scanning to retrieve location details
