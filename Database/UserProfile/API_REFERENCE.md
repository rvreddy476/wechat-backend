# UserProfile Service - Complete API & Database Reference

## Table of Contents
1. [Overview](#overview)
2. [Database Schema](#database-schema)
3. [API Endpoints](#api-endpoints)
4. [Profile Features](#profile-features)
5. [Social Features](#social-features)
6. [Request/Response Examples](#request-response-examples)

---

## Overview

The UserProfile service manages comprehensive user profiles including personal information, media, education, professional data, social connections, and activity tracking.

### Technologies
- **Database**: MongoDB
- **Authentication**: JWT Bearer Token
- **Media Storage**: CDN/Cloud Storage (S3, Azure Blob)
- **Real-time**: SignalR for online status

---

## Database Schema

### Collection: user_profiles

```javascript
{
  // Identity
  _id: "550e8400-e29b-41d4-a716-446655440000",
  userId: "550e8400-e29b-41d4-a716-446655440000",
  username: "john_doe",

  // Basic Info
  displayName: "John Doe",
  firstName: "John",
  lastName: "Doe",
  email: "john@example.com",
  phoneNumber: "+1234567890",
  dateOfBirth: ISODate("1990-01-15"),
  gender: "Male",  // Male, Female, Other, PreferNotToSay

  // Bio & Description
  bio: "Software engineer passionate about building great products",
  tagline: "Code, Coffee, Create",

  // Media
  avatarUrl: "https://cdn.example.com/avatars/john_doe.jpg",
  coverImageUrl: "https://cdn.example.com/covers/john_doe_cover.jpg",

  // Location
  location: {
    city: "San Francisco",
    state: "California",
    country: "United States",
    coordinates: {
      latitude: 37.7749,
      longitude: -122.4194
    }
  },

  // Contact & Links
  website: "https://johndoe.com",
  socialLinks: {
    linkedin: "https://linkedin.com/in/johndoe",
    twitter: "https://twitter.com/johndoe",
    github: "https://github.com/johndoe",
    instagram: "https://instagram.com/johndoe",
    facebook: "https://facebook.com/johndoe"
  },

  // Education
  education: [
    {
      id: "edu-001",
      school: "Stanford University",
      degree: "Bachelor of Science",
      fieldOfStudy: "Computer Science",
      startDate: ISODate("2008-09-01"),
      endDate: ISODate("2012-06-01"),
      grade: "3.8 GPA",
      activities: "Computer Science Club, Hackathons",
      description: "Focused on AI and Machine Learning",
      current: false
    },
    {
      id: "edu-002",
      school: "Stanford University",
      degree: "Master of Science",
      fieldOfStudy: "Artificial Intelligence",
      startDate: ISODate("2012-09-01"),
      endDate: ISODate("2014-06-01"),
      grade: "3.9 GPA",
      activities: "Research Assistant",
      description: "Thesis on Neural Networks",
      current: false
    }
  ],

  // Professional Experience
  experience: [
    {
      id: "exp-001",
      company: "Tech Corp",
      position: "Senior Software Engineer",
      employmentType: "Full-time",  // Full-time, Part-time, Contract, Freelance
      location: "San Francisco, CA",
      startDate: ISODate("2020-01-01"),
      endDate: null,
      current: true,
      description: "Leading the development of microservices architecture",
      skills: ["Node.js", "React", "MongoDB", "AWS"]
    },
    {
      id: "exp-002",
      company: "StartupXYZ",
      position: "Software Engineer",
      employmentType: "Full-time",
      location: "San Francisco, CA",
      startDate: ISODate("2016-06-01"),
      endDate: ISODate("2019-12-31"),
      current: false,
      description: "Built and maintained web applications",
      skills: ["JavaScript", "Python", "PostgreSQL"]
    }
  ],

  // Skills
  skills: [
    {
      name: "JavaScript",
      level: "Expert",  // Beginner, Intermediate, Advanced, Expert
      yearsOfExperience: 8,
      endorsements: 45
    },
    {
      name: "React",
      level: "Expert",
      yearsOfExperience: 6,
      endorsements: 38
    },
    {
      name: "Node.js",
      level: "Advanced",
      yearsOfExperience: 5,
      endorsements: 32
    }
  ],

  // Certifications
  certifications: [
    {
      id: "cert-001",
      name: "AWS Certified Solutions Architect",
      issuingOrganization: "Amazon Web Services",
      issueDate: ISODate("2021-06-15"),
      expirationDate: ISODate("2024-06-15"),
      credentialId: "AWS-12345-67890",
      credentialUrl: "https://aws.amazon.com/verification/..."
    }
  ],

  // Languages
  languages: [
    {
      language: "English",
      proficiency: "Native"  // Native, Fluent, Advanced, Intermediate, Beginner
    },
    {
      language: "Spanish",
      proficiency: "Intermediate"
    }
  ],

  // Interests & Hobbies
  interests: ["Technology", "Artificial Intelligence", "Photography", "Travel", "Hiking"],

  // Social Connections
  friends: ["uuid1", "uuid2", "uuid3"],
  followers: ["uuid1", "uuid2", "uuid3", "uuid4"],
  following: ["uuid1", "uuid2"],
  blockedUsers: [],

  // Online Status
  isOnline: true,
  lastSeenAt: ISODate("2024-01-15T14:30:00Z"),

  // Privacy Settings
  privacySettings: {
    profileVisibility: "Public",  // Public, FriendsOnly, Private
    showEmail: false,
    showPhoneNumber: false,
    showDateOfBirth: false,
    showLocation: true,
    showOnlineStatus: true,
    showLastSeen: true,
    allowFriendRequests: true,
    allowMessages: "Everyone",  // Everyone, FriendsOnly, Nobody
    showEducation: "Public",  // Public, FriendsOnly, Private
    showExperience: "Public",
    showConnections: "FriendsOnly",
    allowTagging: true,
    allowMentions: true
  },

  // Notification Settings
  notificationSettings: {
    emailNotifications: true,
    pushNotifications: true,
    messageNotifications: true,
    friendRequestNotifications: true,
    postNotifications: true,
    commentNotifications: true,
    likeNotifications: true,
    mentionNotifications: true,
    followerNotifications: true
  },

  // Account Settings
  accountSettings: {
    language: "en",
    timezone: "America/Los_Angeles",
    dateFormat: "MM/DD/YYYY",
    theme: "Light"  // Light, Dark, Auto
  },

  // Statistics
  statistics: {
    friendsCount: 150,
    followersCount: 520,
    followingCount: 180,
    postsCount: 87,
    photosCount: 234,
    videosCount: 12,
    profileViewsCount: 1234
  },

  // Verification & Status
  isVerified: true,
  isActive: true,
  isPremium: false,

  // Metadata
  isDeleted: false,
  deletedAt: null,
  createdAt: ISODate("2024-01-01T00:00:00Z"),
  updatedAt: ISODate("2024-01-15T14:30:00Z")
}
```

---

## API Endpoints

### Base URL
```
https://api.example.com/api/userprofile
```

### Authentication
All endpoints require JWT Bearer token in Authorization header:
```
Authorization: Bearer <access_token>
```

---

## Profile Management Endpoints

### 1. Get User Profile

**GET** `/profiles/{userId}`

Get complete profile information.

**Response**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "username": "john_doe",
  "displayName": "John Doe",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Software engineer passionate about...",
  "tagline": "Code, Coffee, Create",
  "avatarUrl": "https://cdn.example.com/avatars/john.jpg",
  "coverImageUrl": "https://cdn.example.com/covers/john_cover.jpg",
  "location": {
    "city": "San Francisco",
    "state": "California",
    "country": "United States"
  },
  "website": "https://johndoe.com",
  "socialLinks": {
    "linkedin": "https://linkedin.com/in/johndoe",
    "twitter": "https://twitter.com/johndoe"
  },
  "isOnline": true,
  "lastSeenAt": "2024-01-15T14:30:00Z",
  "isVerified": true,
  "statistics": {
    "friendsCount": 150,
    "followersCount": 520,
    "followingCount": 180,
    "postsCount": 87
  }
}
```

---

### 2. Get Current User Profile

**GET** `/profiles/me`

Get authenticated user's own profile.

---

### 3. Update Profile

**PUT** `/profiles/me`

**Request Body**:
```json
{
  "displayName": "John Doe Jr.",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Updated bio text",
  "tagline": "New tagline",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "website": "https://johndoe.com",
  "location": {
    "city": "San Francisco",
    "state": "California",
    "country": "United States"
  }
}
```

---

### 4. Update Avatar

**POST** `/profiles/me/avatar`

**Form Data**:
- `file`: Image file (max 5MB, JPG/PNG)

**Response**:
```json
{
  "avatarUrl": "https://cdn.example.com/avatars/john_doe_123456.jpg",
  "thumbnailUrl": "https://cdn.example.com/avatars/john_doe_123456_thumb.jpg"
}
```

---

### 5. Update Cover Photo

**POST** `/profiles/me/cover`

**Form Data**:
- `file`: Image file (max 10MB, JPG/PNG)

**Response**:
```json
{
  "coverImageUrl": "https://cdn.example.com/covers/john_doe_cover_123456.jpg"
}
```

---

### 6. Delete Avatar

**DELETE** `/profiles/me/avatar`

---

### 7. Delete Cover Photo

**DELETE** `/profiles/me/cover`

---

## Education Endpoints

### 8. Add Education

**POST** `/profiles/me/education`

**Request Body**:
```json
{
  "school": "Stanford University",
  "degree": "Bachelor of Science",
  "fieldOfStudy": "Computer Science",
  "startDate": "2008-09-01",
  "endDate": "2012-06-01",
  "grade": "3.8 GPA",
  "activities": "Computer Science Club, Hackathons",
  "description": "Focused on AI and Machine Learning",
  "current": false
}
```

**Response**:
```json
{
  "id": "edu-001",
  "school": "Stanford University",
  "degree": "Bachelor of Science",
  "fieldOfStudy": "Computer Science",
  "startDate": "2008-09-01T00:00:00Z",
  "endDate": "2012-06-01T00:00:00Z",
  "current": false
}
```

---

### 9. Get Education

**GET** `/profiles/{userId}/education`

**Response**:
```json
{
  "education": [
    {
      "id": "edu-001",
      "school": "Stanford University",
      "degree": "Bachelor of Science",
      "fieldOfStudy": "Computer Science",
      "startDate": "2008-09-01T00:00:00Z",
      "endDate": "2012-06-01T00:00:00Z",
      "grade": "3.8 GPA",
      "current": false
    }
  ]
}
```

---

### 10. Update Education

**PUT** `/profiles/me/education/{educationId}`

**Request Body**: Same as Add Education

---

### 11. Delete Education

**DELETE** `/profiles/me/education/{educationId}`

---

## Professional Experience Endpoints

### 12. Add Experience

**POST** `/profiles/me/experience`

**Request Body**:
```json
{
  "company": "Tech Corp",
  "position": "Senior Software Engineer",
  "employmentType": "Full-time",
  "location": "San Francisco, CA",
  "startDate": "2020-01-01",
  "endDate": null,
  "current": true,
  "description": "Leading the development of microservices",
  "skills": ["Node.js", "React", "MongoDB"]
}
```

---

### 13. Get Experience

**GET** `/profiles/{userId}/experience`

---

### 14. Update Experience

**PUT** `/profiles/me/experience/{experienceId}`

---

### 15. Delete Experience

**DELETE** `/profiles/me/experience/{experienceId}`

---

## Skills Endpoints

### 16. Add Skills

**POST** `/profiles/me/skills`

**Request Body**:
```json
{
  "skills": [
    {
      "name": "JavaScript",
      "level": "Expert",
      "yearsOfExperience": 8
    },
    {
      "name": "React",
      "level": "Advanced",
      "yearsOfExperience": 6
    }
  ]
}
```

---

### 17. Get Skills

**GET** `/profiles/{userId}/skills`

---

### 18. Update Skill

**PUT** `/profiles/me/skills/{skillName}`

---

### 19. Delete Skill

**DELETE** `/profiles/me/skills/{skillName}`

---

### 20. Endorse Skill

**POST** `/profiles/{userId}/skills/{skillName}/endorse`

Endorse a user's skill.

---

## Certifications Endpoints

### 21. Add Certification

**POST** `/profiles/me/certifications`

**Request Body**:
```json
{
  "name": "AWS Certified Solutions Architect",
  "issuingOrganization": "Amazon Web Services",
  "issueDate": "2021-06-15",
  "expirationDate": "2024-06-15",
  "credentialId": "AWS-12345-67890",
  "credentialUrl": "https://aws.amazon.com/verification/..."
}
```

---

### 22. Get Certifications

**GET** `/profiles/{userId}/certifications`

---

### 23. Update Certification

**PUT** `/profiles/me/certifications/{certificationId}`

---

### 24. Delete Certification

**DELETE** `/profiles/me/certifications/{certificationId}`

---

## Social Connections Endpoints

### 25. Get Friends

**GET** `/profiles/{userId}/friends`

**Query Parameters**:
- `page` (default: 1)
- `limit` (default: 20)
- `search` (optional)

**Response**:
```json
{
  "friends": [
    {
      "userId": "uuid",
      "username": "jane_smith",
      "displayName": "Jane Smith",
      "avatarUrl": "https://...",
      "isOnline": true,
      "mutualFriendsCount": 15
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 150,
    "totalPages": 8
  }
}
```

---

### 26. Get Followers

**GET** `/profiles/{userId}/followers`

---

### 27. Get Following

**GET** `/profiles/{userId}/following`

---

### 28. Get Mutual Friends

**GET** `/profiles/{userId}/mutual-friends`

Get mutual friends with another user.

---

### 29. Check Friendship Status

**GET** `/profiles/{userId}/friendship-status`

**Response**:
```json
{
  "status": "friends",  // friends, following, not_connected, blocked
  "isFriend": true,
  "isFollowing": false,
  "isFollower": false,
  "isBlocked": false,
  "pendingRequest": null
}
```

---

## Friend Request Endpoints

### 30. Send Friend Request

**POST** `/friend-requests`

**Request Body**:
```json
{
  "receiverId": "550e8400-e29b-41d4-a716-446655440001",
  "message": "Hi! I'd like to connect with you."
}
```

---

### 31. Get Pending Friend Requests

**GET** `/friend-requests/pending`

**Query Parameters**:
- `type`: `received` or `sent`

**Response**:
```json
{
  "friendRequests": [
    {
      "id": "freq-123",
      "senderId": "uuid",
      "senderUsername": "john_doe",
      "senderDisplayName": "John Doe",
      "senderAvatarUrl": "https://...",
      "message": "Hi! I'd like to connect.",
      "mutualFriendsCount": 5,
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ]
}
```

---

### 32. Accept Friend Request

**POST** `/friend-requests/{requestId}/accept`

---

### 33. Reject Friend Request

**POST** `/friend-requests/{requestId}/reject`

---

### 34. Cancel Friend Request

**DELETE** `/friend-requests/{requestId}`

---

### 35. Unfriend User

**DELETE** `/profiles/{userId}/friend`

---

## Follow Endpoints

### 36. Follow User

**POST** `/profiles/{userId}/follow`

---

### 37. Unfollow User

**DELETE** `/profiles/{userId}/follow`

---

## Block Endpoints

### 38. Block User

**POST** `/profiles/{userId}/block`

---

### 39. Unblock User

**DELETE** `/profiles/{userId}/block`

---

### 40. Get Blocked Users

**GET** `/profiles/me/blocked`

---

## Privacy Settings Endpoints

### 41. Get Privacy Settings

**GET** `/profiles/me/privacy-settings`

---

### 42. Update Privacy Settings

**PUT** `/profiles/me/privacy-settings`

**Request Body**:
```json
{
  "profileVisibility": "Public",
  "showEmail": false,
  "showPhoneNumber": false,
  "showDateOfBirth": false,
  "showOnlineStatus": true,
  "showLastSeen": true,
  "allowFriendRequests": true,
  "allowMessages": "FriendsOnly",
  "showEducation": "Public",
  "showExperience": "Public",
  "showConnections": "FriendsOnly"
}
```

---

## Notification Settings Endpoints

### 43. Get Notification Settings

**GET** `/profiles/me/notification-settings`

---

### 44. Update Notification Settings

**PUT** `/profiles/me/notification-settings`

**Request Body**:
```json
{
  "emailNotifications": true,
  "pushNotifications": true,
  "messageNotifications": true,
  "friendRequestNotifications": true,
  "postNotifications": false,
  "commentNotifications": true,
  "likeNotifications": false,
  "mentionNotifications": true
}
```

---

## Search & Discovery Endpoints

### 45. Search Users

**GET** `/profiles/search`

**Query Parameters**:
- `q`: Search query (username, name)
- `location`: Filter by location
- `company`: Filter by company
- `school`: Filter by school
- `page`: Page number
- `limit`: Results per page

**Response**:
```json
{
  "users": [
    {
      "userId": "uuid",
      "username": "john_doe",
      "displayName": "John Doe",
      "avatarUrl": "https://...",
      "bio": "Software engineer...",
      "location": "San Francisco, CA",
      "mutualFriendsCount": 5,
      "connectionType": "following"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 45
  }
}
```

---

### 46. Get Suggested Friends

**GET** `/profiles/suggestions`

Get friend suggestions based on mutual friends, location, interests.

---

### 47. Get People You May Know

**GET** `/profiles/people-you-may-know`

---

## Activity & Analytics Endpoints

### 48. Get Profile Activities

**GET** `/profiles/{userId}/activities`

**Response**:
```json
{
  "activities": [
    {
      "id": "act-123",
      "activityType": "FriendAdded",
      "targetUserId": "uuid",
      "targetUsername": "jane_smith",
      "createdAt": "2024-01-15T10:00:00Z"
    },
    {
      "activityType": "PostCreated",
      "relatedEntityId": "post-456",
      "createdAt": "2024-01-15T09:00:00Z"
    }
  ]
}
```

---

### 49. Get Profile Views

**GET** `/profiles/me/views`

Get who viewed your profile.

---

### 50. Track Profile View

**POST** `/profiles/{userId}/view`

Record a profile view.

---

## User Posts Endpoints

### 51. Get User Posts

**GET** `/profiles/{userId}/posts`

**Query Parameters**:
- `page`: Page number
- `limit`: Posts per page
- `mediaOnly`: true/false (filter posts with media)

**Response**:
```json
{
  "posts": [
    {
      "id": "post-123",
      "authorId": "uuid",
      "authorUsername": "john_doe",
      "content": "Just had an amazing day!",
      "mediaAttachments": [
        {
          "type": "Image",
          "url": "https://...",
          "thumbnailUrl": "https://..."
        }
      ],
      "likesCount": 45,
      "commentsCount": 12,
      "sharesCount": 3,
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 87
  }
}
```

---

### 52. Get User Photos

**GET** `/profiles/{userId}/photos`

Get all photos from user's posts.

---

### 53. Get User Videos

**GET** `/profiles/{userId}/videos`

Get all videos from user's posts.

---

## Account Settings Endpoints

### 54. Get Account Settings

**GET** `/profiles/me/account-settings`

---

### 55. Update Account Settings

**PUT** `/profiles/me/account-settings`

**Request Body**:
```json
{
  "language": "en",
  "timezone": "America/Los_Angeles",
  "dateFormat": "MM/DD/YYYY",
  "theme": "Dark"
}
```

---

### 56. Deactivate Account

**POST** `/profiles/me/deactivate`

Temporarily deactivate account.

---

### 57. Delete Account

**DELETE** `/profiles/me`

Permanently delete account (soft delete).

---

## Database Functions Reference

### MongoDB Operations

#### 1. Create Profile
```javascript
db.user_profiles.insertOne({
  _id: userId,
  userId: userId,
  username: username,
  email: email,
  // ... initial profile data
  createdAt: new Date()
});
```

#### 2. Update Profile
```javascript
db.user_profiles.updateOne(
  { userId: userId },
  {
    $set: {
      displayName: "New Name",
      bio: "Updated bio",
      updatedAt: new Date()
    }
  }
);
```

#### 3. Add Friend
```javascript
// Add to both users' friends arrays
db.user_profiles.updateOne(
  { userId: user1Id },
  {
    $addToSet: { friends: user2Id },
    $inc: { "statistics.friendsCount": 1 }
  }
);

db.user_profiles.updateOne(
  { userId: user2Id },
  {
    $addToSet: { friends: user1Id },
    $inc: { "statistics.friendsCount": 1 }
  }
);
```

#### 4. Add Education
```javascript
db.user_profiles.updateOne(
  { userId: userId },
  {
    $push: {
      education: {
        id: generateId(),
        school: "Stanford University",
        degree: "Bachelor of Science",
        // ... other fields
      }
    }
  }
);
```

#### 5. Add Experience
```javascript
db.user_profiles.updateOne(
  { userId: userId },
  {
    $push: {
      experience: {
        id: generateId(),
        company: "Tech Corp",
        position: "Engineer",
        // ... other fields
      }
    }
  }
);
```

#### 6. Search Profiles
```javascript
db.user_profiles.find({
  $text: { $search: "john developer" },
  isDeleted: false
}).sort({ score: { $meta: "textScore" } });
```

#### 7. Get Mutual Friends
```javascript
db.user_profiles.aggregate([
  { $match: { userId: user1Id } },
  {
    $lookup: {
      from: "user_profiles",
      let: { user1Friends: "$friends" },
      pipeline: [
        { $match: { userId: user2Id } },
        {
          $project: {
            mutualFriends: {
              $setIntersection: ["$friends", "$$user1Friends"]
            }
          }
        }
      ],
      as: "result"
    }
  }
]);
```

---

## Complete Flow Examples

### Example 1: Complete Profile Setup

```javascript
// 1. Update basic info
PUT /api/userprofile/profiles/me
{
  "displayName": "John Doe",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Software engineer passionate about tech",
  "dateOfBirth": "1990-01-15",
  "location": {
    "city": "San Francisco",
    "state": "CA",
    "country": "USA"
  }
}

// 2. Upload avatar
POST /api/userprofile/profiles/me/avatar
FormData: { file: avatarImage }

// 3. Upload cover photo
POST /api/userprofile/profiles/me/cover
FormData: { file: coverImage }

// 4. Add education
POST /api/userprofile/profiles/me/education
{
  "school": "Stanford University",
  "degree": "BS",
  "fieldOfStudy": "Computer Science",
  "startDate": "2008-09-01",
  "endDate": "2012-06-01"
}

// 5. Add experience
POST /api/userprofile/profiles/me/experience
{
  "company": "Tech Corp",
  "position": "Senior Engineer",
  "startDate": "2020-01-01",
  "current": true
}

// 6. Add skills
POST /api/userprofile/profiles/me/skills
{
  "skills": [
    { "name": "JavaScript", "level": "Expert" },
    { "name": "React", "level": "Advanced" }
  ]
}
```

---

### Example 2: Friend Request Flow

```javascript
// 1. Search for user
GET /api/userprofile/profiles/search?q=jane

// 2. Check if already friends
GET /api/userprofile/profiles/{userId}/friendship-status

// 3. Send friend request
POST /api/userprofile/friend-requests
{
  "receiverId": "user-uuid",
  "message": "Hi! Let's connect."
}

// 4. Receiver gets pending requests
GET /api/userprofile/friend-requests/pending?type=received

// 5. Accept request
POST /api/userprofile/friend-requests/{requestId}/accept

// 6. Now they're friends - can view each other's friends-only content
```

---

## Error Responses

All endpoints return standard error responses:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": [
      {
        "field": "dateOfBirth",
        "message": "Invalid date format"
      }
    ]
  }
}
```

**Error Codes**:
- `VALIDATION_ERROR` - Invalid input data
- `NOT_FOUND` - Profile not found
- `UNAUTHORIZED` - Not authenticated
- `FORBIDDEN` - No permission to access resource
- `ALREADY_EXISTS` - Resource already exists (e.g., duplicate friend request)
- `FILE_TOO_LARGE` - Uploaded file exceeds size limit
- `INVALID_FILE_TYPE` - Unsupported file format

---

## Best Practices

1. **Privacy**: Always check privacy settings before showing profile data
2. **Blocking**: Filter out blocked users from all queries
3. **Pagination**: Always paginate lists (friends, followers, posts)
4. **Image Upload**: Validate file size and type before upload
5. **Real-time**: Use SignalR for online status updates
6. **Caching**: Cache frequently accessed profiles in Redis
7. **Verification**: Show verified badge for verified users
8. **Search**: Use text indexes for fast profile search
9. **Analytics**: Track profile views for user insights
10. **Security**: Validate all user input, sanitize bio/description fields

---

This document covers all UserProfile functionality including media, education, professional data, skills, certifications, social connections, and privacy controls!
