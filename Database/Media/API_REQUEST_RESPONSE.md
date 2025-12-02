# Media API - Complete Request & Response Documentation

> **Purpose**: Complete API documentation with request/response examples for file uploads and media management
> **Last Updated**: 2025-12-02
> **Base URL**: `https://api.yourapp.com/api/v1`

---

## Table of Contents
1. [Authentication](#authentication)
2. [File Upload](#file-upload)
3. [Media Management](#media-management)
4. [Media Retrieval](#media-retrieval)
5. [Error Responses](#error-responses)

---

## Authentication

All API requests require JWT Bearer authentication.

**Headers Required**:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## File Upload

### 1. Upload Single File

**Endpoint**: `POST /api/v1/media/upload`

**Description**: Upload a single file (image, video, document, or audio)

**Request**:
```http
POST /api/v1/media/upload HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="photo.jpg"
Content-Type: image/jpeg

[binary file data]
------WebKitFormBoundary
Content-Disposition: form-data; name="category"

profile
------WebKitFormBoundary
Content-Disposition: form-data; name="tags"

vacation,beach,summer
------WebKitFormBoundary--
```

**Form Fields**:
- `file`: File to upload (required)
- `category`: File category - `profile`, `post`, `chat`, `document` (optional)
- `tags`: Comma-separated tags (optional)
- `description`: File description (optional)

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "_id": "media-67890abcdef1234567890001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "fileName": "photo.jpg",
    "originalFileName": "photo.jpg",
    "fileType": "Image",
    "mimeType": "image/jpeg",
    "fileSize": 2048576,
    "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/photo.jpg",
    "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/photo.jpg",
    "dimensions": {
      "width": 1920,
      "height": 1080
    },
    "category": "profile",
    "tags": ["vacation", "beach", "summer"],
    "processingStatus": "Completed",
    "isDeleted": false,
    "uploadedAt": "2025-12-02T12:00:00Z"
  }
}
```

**Progress Response** (202 Accepted - For large files):
```json
{
  "success": true,
  "message": "File upload in progress",
  "data": {
    "_id": "media-67890abcdef1234567890001",
    "fileName": "video.mp4",
    "fileType": "Video",
    "processingStatus": "Processing",
    "uploadProgress": 75,
    "uploadedAt": "2025-12-02T12:00:00Z"
  }
}
```

---

### 2. Upload Multiple Files

**Endpoint**: `POST /api/v1/media/upload/batch`

**Description**: Upload multiple files at once

**Request**:
```http
POST /api/v1/media/upload/batch HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="files"; filename="photo1.jpg"
Content-Type: image/jpeg

[binary file data]
------WebKitFormBoundary
Content-Disposition: form-data; name="files"; filename="photo2.jpg"
Content-Type: image/jpeg

[binary file data]
------WebKitFormBoundary
Content-Disposition: form-data; name="category"

post
------WebKitFormBoundary--
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Files uploaded successfully",
  "data": {
    "uploadedFiles": [
      {
        "_id": "media-67890abcdef1234567890001",
        "fileName": "photo1.jpg",
        "fileType": "Image",
        "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/photo1.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/photo1.jpg",
        "processingStatus": "Completed",
        "uploadedAt": "2025-12-02T12:05:00Z"
      },
      {
        "_id": "media-67890abcdef1234567890002",
        "fileName": "photo2.jpg",
        "fileType": "Image",
        "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/photo2.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/photo2.jpg",
        "processingStatus": "Completed",
        "uploadedAt": "2025-12-02T12:05:00Z"
      }
    ],
    "failedFiles": [],
    "totalUploaded": 2,
    "totalFailed": 0
  }
}
```

---

### 3. Check Upload Status

**Endpoint**: `GET /api/v1/media/{mediaId}/status`

**Description**: Check processing status of uploaded file

**Request**:
```http
GET /api/v1/media/media-67890abcdef1234567890001/status HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "mediaId": "media-67890abcdef1234567890001",
    "processingStatus": "Completed",
    "uploadProgress": 100,
    "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/video.mp4",
    "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/video.jpg",
    "completedAt": "2025-12-02T12:10:00Z"
  }
}
```

**Processing Status Values**:
- `Pending`: Upload initiated, waiting to process
- `Processing`: Currently processing
- `Completed`: Processing completed successfully
- `Failed`: Processing failed

---

## Media Management

### 4. Get Media by ID

**Endpoint**: `GET /api/v1/media/{mediaId}`

**Description**: Get detailed information about a media file

**Request**:
```http
GET /api/v1/media/media-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "_id": "media-67890abcdef1234567890001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "fileName": "photo.jpg",
    "originalFileName": "vacation_photo.jpg",
    "fileType": "Image",
    "mimeType": "image/jpeg",
    "fileSize": 2048576,
    "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/photo.jpg",
    "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/photo.jpg",
    "dimensions": {
      "width": 1920,
      "height": 1080
    },
    "duration": null,
    "category": "profile",
    "tags": ["vacation", "beach", "summer"],
    "description": "Summer vacation at the beach",
    "metadata": {
      "camera": "iPhone 15 Pro",
      "location": "San Francisco, CA",
      "capturedAt": "2025-11-20T15:30:00Z"
    },
    "processingStatus": "Completed",
    "isDeleted": false,
    "uploadedAt": "2025-12-02T12:00:00Z"
  }
}
```

---

### 5. Update Media Metadata

**Endpoint**: `PUT /api/v1/media/{mediaId}`

**Description**: Update media file metadata (tags, description, etc.)

**Request**:
```http
PUT /api/v1/media/media-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "tags": ["vacation", "beach", "summer", "2025"],
  "description": "Amazing summer vacation at the beach in California",
  "category": "personal"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Media metadata updated successfully",
  "data": {
    "_id": "media-67890abcdef1234567890001",
    "tags": ["vacation", "beach", "summer", "2025"],
    "description": "Amazing summer vacation at the beach in California",
    "category": "personal",
    "updatedAt": "2025-12-02T12:30:00Z"
  }
}
```

---

### 6. Delete Media

**Endpoint**: `DELETE /api/v1/media/{mediaId}`

**Description**: Delete a media file (soft delete)

**Request**:
```http
DELETE /api/v1/media/media-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Media deleted successfully",
  "data": {
    "mediaId": "media-67890abcdef1234567890001",
    "deletedAt": "2025-12-02T13:00:00Z"
  }
}
```

---

## Media Retrieval

### 7. Get User Media

**Endpoint**: `GET /api/v1/users/{userId}/media`

**Description**: Get all media files uploaded by a user

**Request**:
```http
GET /api/v1/users/550e8400-e29b-41d4-a716-446655440000/media?page=1&limit=20&fileType=Image&category=profile HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 20, max: 100)
- `fileType`: Filter by type - `Image`, `Video`, `Document`, `Audio` (optional)
- `category`: Filter by category (optional)
- `tags`: Filter by tags (comma-separated) (optional)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "media": [
      {
        "_id": "media-67890abcdef1234567890001",
        "fileName": "photo.jpg",
        "fileType": "Image",
        "mimeType": "image/jpeg",
        "fileSize": 2048576,
        "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/photo.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/photo.jpg",
        "category": "profile",
        "tags": ["vacation", "beach"],
        "uploadedAt": "2025-12-02T12:00:00Z"
      },
      {
        "_id": "media-67890abcdef1234567890002",
        "fileName": "video.mp4",
        "fileType": "Video",
        "mimeType": "video/mp4",
        "fileSize": 15728640,
        "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/video.mp4",
        "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/video.jpg",
        "duration": 45,
        "category": "post",
        "tags": ["coding", "tutorial"],
        "uploadedAt": "2025-12-01T15:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 5,
      "totalCount": 89,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 8. Search Media

**Endpoint**: `GET /api/v1/media/search`

**Description**: Search media files by tags, description, or filename

**Request**:
```http
GET /api/v1/media/search?q=vacation&fileType=Image&page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "_id": "media-67890abcdef1234567890001",
        "fileName": "vacation_photo.jpg",
        "fileType": "Image",
        "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/vacation_photo.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/vacation_photo.jpg",
        "tags": ["vacation", "beach", "summer"],
        "description": "Summer vacation at the beach",
        "uploadedAt": "2025-12-02T12:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalCount": 1,
      "hasNextPage": false
    }
  }
}
```

---

### 9. Get Recent Media

**Endpoint**: `GET /api/v1/media/recent`

**Description**: Get recently uploaded media files

**Request**:
```http
GET /api/v1/media/recent?limit=10&fileType=Image HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "media": [
      {
        "_id": "media-67890abcdef1234567890005",
        "fileName": "latest_photo.jpg",
        "fileType": "Image",
        "url": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/latest_photo.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/media/550e8400-e29b-41d4-a716-446655440000/thumbs/latest_photo.jpg",
        "uploadedAt": "2025-12-02T14:00:00Z"
      }
    ],
    "count": 10
  }
}
```

---

## Error Responses

### Standard Error Format

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "statusCode": 400
  }
}
```

### Common Error Codes

**File Too Large (413)**:
```json
{
  "success": false,
  "error": {
    "code": "FILE_TOO_LARGE",
    "message": "File size exceeds maximum allowed size",
    "statusCode": 413,
    "details": {
      "maxSize": "10MB",
      "actualSize": "15MB"
    }
  }
}
```

**Invalid File Type (415)**:
```json
{
  "success": false,
  "error": {
    "code": "INVALID_FILE_TYPE",
    "message": "File type not supported",
    "statusCode": 415,
    "details": {
      "allowedTypes": ["image/jpeg", "image/png", "image/gif", "image/webp"],
      "receivedType": "image/bmp"
    }
  }
}
```

**Processing Failed (500)**:
```json
{
  "success": false,
  "error": {
    "code": "PROCESSING_FAILED",
    "message": "File processing failed",
    "statusCode": 500,
    "details": {
      "reason": "Failed to generate thumbnail"
    }
  }
}
```

---

## File Type Limits

### Images
- **Formats**: JPEG, PNG, GIF, WebP
- **Max Size**: 10 MB
- **Max Dimensions**: 4096 x 4096 pixels

### Videos
- **Formats**: MP4, MOV, AVI, WebM
- **Max Size**: 100 MB
- **Max Duration**: 10 minutes
- **Max Resolution**: 1920 x 1080 (Full HD)

### Documents
- **Formats**: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, TXT
- **Max Size**: 20 MB

### Audio
- **Formats**: MP3, WAV, M4A, OGG
- **Max Size**: 20 MB
- **Max Duration**: 10 minutes

---

**End of Documentation**
