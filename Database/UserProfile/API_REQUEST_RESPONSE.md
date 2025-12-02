# UserProfile API - Complete Request & Response Documentation

> **Purpose**: Complete API documentation with request/response examples for UI development
> **Last Updated**: 2025-12-02
> **Base URL**: `https://api.yourapp.com/api/v1`

---

## Table of Contents
1. [Authentication](#authentication)
2. [Profile Management](#profile-management)
3. [Education](#education)
4. [Professional Experience](#professional-experience)
5. [Skills](#skills)
6. [Certifications](#certifications)
7. [Social Connections](#social-connections)
8. [Friend Requests](#friend-requests)
9. [Follow System](#follow-system)
10. [Block System](#block-system)
11. [Privacy Settings](#privacy-settings)
12. [Notification Settings](#notification-settings)
13. [Search & Discovery](#search--discovery)
14. [Activity & Analytics](#activity--analytics)
15. [User Content](#user-content)
16. [Account Settings](#account-settings)
17. [Error Responses](#error-responses)

---

## Authentication

All API requests require authentication using JWT Bearer token.

**Headers Required**:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

---

## Profile Management

### 1. Get User Profile

**Endpoint**: `GET /api/v1/profiles/{userId}`

**Description**: Retrieve a user's profile information (respects privacy settings)

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "_id": "67890abcdef1234567890abc",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "displayName": "John Doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "dateOfBirth": "1990-05-15T00:00:00Z",
    "gender": "Male",
    "bio": "Software Engineer | Tech Enthusiast | Coffee Lover ☕",
    "tagline": "Building amazing things one line of code at a time",
    "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe_avatar.jpg",
    "coverImageUrl": "https://cdn.yourapp.com/covers/john_doe_cover.jpg",
    "location": {
      "city": "San Francisco",
      "state": "California",
      "country": "United States",
      "coordinates": {
        "latitude": 37.7749,
        "longitude": -122.4194
      }
    },
    "website": "https://johndoe.dev",
    "socialLinks": {
      "linkedin": "https://linkedin.com/in/johndoe",
      "twitter": "https://twitter.com/johndoe",
      "github": "https://github.com/johndoe",
      "instagram": "https://instagram.com/johndoe",
      "facebook": "https://facebook.com/johndoe"
    },
    "interests": ["Programming", "Photography", "Travel", "Music"],
    "statistics": {
      "friendsCount": 342,
      "followersCount": 1250,
      "followingCount": 487,
      "postsCount": 156,
      "photosCount": 89,
      "videosCount": 12,
      "profileViewsCount": 5432
    },
    "isVerified": true,
    "isOnline": true,
    "lastSeenAt": "2025-12-02T10:30:00Z",
    "isPremium": true,
    "isActive": true,
    "createdAt": "2023-01-15T08:00:00Z",
    "updatedAt": "2025-12-02T10:30:00Z"
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "error": {
    "code": "NOT_FOUND",
    "message": "User profile not found",
    "statusCode": 404
  }
}
```

---

### 2. Get Current User Profile

**Endpoint**: `GET /api/v1/profiles/me`

**Description**: Retrieve the authenticated user's own profile (includes private fields)

**Request**:
```http
GET /api/v1/profiles/me HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "_id": "67890abcdef1234567890abc",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "displayName": "John Doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "dateOfBirth": "1990-05-15T00:00:00Z",
    "gender": "Male",
    "bio": "Software Engineer | Tech Enthusiast | Coffee Lover ☕",
    "tagline": "Building amazing things one line of code at a time",
    "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe_avatar.jpg",
    "coverImageUrl": "https://cdn.yourapp.com/covers/john_doe_cover.jpg",
    "location": {
      "city": "San Francisco",
      "state": "California",
      "country": "United States",
      "coordinates": {
        "latitude": 37.7749,
        "longitude": -122.4194
      }
    },
    "website": "https://johndoe.dev",
    "socialLinks": {
      "linkedin": "https://linkedin.com/in/johndoe",
      "twitter": "https://twitter.com/johndoe",
      "github": "https://github.com/johndoe",
      "instagram": "https://instagram.com/johndoe",
      "facebook": "https://facebook.com/johndoe"
    },
    "interests": ["Programming", "Photography", "Travel", "Music"],
    "friends": [
      "660e8400-e29b-41d4-a716-446655440001",
      "660e8400-e29b-41d4-a716-446655440002"
    ],
    "followers": [
      "770e8400-e29b-41d4-a716-446655440001"
    ],
    "following": [
      "880e8400-e29b-41d4-a716-446655440001"
    ],
    "blockedUsers": [],
    "privacySettings": {
      "profileVisibility": "Friends",
      "showEmail": false,
      "showPhoneNumber": false,
      "showDateOfBirth": false,
      "showLocation": true,
      "showOnlineStatus": true,
      "showLastSeen": false,
      "allowFriendRequests": true,
      "allowMessages": "Friends",
      "showEducation": true,
      "showExperience": true,
      "showConnections": true,
      "allowTagging": "Friends",
      "allowMentions": "Everyone"
    },
    "notificationSettings": {
      "emailNotifications": true,
      "pushNotifications": true,
      "messageNotifications": true,
      "friendRequestNotifications": true,
      "postNotifications": true,
      "commentNotifications": true,
      "likeNotifications": false,
      "mentionNotifications": true,
      "followerNotifications": true
    },
    "accountSettings": {
      "language": "en",
      "timezone": "America/Los_Angeles",
      "dateFormat": "MM/DD/YYYY",
      "theme": "Dark"
    },
    "statistics": {
      "friendsCount": 342,
      "followersCount": 1250,
      "followingCount": 487,
      "postsCount": 156,
      "photosCount": 89,
      "videosCount": 12,
      "profileViewsCount": 5432
    },
    "isVerified": true,
    "isOnline": true,
    "lastSeenAt": "2025-12-02T10:30:00Z",
    "isPremium": true,
    "isActive": true,
    "createdAt": "2023-01-15T08:00:00Z",
    "updatedAt": "2025-12-02T10:30:00Z"
  }
}
```

---

### 3. Update User Profile

**Endpoint**: `PUT /api/v1/profiles/me`

**Description**: Update the authenticated user's profile information

**Request**:
```http
PUT /api/v1/profiles/me HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "displayName": "John M. Doe",
  "bio": "Senior Software Engineer | Tech Enthusiast | Coffee Lover ☕ | Open Source Contributor",
  "tagline": "Building amazing things one line of code at a time",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "location": {
    "city": "San Francisco",
    "state": "California",
    "country": "United States",
    "coordinates": {
      "latitude": 37.7749,
      "longitude": -122.4194
    }
  },
  "website": "https://johndoe.dev",
  "socialLinks": {
    "linkedin": "https://linkedin.com/in/johndoe",
    "twitter": "https://twitter.com/johndoe",
    "github": "https://github.com/johndoe",
    "instagram": "https://instagram.com/johndoe"
  },
  "interests": ["Programming", "Photography", "Travel", "Music", "AI/ML"]
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "_id": "67890abcdef1234567890abc",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "displayName": "John M. Doe",
    "firstName": "John",
    "lastName": "Doe",
    "bio": "Senior Software Engineer | Tech Enthusiast | Coffee Lover ☕ | Open Source Contributor",
    "tagline": "Building amazing things one line of code at a time",
    "location": {
      "city": "San Francisco",
      "state": "California",
      "country": "United States",
      "coordinates": {
        "latitude": 37.7749,
        "longitude": -122.4194
      }
    },
    "interests": ["Programming", "Photography", "Travel", "Music", "AI/ML"],
    "updatedAt": "2025-12-02T11:00:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "statusCode": 400,
    "details": [
      {
        "field": "bio",
        "message": "Bio must not exceed 500 characters"
      },
      {
        "field": "website",
        "message": "Website must be a valid URL"
      }
    ]
  }
}
```

---

### 4. Upload Avatar

**Endpoint**: `POST /api/v1/profiles/me/avatar`

**Description**: Upload or update profile avatar image

**Request**:
```http
POST /api/v1/profiles/me/avatar HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="avatar"; filename="profile.jpg"
Content-Type: image/jpeg

[binary image data]
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Avatar uploaded successfully",
  "data": {
    "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe_avatar_1701518400.jpg",
    "thumbnailUrl": "https://cdn.yourapp.com/avatars/thumbs/john_doe_avatar_1701518400.jpg",
    "uploadedAt": "2025-12-02T11:15:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "FILE_TOO_LARGE",
    "message": "File size exceeds maximum allowed size of 5MB",
    "statusCode": 400
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_FILE_TYPE",
    "message": "Only JPEG, PNG, and WebP images are allowed",
    "statusCode": 400
  }
}
```

---

### 5. Upload Cover Image

**Endpoint**: `POST /api/v1/profiles/me/cover`

**Description**: Upload or update profile cover image

**Request**:
```http
POST /api/v1/profiles/me/cover HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="cover"; filename="cover.jpg"
Content-Type: image/jpeg

[binary image data]
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Cover image uploaded successfully",
  "data": {
    "coverImageUrl": "https://cdn.yourapp.com/covers/john_doe_cover_1701518500.jpg",
    "uploadedAt": "2025-12-02T11:20:00Z"
  }
}
```

---

### 6. Delete Avatar

**Endpoint**: `DELETE /api/v1/profiles/me/avatar`

**Description**: Remove profile avatar (revert to default)

**Request**:
```http
DELETE /api/v1/profiles/me/avatar HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Avatar deleted successfully",
  "data": {
    "avatarUrl": "https://cdn.yourapp.com/avatars/default_avatar.png"
  }
}
```

---

### 7. Delete Cover Image

**Endpoint**: `DELETE /api/v1/profiles/me/cover`

**Description**: Remove profile cover image

**Request**:
```http
DELETE /api/v1/profiles/me/cover HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Cover image deleted successfully",
  "data": {
    "coverImageUrl": null
  }
}
```

---

## Education

### 8. Add Education

**Endpoint**: `POST /api/v1/profiles/me/education`

**Description**: Add education entry to user profile

**Request**:
```http
POST /api/v1/profiles/me/education HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "school": "Stanford University",
  "degree": "Bachelor of Science",
  "fieldOfStudy": "Computer Science",
  "startDate": "2008-09-01",
  "endDate": "2012-06-15",
  "grade": "3.85 GPA",
  "activities": "Computer Science Society, ACM Member, Robotics Club",
  "description": "Focused on Artificial Intelligence and Machine Learning. Completed senior thesis on Neural Networks.",
  "current": false
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Education added successfully",
  "data": {
    "id": "edu-67890abcdef1234567890001",
    "school": "Stanford University",
    "degree": "Bachelor of Science",
    "fieldOfStudy": "Computer Science",
    "startDate": "2008-09-01T00:00:00Z",
    "endDate": "2012-06-15T00:00:00Z",
    "grade": "3.85 GPA",
    "activities": "Computer Science Society, ACM Member, Robotics Club",
    "description": "Focused on Artificial Intelligence and Machine Learning. Completed senior thesis on Neural Networks.",
    "current": false,
    "createdAt": "2025-12-02T11:30:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "statusCode": 400,
    "details": [
      {
        "field": "school",
        "message": "School name is required"
      },
      {
        "field": "endDate",
        "message": "End date must be after start date"
      }
    ]
  }
}
```

---

### 9. Get Education

**Endpoint**: `GET /api/v1/profiles/{userId}/education`

**Description**: Get all education entries for a user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/education HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "education": [
      {
        "id": "edu-67890abcdef1234567890001",
        "school": "Stanford University",
        "degree": "Bachelor of Science",
        "fieldOfStudy": "Computer Science",
        "startDate": "2008-09-01T00:00:00Z",
        "endDate": "2012-06-15T00:00:00Z",
        "grade": "3.85 GPA",
        "activities": "Computer Science Society, ACM Member, Robotics Club",
        "description": "Focused on Artificial Intelligence and Machine Learning. Completed senior thesis on Neural Networks.",
        "current": false
      },
      {
        "id": "edu-67890abcdef1234567890002",
        "school": "MIT",
        "degree": "Master of Science",
        "fieldOfStudy": "Artificial Intelligence",
        "startDate": "2012-09-01T00:00:00Z",
        "endDate": "2014-06-15T00:00:00Z",
        "grade": "4.0 GPA",
        "activities": "AI Research Lab, Teaching Assistant",
        "description": "Research in Deep Learning and Computer Vision.",
        "current": false
      }
    ],
    "count": 2
  }
}
```

---

### 10. Update Education

**Endpoint**: `PUT /api/v1/profiles/me/education/{educationId}`

**Description**: Update an existing education entry

**Request**:
```http
PUT /api/v1/profiles/me/education/edu-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "school": "Stanford University",
  "degree": "Bachelor of Science (Honors)",
  "fieldOfStudy": "Computer Science",
  "startDate": "2008-09-01",
  "endDate": "2012-06-15",
  "grade": "3.90 GPA",
  "activities": "Computer Science Society President, ACM Member, Robotics Club",
  "description": "Focused on Artificial Intelligence and Machine Learning. Completed senior thesis on Neural Networks. Dean's List all semesters.",
  "current": false
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Education updated successfully",
  "data": {
    "id": "edu-67890abcdef1234567890001",
    "school": "Stanford University",
    "degree": "Bachelor of Science (Honors)",
    "fieldOfStudy": "Computer Science",
    "grade": "3.90 GPA",
    "updatedAt": "2025-12-02T11:45:00Z"
  }
}
```

---

### 11. Delete Education

**Endpoint**: `DELETE /api/v1/profiles/me/education/{educationId}`

**Description**: Remove an education entry

**Request**:
```http
DELETE /api/v1/profiles/me/education/edu-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Education deleted successfully"
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "error": {
    "code": "NOT_FOUND",
    "message": "Education entry not found",
    "statusCode": 404
  }
}
```

---

## Professional Experience

### 12. Add Experience

**Endpoint**: `POST /api/v1/profiles/me/experience`

**Description**: Add professional experience entry

**Request**:
```http
POST /api/v1/profiles/me/experience HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "company": "Google",
  "position": "Senior Software Engineer",
  "employmentType": "Full-time",
  "location": "Mountain View, CA",
  "startDate": "2020-03-01",
  "endDate": null,
  "current": true,
  "description": "Leading the development of cloud infrastructure services. Managing a team of 5 engineers. Implementing microservices architecture using Kubernetes and Docker.",
  "skills": ["Python", "Go", "Kubernetes", "Docker", "Cloud Architecture", "Team Leadership"]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Experience added successfully",
  "data": {
    "id": "exp-67890abcdef1234567890001",
    "company": "Google",
    "position": "Senior Software Engineer",
    "employmentType": "Full-time",
    "location": "Mountain View, CA",
    "startDate": "2020-03-01T00:00:00Z",
    "endDate": null,
    "current": true,
    "description": "Leading the development of cloud infrastructure services. Managing a team of 5 engineers. Implementing microservices architecture using Kubernetes and Docker.",
    "skills": ["Python", "Go", "Kubernetes", "Docker", "Cloud Architecture", "Team Leadership"],
    "createdAt": "2025-12-02T12:00:00Z"
  }
}
```

---

### 13. Get Experience

**Endpoint**: `GET /api/v1/profiles/{userId}/experience`

**Description**: Get all professional experience entries for a user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/experience HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "experience": [
      {
        "id": "exp-67890abcdef1234567890001",
        "company": "Google",
        "position": "Senior Software Engineer",
        "employmentType": "Full-time",
        "location": "Mountain View, CA",
        "startDate": "2020-03-01T00:00:00Z",
        "endDate": null,
        "current": true,
        "description": "Leading the development of cloud infrastructure services. Managing a team of 5 engineers.",
        "skills": ["Python", "Go", "Kubernetes", "Docker", "Cloud Architecture"]
      },
      {
        "id": "exp-67890abcdef1234567890002",
        "company": "Facebook",
        "position": "Software Engineer",
        "employmentType": "Full-time",
        "location": "Menlo Park, CA",
        "startDate": "2016-06-01T00:00:00Z",
        "endDate": "2020-02-28T00:00:00Z",
        "current": false,
        "description": "Developed features for News Feed and Stories. Worked on React and GraphQL infrastructure.",
        "skills": ["React", "JavaScript", "GraphQL", "PHP", "MySQL"]
      },
      {
        "id": "exp-67890abcdef1234567890003",
        "company": "Microsoft",
        "position": "Software Development Intern",
        "employmentType": "Internship",
        "location": "Redmond, WA",
        "startDate": "2014-06-01T00:00:00Z",
        "endDate": "2014-08-31T00:00:00Z",
        "current": false,
        "description": "Contributed to Azure DevOps services. Implemented CI/CD pipeline features.",
        "skills": ["C#", ".NET", "Azure", "CI/CD"]
      }
    ],
    "count": 3
  }
}
```

---

### 14. Update Experience

**Endpoint**: `PUT /api/v1/profiles/me/experience/{experienceId}`

**Description**: Update an existing experience entry

**Request**:
```http
PUT /api/v1/profiles/me/experience/exp-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "company": "Google",
  "position": "Staff Software Engineer",
  "employmentType": "Full-time",
  "location": "Mountain View, CA",
  "startDate": "2020-03-01",
  "endDate": null,
  "current": true,
  "description": "Leading the development of cloud infrastructure services. Managing a team of 8 engineers. Implementing microservices architecture using Kubernetes and Docker. Recently promoted to Staff level.",
  "skills": ["Python", "Go", "Kubernetes", "Docker", "Cloud Architecture", "Team Leadership", "System Design"]
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Experience updated successfully",
  "data": {
    "id": "exp-67890abcdef1234567890001",
    "position": "Staff Software Engineer",
    "description": "Leading the development of cloud infrastructure services. Managing a team of 8 engineers. Implementing microservices architecture using Kubernetes and Docker. Recently promoted to Staff level.",
    "updatedAt": "2025-12-02T12:15:00Z"
  }
}
```

---

### 15. Delete Experience

**Endpoint**: `DELETE /api/v1/profiles/me/experience/{experienceId}`

**Description**: Remove an experience entry

**Request**:
```http
DELETE /api/v1/profiles/me/experience/exp-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Experience deleted successfully"
}
```

---

## Skills

### 16. Add Skills

**Endpoint**: `POST /api/v1/profiles/me/skills`

**Description**: Add one or more skills to user profile

**Request**:
```http
POST /api/v1/profiles/me/skills HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "skills": [
    {
      "name": "Python",
      "level": "Expert",
      "yearsOfExperience": 8
    },
    {
      "name": "JavaScript",
      "level": "Advanced",
      "yearsOfExperience": 6
    },
    {
      "name": "Docker",
      "level": "Intermediate",
      "yearsOfExperience": 3
    }
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Skills added successfully",
  "data": {
    "addedSkills": [
      {
        "name": "Python",
        "level": "Expert",
        "yearsOfExperience": 8,
        "endorsements": 0
      },
      {
        "name": "JavaScript",
        "level": "Advanced",
        "yearsOfExperience": 6,
        "endorsements": 0
      },
      {
        "name": "Docker",
        "level": "Intermediate",
        "yearsOfExperience": 3,
        "endorsements": 0
      }
    ],
    "count": 3
  }
}
```

---

### 17. Get Skills

**Endpoint**: `GET /api/v1/profiles/{userId}/skills`

**Description**: Get all skills for a user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/skills HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "skills": [
      {
        "name": "Python",
        "level": "Expert",
        "yearsOfExperience": 8,
        "endorsements": 47
      },
      {
        "name": "JavaScript",
        "level": "Advanced",
        "yearsOfExperience": 6,
        "endorsements": 35
      },
      {
        "name": "Kubernetes",
        "level": "Advanced",
        "yearsOfExperience": 4,
        "endorsements": 28
      },
      {
        "name": "Docker",
        "level": "Intermediate",
        "yearsOfExperience": 3,
        "endorsements": 22
      },
      {
        "name": "React",
        "level": "Advanced",
        "yearsOfExperience": 5,
        "endorsements": 31
      }
    ],
    "count": 5
  }
}
```

---

### 18. Update Skill

**Endpoint**: `PUT /api/v1/profiles/me/skills/{skillName}`

**Description**: Update an existing skill

**Request**:
```http
PUT /api/v1/profiles/me/skills/Python HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "level": "Expert",
  "yearsOfExperience": 9
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Skill updated successfully",
  "data": {
    "name": "Python",
    "level": "Expert",
    "yearsOfExperience": 9,
    "endorsements": 47
  }
}
```

---

### 19. Delete Skill

**Endpoint**: `DELETE /api/v1/profiles/me/skills/{skillName}`

**Description**: Remove a skill from profile

**Request**:
```http
DELETE /api/v1/profiles/me/skills/Docker HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Skill deleted successfully"
}
```

---

### 20. Endorse Skill

**Endpoint**: `POST /api/v1/profiles/{userId}/skills/{skillName}/endorse`

**Description**: Endorse a user's skill (can only endorse once per skill)

**Request**:
```http
POST /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/skills/Python/endorse HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Skill endorsed successfully",
  "data": {
    "name": "Python",
    "endorsements": 48
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_ENDORSED",
    "message": "You have already endorsed this skill",
    "statusCode": 400
  }
}
```

---

## Certifications

### 21. Add Certification

**Endpoint**: `POST /api/v1/profiles/me/certifications`

**Description**: Add a certification to user profile

**Request**:
```http
POST /api/v1/profiles/me/certifications HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "AWS Certified Solutions Architect - Professional",
  "issuingOrganization": "Amazon Web Services (AWS)",
  "issueDate": "2023-06-15",
  "expirationDate": "2026-06-15",
  "credentialId": "AWS-PSA-123456",
  "credentialUrl": "https://aws.amazon.com/verification/AWS-PSA-123456"
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Certification added successfully",
  "data": {
    "id": "cert-67890abcdef1234567890001",
    "name": "AWS Certified Solutions Architect - Professional",
    "issuingOrganization": "Amazon Web Services (AWS)",
    "issueDate": "2023-06-15T00:00:00Z",
    "expirationDate": "2026-06-15T00:00:00Z",
    "credentialId": "AWS-PSA-123456",
    "credentialUrl": "https://aws.amazon.com/verification/AWS-PSA-123456",
    "createdAt": "2025-12-02T12:30:00Z"
  }
}
```

---

### 22. Get Certifications

**Endpoint**: `GET /api/v1/profiles/{userId}/certifications`

**Description**: Get all certifications for a user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/certifications HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "certifications": [
      {
        "id": "cert-67890abcdef1234567890001",
        "name": "AWS Certified Solutions Architect - Professional",
        "issuingOrganization": "Amazon Web Services (AWS)",
        "issueDate": "2023-06-15T00:00:00Z",
        "expirationDate": "2026-06-15T00:00:00Z",
        "credentialId": "AWS-PSA-123456",
        "credentialUrl": "https://aws.amazon.com/verification/AWS-PSA-123456"
      },
      {
        "id": "cert-67890abcdef1234567890002",
        "name": "Google Cloud Professional Cloud Architect",
        "issuingOrganization": "Google Cloud",
        "issueDate": "2022-09-20T00:00:00Z",
        "expirationDate": "2024-09-20T00:00:00Z",
        "credentialId": "GCP-PCA-789012",
        "credentialUrl": "https://cloud.google.com/verification/GCP-PCA-789012"
      },
      {
        "id": "cert-67890abcdef1234567890003",
        "name": "Certified Kubernetes Administrator (CKA)",
        "issuingOrganization": "Cloud Native Computing Foundation",
        "issueDate": "2021-11-10T00:00:00Z",
        "expirationDate": "2024-11-10T00:00:00Z",
        "credentialId": "CKA-345678",
        "credentialUrl": "https://www.cncf.io/certification/cka/verify/CKA-345678"
      }
    ],
    "count": 3
  }
}
```

---

### 23. Update Certification

**Endpoint**: `PUT /api/v1/profiles/me/certifications/{certificationId}`

**Description**: Update an existing certification

**Request**:
```http
PUT /api/v1/profiles/me/certifications/cert-67890abcdef1234567890002 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Google Cloud Professional Cloud Architect",
  "issuingOrganization": "Google Cloud",
  "issueDate": "2024-10-01",
  "expirationDate": "2026-10-01",
  "credentialId": "GCP-PCA-999888",
  "credentialUrl": "https://cloud.google.com/verification/GCP-PCA-999888"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Certification updated successfully",
  "data": {
    "id": "cert-67890abcdef1234567890002",
    "issueDate": "2024-10-01T00:00:00Z",
    "expirationDate": "2026-10-01T00:00:00Z",
    "credentialId": "GCP-PCA-999888",
    "updatedAt": "2025-12-02T12:45:00Z"
  }
}
```

---

### 24. Delete Certification

**Endpoint**: `DELETE /api/v1/profiles/me/certifications/{certificationId}`

**Description**: Remove a certification from profile

**Request**:
```http
DELETE /api/v1/profiles/me/certifications/cert-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Certification deleted successfully"
}
```

---

## Social Connections

### 25. Get Friends

**Endpoint**: `GET /api/v1/profiles/{userId}/friends`

**Description**: Get list of user's friends with pagination

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/friends?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "friends": [
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith",
        "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith_avatar.jpg",
        "isOnline": true,
        "isVerified": true,
        "mutualFriendsCount": 45
      },
      {
        "userId": "660e8400-e29b-41d4-a716-446655440002",
        "username": "bob_wilson",
        "displayName": "Bob Wilson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/bob_wilson_avatar.jpg",
        "isOnline": false,
        "lastSeenAt": "2025-12-01T18:30:00Z",
        "isVerified": false,
        "mutualFriendsCount": 12
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 18,
      "totalCount": 342,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 26. Get Followers

**Endpoint**: `GET /api/v1/profiles/{userId}/followers`

**Description**: Get list of users following this user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/followers?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "followers": [
      {
        "userId": "770e8400-e29b-41d4-a716-446655440001",
        "username": "alice_johnson",
        "displayName": "Alice Johnson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/alice_johnson_avatar.jpg",
        "isOnline": true,
        "isVerified": true,
        "followedAt": "2025-11-15T10:00:00Z",
        "isFriend": false,
        "isFollowingBack": true
      },
      {
        "userId": "770e8400-e29b-41d4-a716-446655440002",
        "username": "charlie_brown",
        "displayName": "Charlie Brown",
        "avatarUrl": "https://cdn.yourapp.com/avatars/charlie_brown_avatar.jpg",
        "isOnline": false,
        "isVerified": false,
        "followedAt": "2025-10-20T14:30:00Z",
        "isFriend": true,
        "isFollowingBack": true
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 63,
      "totalCount": 1250,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 27. Get Following

**Endpoint**: `GET /api/v1/profiles/{userId}/following`

**Description**: Get list of users this user is following

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/following?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "following": [
      {
        "userId": "880e8400-e29b-41d4-a716-446655440001",
        "username": "david_lee",
        "displayName": "David Lee",
        "avatarUrl": "https://cdn.yourapp.com/avatars/david_lee_avatar.jpg",
        "isOnline": true,
        "isVerified": true,
        "followedAt": "2025-09-10T08:00:00Z",
        "isFriend": false,
        "followsBack": true
      },
      {
        "userId": "880e8400-e29b-41d4-a716-446655440002",
        "username": "emma_davis",
        "displayName": "Emma Davis",
        "avatarUrl": "https://cdn.yourapp.com/avatars/emma_davis_avatar.jpg",
        "isOnline": false,
        "isVerified": true,
        "followedAt": "2025-08-05T12:00:00Z",
        "isFriend": true,
        "followsBack": true
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 25,
      "totalCount": 487,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 28. Get Mutual Friends

**Endpoint**: `GET /api/v1/profiles/{userId}/mutual-friends`

**Description**: Get list of mutual friends with another user

**Request**:
```http
GET /api/v1/profiles/660e8400-e29b-41d4-a716-446655440001/mutual-friends HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "mutualFriends": [
      {
        "userId": "990e8400-e29b-41d4-a716-446655440001",
        "username": "frank_martin",
        "displayName": "Frank Martin",
        "avatarUrl": "https://cdn.yourapp.com/avatars/frank_martin_avatar.jpg",
        "isOnline": true,
        "isVerified": false
      },
      {
        "userId": "990e8400-e29b-41d4-a716-446655440002",
        "username": "grace_wilson",
        "displayName": "Grace Wilson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/grace_wilson_avatar.jpg",
        "isOnline": false,
        "isVerified": true
      }
    ],
    "count": 45
  }
}
```

---

### 29. Check Friendship Status

**Endpoint**: `GET /api/v1/profiles/{userId}/friendship-status`

**Description**: Check friendship status with another user

**Request**:
```http
GET /api/v1/profiles/660e8400-e29b-41d4-a716-446655440001/friendship-status HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "660e8400-e29b-41d4-a716-446655440001",
    "areFriends": true,
    "isFollowing": true,
    "isFollower": true,
    "hasPendingFriendRequest": false,
    "friendRequestSentBy": null,
    "mutualFriendsCount": 45
  }
}
```

**Another Example** (Not friends, pending request):
```json
{
  "success": true,
  "data": {
    "userId": "660e8400-e29b-41d4-a716-446655440003",
    "areFriends": false,
    "isFollowing": true,
    "isFollower": false,
    "hasPendingFriendRequest": true,
    "friendRequestSentBy": "me",
    "mutualFriendsCount": 8
  }
}
```

---

## Friend Requests

### 30. Send Friend Request

**Endpoint**: `POST /api/v1/friend-requests`

**Description**: Send a friend request to another user

**Request**:
```http
POST /api/v1/friend-requests HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "receiverId": "660e8400-e29b-41d4-a716-446655440005",
  "message": "Hi! We met at the tech conference last week. Would love to connect!"
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Friend request sent successfully",
  "data": {
    "_id": "freq-67890abcdef1234567890001",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "john_doe",
    "senderDisplayName": "John Doe",
    "senderAvatarUrl": "https://cdn.yourapp.com/avatars/john_doe_avatar.jpg",
    "receiverId": "660e8400-e29b-41d4-a716-446655440005",
    "receiverUsername": "sarah_connor",
    "message": "Hi! We met at the tech conference last week. Would love to connect!",
    "status": "Pending",
    "createdAt": "2025-12-02T13:00:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_FRIENDS",
    "message": "You are already friends with this user",
    "statusCode": 400
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "REQUEST_ALREADY_EXISTS",
    "message": "A friend request already exists between you and this user",
    "statusCode": 400
  }
}
```

---

### 31. Get Pending Friend Requests

**Endpoint**: `GET /api/v1/friend-requests/pending`

**Description**: Get all pending friend requests (received)

**Request**:
```http
GET /api/v1/friend-requests/pending?type=received&page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `type`: `received` or `sent` (default: `received`)
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 20)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "friendRequests": [
      {
        "_id": "freq-67890abcdef1234567890010",
        "senderId": "aa0e8400-e29b-41d4-a716-446655440001",
        "senderUsername": "mike_ross",
        "senderDisplayName": "Mike Ross",
        "senderAvatarUrl": "https://cdn.yourapp.com/avatars/mike_ross_avatar.jpg",
        "message": "Hey! Found your profile through the Python developers group.",
        "status": "Pending",
        "createdAt": "2025-12-01T15:30:00Z",
        "mutualFriendsCount": 12
      },
      {
        "_id": "freq-67890abcdef1234567890011",
        "senderId": "bb0e8400-e29b-41d4-a716-446655440002",
        "senderUsername": "rachel_zane",
        "senderDisplayName": "Rachel Zane",
        "senderAvatarUrl": "https://cdn.yourapp.com/avatars/rachel_zane_avatar.jpg",
        "message": null,
        "status": "Pending",
        "createdAt": "2025-11-30T10:00:00Z",
        "mutualFriendsCount": 5
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 1,
      "totalCount": 2,
      "hasNextPage": false,
      "hasPreviousPage": false
    }
  }
}
```

---

### 32. Accept Friend Request

**Endpoint**: `POST /api/v1/friend-requests/{requestId}/accept`

**Description**: Accept a pending friend request

**Request**:
```http
POST /api/v1/friend-requests/freq-67890abcdef1234567890010/accept HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Friend request accepted successfully",
  "data": {
    "friendRequestId": "freq-67890abcdef1234567890010",
    "status": "Accepted",
    "newFriend": {
      "userId": "aa0e8400-e29b-41d4-a716-446655440001",
      "username": "mike_ross",
      "displayName": "Mike Ross",
      "avatarUrl": "https://cdn.yourapp.com/avatars/mike_ross_avatar.jpg"
    },
    "respondedAt": "2025-12-02T13:15:00Z"
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "error": {
    "code": "NOT_FOUND",
    "message": "Friend request not found",
    "statusCode": 404
  }
}
```

---

### 33. Reject Friend Request

**Endpoint**: `POST /api/v1/friend-requests/{requestId}/reject`

**Description**: Reject a pending friend request

**Request**:
```http
POST /api/v1/friend-requests/freq-67890abcdef1234567890011/reject HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Friend request rejected successfully",
  "data": {
    "friendRequestId": "freq-67890abcdef1234567890011",
    "status": "Rejected",
    "respondedAt": "2025-12-02T13:20:00Z"
  }
}
```

---

### 34. Cancel Friend Request

**Endpoint**: `DELETE /api/v1/friend-requests/{requestId}`

**Description**: Cancel a friend request you sent

**Request**:
```http
DELETE /api/v1/friend-requests/freq-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Friend request cancelled successfully"
}
```

---

### 35. Unfriend User

**Endpoint**: `DELETE /api/v1/profiles/{userId}/friend`

**Description**: Remove a user from your friends list

**Request**:
```http
DELETE /api/v1/profiles/aa0e8400-e29b-41d4-a716-446655440001/friend HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "User removed from friends successfully",
  "data": {
    "removedUserId": "aa0e8400-e29b-41d4-a716-446655440001",
    "removedUsername": "mike_ross"
  }
}
```

---

## Follow System

### 36. Follow User

**Endpoint**: `POST /api/v1/profiles/{userId}/follow`

**Description**: Follow another user

**Request**:
```http
POST /api/v1/profiles/cc0e8400-e29b-41d4-a716-446655440001/follow HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "User followed successfully",
  "data": {
    "followedUserId": "cc0e8400-e29b-41d4-a716-446655440001",
    "followedUsername": "tech_influencer",
    "followedAt": "2025-12-02T13:30:00Z",
    "isFollowingBack": false
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_FOLLOWING",
    "message": "You are already following this user",
    "statusCode": 400
  }
}
```

---

### 37. Unfollow User

**Endpoint**: `DELETE /api/v1/profiles/{userId}/follow`

**Description**: Unfollow a user

**Request**:
```http
DELETE /api/v1/profiles/cc0e8400-e29b-41d4-a716-446655440001/follow HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "User unfollowed successfully",
  "data": {
    "unfollowedUserId": "cc0e8400-e29b-41d4-a716-446655440001",
    "unfollowedUsername": "tech_influencer"
  }
}
```

---

## Block System

### 38. Block User

**Endpoint**: `POST /api/v1/profiles/{userId}/block`

**Description**: Block a user (automatically unfriends and prevents interaction)

**Request**:
```http
POST /api/v1/profiles/dd0e8400-e29b-41d4-a716-446655440001/block HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "reason": "Spam"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "User blocked successfully",
  "data": {
    "blockedUserId": "dd0e8400-e29b-41d4-a716-446655440001",
    "blockedUsername": "spam_account",
    "blockedAt": "2025-12-02T13:40:00Z"
  }
}
```

---

### 39. Unblock User

**Endpoint**: `DELETE /api/v1/profiles/{userId}/block`

**Description**: Unblock a previously blocked user

**Request**:
```http
DELETE /api/v1/profiles/dd0e8400-e29b-41d4-a716-446655440001/block HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "User unblocked successfully",
  "data": {
    "unblockedUserId": "dd0e8400-e29b-41d4-a716-446655440001",
    "unblockedUsername": "spam_account"
  }
}
```

---

### 40. Get Blocked Users

**Endpoint**: `GET /api/v1/profiles/me/blocked`

**Description**: Get list of users you have blocked

**Request**:
```http
GET /api/v1/profiles/me/blocked?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "blockedUsers": [
      {
        "userId": "dd0e8400-e29b-41d4-a716-446655440001",
        "username": "spam_account",
        "displayName": "Spam Account",
        "avatarUrl": "https://cdn.yourapp.com/avatars/default_avatar.png",
        "blockedAt": "2025-11-20T10:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 1,
      "totalCount": 1,
      "hasNextPage": false,
      "hasPreviousPage": false
    }
  }
}
```

---

## Privacy Settings

### 41. Get Privacy Settings

**Endpoint**: `GET /api/v1/profiles/me/privacy-settings`

**Description**: Get current privacy settings

**Request**:
```http
GET /api/v1/profiles/me/privacy-settings HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "privacySettings": {
      "profileVisibility": "Friends",
      "showEmail": false,
      "showPhoneNumber": false,
      "showDateOfBirth": false,
      "showLocation": true,
      "showOnlineStatus": true,
      "showLastSeen": false,
      "allowFriendRequests": true,
      "allowMessages": "Friends",
      "showEducation": true,
      "showExperience": true,
      "showConnections": true,
      "allowTagging": "Friends",
      "allowMentions": "Everyone"
    }
  }
}
```

---

### 42. Update Privacy Settings

**Endpoint**: `PUT /api/v1/profiles/me/privacy-settings`

**Description**: Update privacy settings

**Request**:
```http
PUT /api/v1/profiles/me/privacy-settings HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "profileVisibility": "Public",
  "showEmail": false,
  "showPhoneNumber": false,
  "showDateOfBirth": false,
  "showLocation": true,
  "showOnlineStatus": true,
  "showLastSeen": true,
  "allowFriendRequests": true,
  "allowMessages": "Everyone",
  "showEducation": true,
  "showExperience": true,
  "showConnections": true,
  "allowTagging": "Friends",
  "allowMentions": "Everyone"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Privacy settings updated successfully",
  "data": {
    "privacySettings": {
      "profileVisibility": "Public",
      "showEmail": false,
      "showPhoneNumber": false,
      "showDateOfBirth": false,
      "showLocation": true,
      "showOnlineStatus": true,
      "showLastSeen": true,
      "allowFriendRequests": true,
      "allowMessages": "Everyone",
      "showEducation": true,
      "showExperience": true,
      "showConnections": true,
      "allowTagging": "Friends",
      "allowMentions": "Everyone"
    },
    "updatedAt": "2025-12-02T14:00:00Z"
  }
}
```

**Validation Notes**:
- `profileVisibility`: `Public`, `Friends`, `Private`
- `allowMessages`: `Everyone`, `Friends`, `None`
- `allowTagging`: `Everyone`, `Friends`, `None`
- `allowMentions`: `Everyone`, `Friends`, `None`

---

## Notification Settings

### 43. Get Notification Settings

**Endpoint**: `GET /api/v1/profiles/me/notification-settings`

**Description**: Get current notification preferences

**Request**:
```http
GET /api/v1/profiles/me/notification-settings HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "notificationSettings": {
      "emailNotifications": true,
      "pushNotifications": true,
      "messageNotifications": true,
      "friendRequestNotifications": true,
      "postNotifications": true,
      "commentNotifications": true,
      "likeNotifications": false,
      "mentionNotifications": true,
      "followerNotifications": true
    }
  }
}
```

---

### 44. Update Notification Settings

**Endpoint**: `PUT /api/v1/profiles/me/notification-settings`

**Description**: Update notification preferences

**Request**:
```http
PUT /api/v1/profiles/me/notification-settings HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "emailNotifications": true,
  "pushNotifications": true,
  "messageNotifications": true,
  "friendRequestNotifications": true,
  "postNotifications": false,
  "commentNotifications": true,
  "likeNotifications": false,
  "mentionNotifications": true,
  "followerNotifications": false
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Notification settings updated successfully",
  "data": {
    "notificationSettings": {
      "emailNotifications": true,
      "pushNotifications": true,
      "messageNotifications": true,
      "friendRequestNotifications": true,
      "postNotifications": false,
      "commentNotifications": true,
      "likeNotifications": false,
      "mentionNotifications": true,
      "followerNotifications": false
    },
    "updatedAt": "2025-12-02T14:10:00Z"
  }
}
```

---

## Search & Discovery

### 45. Search Users

**Endpoint**: `GET /api/v1/profiles/search`

**Description**: Search for users by name, username, or other criteria

**Request**:
```http
GET /api/v1/profiles/search?q=john&location=San%20Francisco&page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `q`: Search query (username, display name, bio) - **required**
- `location`: Filter by location (optional)
- `skills`: Filter by skills (comma-separated) (optional)
- `verified`: Filter verified users (true/false) (optional)
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 20, max: 100)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "users": [
      {
        "userId": "ee0e8400-e29b-41d4-a716-446655440001",
        "username": "john_smith",
        "displayName": "John Smith",
        "bio": "Software Engineer at Google",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_smith_avatar.jpg",
        "location": {
          "city": "San Francisco",
          "state": "California",
          "country": "United States"
        },
        "isVerified": true,
        "isOnline": true,
        "friendshipStatus": {
          "areFriends": false,
          "isFollowing": false,
          "hasPendingRequest": false
        },
        "mutualFriendsCount": 5
      },
      {
        "userId": "ff0e8400-e29b-41d4-a716-446655440002",
        "username": "johnny_walker",
        "displayName": "Johnny Walker",
        "bio": "Product Manager | Tech Enthusiast",
        "avatarUrl": "https://cdn.yourapp.com/avatars/johnny_walker_avatar.jpg",
        "location": {
          "city": "San Francisco",
          "state": "California",
          "country": "United States"
        },
        "isVerified": false,
        "isOnline": false,
        "friendshipStatus": {
          "areFriends": false,
          "isFollowing": true,
          "hasPendingRequest": false
        },
        "mutualFriendsCount": 12
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 5,
      "totalCount": 87,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 46. Get Friend Suggestions

**Endpoint**: `GET /api/v1/profiles/suggestions`

**Description**: Get personalized friend suggestions based on mutual friends, location, interests

**Request**:
```http
GET /api/v1/profiles/suggestions?limit=10 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "suggestions": [
      {
        "userId": "gg0e8400-e29b-41d4-a716-446655440001",
        "username": "lisa_anderson",
        "displayName": "Lisa Anderson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/lisa_anderson_avatar.jpg",
        "bio": "Data Scientist | AI Researcher",
        "location": {
          "city": "San Francisco",
          "state": "California"
        },
        "isVerified": true,
        "mutualFriendsCount": 23,
        "commonInterests": ["Programming", "AI/ML", "Photography"],
        "suggestionReason": "23 mutual friends and common interests"
      },
      {
        "userId": "hh0e8400-e29b-41d4-a716-446655440002",
        "username": "mark_johnson",
        "displayName": "Mark Johnson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/mark_johnson_avatar.jpg",
        "bio": "Full Stack Developer",
        "location": {
          "city": "San Francisco",
          "state": "California"
        },
        "isVerified": false,
        "mutualFriendsCount": 18,
        "commonInterests": ["Programming", "Travel"],
        "suggestionReason": "18 mutual friends in your area"
      }
    ],
    "count": 10
  }
}
```

---

### 47. Get People You May Know

**Endpoint**: `GET /api/v1/profiles/people-you-may-know`

**Description**: Get extended network suggestions (friends of friends)

**Request**:
```http
GET /api/v1/profiles/people-you-may-know?limit=15 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "suggestions": [
      {
        "userId": "ii0e8400-e29b-41d4-a716-446655440001",
        "username": "sarah_miller",
        "displayName": "Sarah Miller",
        "avatarUrl": "https://cdn.yourapp.com/avatars/sarah_miller_avatar.jpg",
        "bio": "UX Designer | Creative Thinker",
        "mutualFriendsCount": 8,
        "mutualFriends": [
          {
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "jane_smith",
            "displayName": "Jane Smith",
            "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith_avatar.jpg"
          },
          {
            "userId": "660e8400-e29b-41d4-a716-446655440002",
            "username": "bob_wilson",
            "displayName": "Bob Wilson",
            "avatarUrl": "https://cdn.yourapp.com/avatars/bob_wilson_avatar.jpg"
          }
        ],
        "suggestionReason": "Friends with Jane Smith and 7 others"
      }
    ],
    "count": 15
  }
}
```

---

## Activity & Analytics

### 48. Get User Activities

**Endpoint**: `GET /api/v1/profiles/{userId}/activities`

**Description**: Get recent activity history for a user (if privacy allows)

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/activities?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "activities": [
      {
        "_id": "act-67890abcdef1234567890001",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "activityType": "PostCreated",
        "metadata": {
          "postId": "post-12345",
          "content": "Just launched my new project! 🚀"
        },
        "createdAt": "2025-12-02T10:00:00Z"
      },
      {
        "_id": "act-67890abcdef1234567890002",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "activityType": "ProfileUpdated",
        "metadata": {
          "fields": ["bio", "coverImage"]
        },
        "createdAt": "2025-12-01T14:30:00Z"
      },
      {
        "_id": "act-67890abcdef1234567890003",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "activityType": "FriendAdded",
        "metadata": {
          "friendId": "aa0e8400-e29b-41d4-a716-446655440001",
          "friendUsername": "mike_ross"
        },
        "createdAt": "2025-11-30T09:15:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 3,
      "totalCount": 52,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 49. Get Profile Views

**Endpoint**: `GET /api/v1/profiles/me/views`

**Description**: Get list of users who viewed your profile (premium feature)

**Request**:
```http
GET /api/v1/profiles/me/views?days=7&page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "views": [
      {
        "viewerId": "jj0e8400-e29b-41d4-a716-446655440001",
        "viewerUsername": "emma_watson",
        "viewerDisplayName": "Emma Watson",
        "viewerAvatarUrl": "https://cdn.yourapp.com/avatars/emma_watson_avatar.jpg",
        "viewedAt": "2025-12-02T09:30:00Z",
        "viewCount": 3
      },
      {
        "viewerId": "kk0e8400-e29b-41d4-a716-446655440002",
        "viewerUsername": "daniel_craig",
        "viewerDisplayName": "Daniel Craig",
        "viewerAvatarUrl": "https://cdn.yourapp.com/avatars/daniel_craig_avatar.jpg",
        "viewedAt": "2025-12-01T16:45:00Z",
        "viewCount": 1
      }
    ],
    "summary": {
      "totalViews": 127,
      "uniqueViewers": 89,
      "period": "Last 7 days"
    },
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

**Error Response** (403 Forbidden - Non-premium user):
```json
{
  "success": false,
  "error": {
    "code": "PREMIUM_FEATURE",
    "message": "This feature is only available for premium users",
    "statusCode": 403,
    "upgradeUrl": "https://yourapp.com/premium"
  }
}
```

---

### 50. Track Profile View

**Endpoint**: `POST /api/v1/profiles/{userId}/view`

**Description**: Track that you viewed a user's profile (analytics)

**Request**:
```http
POST /api/v1/profiles/660e8400-e29b-41d4-a716-446655440001/view HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (204 No Content):
```
(No response body)
```

**Note**: This is typically called automatically by the frontend when viewing a profile.

---

## User Content

### 51. Get User Posts

**Endpoint**: `GET /api/v1/profiles/{userId}/posts`

**Description**: Get all posts created by a user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/posts?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "posts": [
      {
        "_id": "post-67890abcdef1234567890001",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe_avatar.jpg",
        "content": "Just launched my new project! 🚀 Check it out at https://myproject.com",
        "mediaUrls": [
          "https://cdn.yourapp.com/posts/image1.jpg",
          "https://cdn.yourapp.com/posts/image2.jpg"
        ],
        "hashtags": ["project", "launch", "coding"],
        "mentions": [],
        "likesCount": 234,
        "commentsCount": 42,
        "sharesCount": 18,
        "isLikedByMe": true,
        "visibility": "Public",
        "createdAt": "2025-12-02T10:00:00Z",
        "updatedAt": "2025-12-02T10:00:00Z"
      },
      {
        "_id": "post-67890abcdef1234567890002",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe_avatar.jpg",
        "content": "Beautiful sunset at the beach today 🌅",
        "mediaUrls": [
          "https://cdn.yourapp.com/posts/sunset.jpg"
        ],
        "hashtags": ["sunset", "beach", "nature"],
        "mentions": [],
        "likesCount": 567,
        "commentsCount": 89,
        "sharesCount": 34,
        "isLikedByMe": false,
        "visibility": "Public",
        "createdAt": "2025-12-01T18:30:00Z",
        "updatedAt": "2025-12-01T18:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 8,
      "totalCount": 156,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 52. Get User Photos

**Endpoint**: `GET /api/v1/profiles/{userId}/photos`

**Description**: Get all photos posted by a user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/photos?page=1&limit=30 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "photos": [
      {
        "mediaId": "media-67890abcdef1234567890001",
        "url": "https://cdn.yourapp.com/photos/photo1.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/photos/thumbs/photo1.jpg",
        "postId": "post-67890abcdef1234567890001",
        "caption": "Beautiful sunset at the beach",
        "likesCount": 567,
        "commentsCount": 89,
        "uploadedAt": "2025-12-01T18:30:00Z"
      },
      {
        "mediaId": "media-67890abcdef1234567890002",
        "url": "https://cdn.yourapp.com/photos/photo2.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/photos/thumbs/photo2.jpg",
        "postId": "post-67890abcdef1234567890002",
        "caption": "My new project launch",
        "likesCount": 234,
        "commentsCount": 42,
        "uploadedAt": "2025-12-02T10:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 30,
      "totalPages": 3,
      "totalCount": 89,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 53. Get User Videos

**Endpoint**: `GET /api/v1/profiles/{userId}/videos`

**Description**: Get all videos posted by a user

**Request**:
```http
GET /api/v1/profiles/550e8400-e29b-41d4-a716-446655440000/videos?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "videos": [
      {
        "mediaId": "media-67890abcdef1234567890010",
        "url": "https://cdn.yourapp.com/videos/video1.mp4",
        "thumbnailUrl": "https://cdn.yourapp.com/videos/thumbs/video1.jpg",
        "postId": "post-67890abcdef1234567890010",
        "caption": "Demo of my new app",
        "duration": 125,
        "likesCount": 423,
        "commentsCount": 67,
        "viewsCount": 2341,
        "uploadedAt": "2025-11-28T14:00:00Z"
      },
      {
        "mediaId": "media-67890abcdef1234567890011",
        "url": "https://cdn.yourapp.com/videos/video2.mp4",
        "thumbnailUrl": "https://cdn.yourapp.com/videos/thumbs/video2.jpg",
        "postId": "post-67890abcdef1234567890011",
        "caption": "Coding tutorial - React Hooks",
        "duration": 453,
        "likesCount": 891,
        "commentsCount": 134,
        "viewsCount": 5678,
        "uploadedAt": "2025-11-15T09:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 1,
      "totalCount": 12,
      "hasNextPage": false,
      "hasPreviousPage": false
    }
  }
}
```

---

## Account Settings

### 54. Get Account Settings

**Endpoint**: `GET /api/v1/profiles/me/account-settings`

**Description**: Get current account settings (language, timezone, theme)

**Request**:
```http
GET /api/v1/profiles/me/account-settings HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "accountSettings": {
      "language": "en",
      "timezone": "America/Los_Angeles",
      "dateFormat": "MM/DD/YYYY",
      "theme": "Dark"
    }
  }
}
```

---

### 55. Update Account Settings

**Endpoint**: `PUT /api/v1/profiles/me/account-settings`

**Description**: Update account preferences

**Request**:
```http
PUT /api/v1/profiles/me/account-settings HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "language": "en",
  "timezone": "America/New_York",
  "dateFormat": "DD/MM/YYYY",
  "theme": "Light"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Account settings updated successfully",
  "data": {
    "accountSettings": {
      "language": "en",
      "timezone": "America/New_York",
      "dateFormat": "DD/MM/YYYY",
      "theme": "Light"
    },
    "updatedAt": "2025-12-02T14:30:00Z"
  }
}
```

**Validation Notes**:
- `language`: ISO 639-1 code (e.g., "en", "es", "fr", "de")
- `timezone`: IANA timezone (e.g., "America/New_York", "Europe/London")
- `dateFormat`: "MM/DD/YYYY", "DD/MM/YYYY", "YYYY-MM-DD"
- `theme`: "Light", "Dark", "Auto"

---

### 56. Deactivate Account

**Endpoint**: `POST /api/v1/profiles/me/deactivate`

**Description**: Temporarily deactivate account (can be reactivated by logging in)

**Request**:
```http
POST /api/v1/profiles/me/deactivate HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "reason": "Taking a break from social media",
  "password": "user_password_confirmation"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Account deactivated successfully",
  "data": {
    "deactivatedAt": "2025-12-02T14:45:00Z",
    "reactivationInfo": "You can reactivate your account anytime by logging in"
  }
}
```

---

### 57. Delete Account

**Endpoint**: `DELETE /api/v1/profiles/me`

**Description**: Permanently delete account and all associated data (cannot be undone)

**Request**:
```http
DELETE /api/v1/profiles/me HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "password": "user_password_confirmation",
  "confirmationText": "DELETE MY ACCOUNT"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Account deletion initiated. Your account and all data will be permanently deleted within 30 days. You can cancel this request by logging in during this period.",
  "data": {
    "scheduledDeletionDate": "2026-01-01T14:50:00Z",
    "cancellationDeadline": "2026-01-01T14:50:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_CONFIRMATION",
    "message": "Confirmation text must be 'DELETE MY ACCOUNT'",
    "statusCode": 400
  }
}
```

---

## Error Responses

### Standard Error Format

All error responses follow this structure:

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "statusCode": 400,
    "details": []
  }
}
```

### Common Error Codes

#### 400 Bad Request
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "statusCode": 400,
    "details": [
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ]
  }
}
```

#### 401 Unauthorized
```json
{
  "success": false,
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Authentication required",
    "statusCode": 401
  }
}
```

#### 403 Forbidden
```json
{
  "success": false,
  "error": {
    "code": "FORBIDDEN",
    "message": "You don't have permission to access this resource",
    "statusCode": 403
  }
}
```

#### 404 Not Found
```json
{
  "success": false,
  "error": {
    "code": "NOT_FOUND",
    "message": "Resource not found",
    "statusCode": 404
  }
}
```

#### 409 Conflict
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_EXISTS",
    "message": "Resource already exists",
    "statusCode": 409
  }
}
```

#### 413 Payload Too Large
```json
{
  "success": false,
  "error": {
    "code": "FILE_TOO_LARGE",
    "message": "File size exceeds maximum allowed size",
    "statusCode": 413,
    "details": {
      "maxSize": "5MB",
      "actualSize": "8MB"
    }
  }
}
```

#### 415 Unsupported Media Type
```json
{
  "success": false,
  "error": {
    "code": "INVALID_FILE_TYPE",
    "message": "File type not supported",
    "statusCode": 415,
    "details": {
      "allowedTypes": ["image/jpeg", "image/png", "image/webp"],
      "receivedType": "image/gif"
    }
  }
}
```

#### 429 Too Many Requests
```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Please try again later.",
    "statusCode": 429,
    "details": {
      "retryAfter": 60,
      "limit": 100,
      "window": "1 minute"
    }
  }
}
```

#### 500 Internal Server Error
```json
{
  "success": false,
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "An unexpected error occurred",
    "statusCode": 500,
    "requestId": "req-67890abcdef1234567890xyz"
  }
}
```

---

## HTTP Status Codes Reference

| Status Code | Meaning | Usage |
|------------|---------|-------|
| 200 | OK | Successful GET, PUT, DELETE |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful request with no response body |
| 400 | Bad Request | Validation errors, invalid input |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | Valid token but insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource already exists, duplicate entry |
| 413 | Payload Too Large | File upload exceeds size limit |
| 415 | Unsupported Media Type | Invalid file type |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server-side error |
| 503 | Service Unavailable | Server temporarily unavailable |

---

## Rate Limiting

All API endpoints are rate-limited to prevent abuse:

**Rate Limit Headers** (included in all responses):
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1701518400
```

**Default Limits**:
- **Authenticated requests**: 1000 requests per hour
- **Search endpoints**: 100 requests per hour
- **Upload endpoints**: 50 requests per hour
- **Profile view tracking**: 500 requests per hour

---

## Pagination

Paginated endpoints follow this structure:

**Query Parameters**:
- `page`: Page number (default: 1, min: 1)
- `limit`: Items per page (default: 20, min: 1, max: 100)

**Response Structure**:
```json
{
  "success": true,
  "data": {
    "items": [...],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 10,
      "totalCount": 200,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

## File Upload Specifications

### Avatar Upload
- **Max Size**: 5 MB
- **Allowed Types**: JPEG, PNG, WebP
- **Recommended Dimensions**: 400x400 px (square)
- **Processing**: Auto-cropped to square, resized to 400x400px

### Cover Image Upload
- **Max Size**: 10 MB
- **Allowed Types**: JPEG, PNG, WebP
- **Recommended Dimensions**: 1920x480 px (4:1 ratio)
- **Processing**: Auto-resized to fit dimensions

---

## Best Practices for UI Development

1. **Authentication**: Always include the Bearer token in the Authorization header
2. **Error Handling**: Display user-friendly messages from `error.message`
3. **Loading States**: Show loading indicators during API calls
4. **Pagination**: Implement infinite scroll or pagination controls
5. **Image Optimization**: Use thumbnail URLs when available
6. **Real-time Updates**: Consider using WebSockets for online status and notifications
7. **Caching**: Cache profile data and invalidate on updates
8. **Retry Logic**: Implement exponential backoff for failed requests
9. **Validation**: Perform client-side validation before API calls
10. **Privacy**: Respect user privacy settings and hide restricted information

---

## Example: Complete Profile Setup Flow

```javascript
// 1. Get current user profile
const profile = await fetch('/api/v1/profiles/me', {
  headers: { 'Authorization': `Bearer ${token}` }
});

// 2. Update basic info
await fetch('/api/v1/profiles/me', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    bio: "Software Engineer | Tech Enthusiast",
    location: { city: "San Francisco", state: "CA", country: "US" }
  })
});

// 3. Upload avatar
const formData = new FormData();
formData.append('avatar', avatarFile);
await fetch('/api/v1/profiles/me/avatar', {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` },
  body: formData
});

// 4. Add education
await fetch('/api/v1/profiles/me/education', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    school: "Stanford University",
    degree: "BS",
    fieldOfStudy: "Computer Science",
    startDate: "2008-09-01",
    endDate: "2012-06-15"
  })
});

// 5. Add experience
await fetch('/api/v1/profiles/me/experience', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    company: "Google",
    position: "Senior Software Engineer",
    current: true,
    startDate: "2020-03-01"
  })
});

// 6. Add skills
await fetch('/api/v1/profiles/me/skills', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    skills: [
      { name: "Python", level: "Expert", yearsOfExperience: 8 },
      { name: "JavaScript", level: "Advanced", yearsOfExperience: 6 }
    ]
  })
});
```

---

## WebSocket / Real-Time Features

For real-time features (online status, typing indicators, new messages), use SignalR:

**Connection**:
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/userProfile", {
    accessTokenFactory: () => authToken
  })
  .build();

await connection.start();

// Subscribe to online status updates
connection.on("UserOnlineStatusChanged", (userId, isOnline) => {
  console.log(`User ${userId} is now ${isOnline ? 'online' : 'offline'}`);
});

// Subscribe to profile updates
connection.on("ProfileUpdated", (userId, updatedFields) => {
  console.log(`User ${userId} updated their profile:`, updatedFields);
});
```

---

**End of Documentation**

For additional API documentation, see:
- [Auth API Documentation](../Auth/API_REFERENCE.md)
- [Chat API Documentation](../Chat/API_REFERENCE.md)
- [PostFeed API Documentation](../PostFeed/API_REFERENCE.md)
- [Media API Documentation](../Media/API_REFERENCE.md)
- [Notification API Documentation](../Notification/API_REFERENCE.md)
