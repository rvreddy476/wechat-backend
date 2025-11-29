// ========================================
// UserProfile Service - User Profiles Collection
// ========================================

db.createCollection("user_profiles", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["userId", "username", "email", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique profile identifier (same as userId from Auth service)"
        },
        userId: {
          bsonType: "string",
          description: "UUID from Auth service (same as _id)"
        },
        username: {
          bsonType: "string",
          description: "Username from Auth service"
        },
        displayName: {
          bsonType: ["string", "null"],
          description: "Display name for the user"
        },
        email: {
          bsonType: "string",
          description: "Email address from Auth service"
        },
        phoneNumber: {
          bsonType: ["string", "null"],
          description: "Phone number"
        },
        bio: {
          bsonType: ["string", "null"],
          description: "User biography/description",
          maxLength: 500
        },
        avatarUrl: {
          bsonType: ["string", "null"],
          description: "Profile picture URL"
        },
        coverImageUrl: {
          bsonType: ["string", "null"],
          description: "Cover/banner image URL"
        },
        location: {
          bsonType: ["string", "null"],
          description: "User location (city, country)"
        },
        website: {
          bsonType: ["string", "null"],
          description: "Personal website URL"
        },
        dateOfBirth: {
          bsonType: ["date", "null"],
          description: "Date of birth"
        },
        gender: {
          enum: ["Male", "Female", "Other", "PreferNotToSay", null],
          description: "Gender identity"
        },
        friends: {
          bsonType: "array",
          description: "List of friend user IDs",
          items: {
            bsonType: "string"
          }
        },
        followers: {
          bsonType: "array",
          description: "List of follower user IDs",
          items: {
            bsonType: "string"
          }
        },
        following: {
          bsonType: "array",
          description: "List of user IDs being followed",
          items: {
            bsonType: "string"
          }
        },
        blockedUsers: {
          bsonType: "array",
          description: "List of blocked user IDs",
          items: {
            bsonType: "string"
          }
        },
        isOnline: {
          bsonType: "bool",
          description: "Current online status"
        },
        lastSeenAt: {
          bsonType: ["date", "null"],
          description: "Last time user was active"
        },
        privacySettings: {
          bsonType: "object",
          description: "Privacy preferences",
          properties: {
            profileVisibility: {
              enum: ["Public", "FriendsOnly", "Private"],
              description: "Who can view profile"
            },
            showOnlineStatus: {
              bsonType: "bool",
              description: "Show online/offline status"
            },
            showLastSeen: {
              bsonType: "bool",
              description: "Show last seen timestamp"
            },
            allowFriendRequests: {
              bsonType: "bool",
              description: "Accept friend requests"
            },
            allowMessages: {
              enum: ["Everyone", "FriendsOnly", "Nobody"],
              description: "Who can send messages"
            }
          }
        },
        notificationSettings: {
          bsonType: "object",
          description: "Notification preferences",
          properties: {
            emailNotifications: {
              bsonType: "bool"
            },
            pushNotifications: {
              bsonType: "bool"
            },
            messageNotifications: {
              bsonType: "bool"
            },
            friendRequestNotifications: {
              bsonType: "bool"
            },
            postNotifications: {
              bsonType: "bool"
            }
          }
        },
        statistics: {
          bsonType: "object",
          description: "User statistics",
          properties: {
            friendsCount: {
              bsonType: "int",
              minimum: 0
            },
            followersCount: {
              bsonType: "int",
              minimum: 0
            },
            followingCount: {
              bsonType: "int",
              minimum: 0
            },
            postsCount: {
              bsonType: "int",
              minimum: 0
            }
          }
        },
        isVerified: {
          bsonType: "bool",
          description: "Verified user badge"
        },
        isDeleted: {
          bsonType: "bool",
          description: "Soft delete flag"
        },
        deletedAt: {
          bsonType: ["date", "null"],
          description: "When the profile was soft deleted"
        },
        createdAt: {
          bsonType: "date",
          description: "Profile creation timestamp"
        },
        updatedAt: {
          bsonType: "date",
          description: "Last update timestamp"
        }
      }
    }
  }
});

print("User profiles collection created with validation schema");
