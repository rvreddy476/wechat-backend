# Media Service - Database Operations Guide

## Collection: media_files

### Upload Media File

**Step 1: Upload to Storage** (S3, Azure Blob, etc.) - Application Code

**Step 2: Create Database Record**:
```javascript
const mediaFile = {
  _id: `media-${Date.now()}-${generateId()}`,
  uploadedBy: "550e8400-e29b-41d4-a716-446655440000",
  uploaderUsername: "john_doe",
  fileName: "vacation-photo.jpg",
  fileUrl: "https://cdn.example.com/media/abc123.jpg",
  thumbnailUrl: "https://cdn.example.com/media/abc123_thumb.jpg",
  mediaType: "Image",  // Image, Video, Audio, Document, Other
  mimeType: "image/jpeg",
  fileSize: 2048000,  // 2MB in bytes
  width: 1920,
  height: 1080,
  duration: null,  // For audio/video
  status: "Ready",  // Uploading, Processing, Ready, Failed
  isPublic: false,
  tags: ["vacation", "beach"],
  metadata: {
    camera: "iPhone 14 Pro",
    location: "Hawaii"
  },
  isDeleted: false,
  createdAt: new Date(),
  updatedAt: new Date()
};

db.media_files.insertOne(mediaFile);
```

---

### Get User's Media Files

```javascript
db.media_files.find({
  uploadedBy: "550e8400-e29b-41d4-a716-446655440000",
  isDeleted: false
}).sort({ createdAt: -1 });
```

---

### Filter by Media Type

```javascript
// Get only images
db.media_files.find({
  uploadedBy: "550e8400-e29b-41d4-a716-446655440000",
  mediaType: "Image",
  isDeleted: false
}).sort({ createdAt: -1 });

// Get videos
db.media_files.find({
  uploadedBy: "550e8400-e29b-41d4-a716-446655440000",
  mediaType: "Video",
  isDeleted: false
}).sort({ createdAt: -1 });
```

---

### Search by Tags

```javascript
db.media_files.find({
  tags: "vacation",
  uploadedBy: "550e8400-e29b-41d4-a716-446655440000",
  isDeleted: false
}).sort({ createdAt: -1 });
```

---

### Update Processing Status

```javascript
// After upload starts
db.media_files.updateOne(
  { _id: "media-12345" },
  { $set: { status: "Processing" } }
);

// After processing completes
db.media_files.updateOne(
  { _id: "media-12345" },
  {
    $set: {
      status: "Ready",
      thumbnailUrl: "https://cdn.example.com/media/thumb.jpg",
      updatedAt: new Date()
    }
  }
);

// If processing fails
db.media_files.updateOne(
  { _id: "media-12345" },
  { $set: { status: "Failed", updatedAt: new Date() } }
);
```

---

### Delete Media File

**Soft Delete**:
```javascript
db.media_files.updateOne(
  { _id: "media-12345" },
  {
    $set: {
      isDeleted: true,
      updatedAt: new Date()
    }
  }
);
```

**Then Delete from Storage** (Application Code):
```javascript
await s3.deleteObject({ Bucket: 'mybucket', Key: 'abc123.jpg' });
await s3.deleteObject({ Bucket: 'mybucket', Key: 'abc123_thumb.jpg' });
```

---
