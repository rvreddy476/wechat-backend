// =============================================
// WeChat.com - UserProfileService MongoDB Schema
// Collection: profiles
// Purpose: User profile data and social information
// =============================================

// Database and collection names
const DB_NAME = 'wechat_profiles';
const COLLECTION_NAME = 'profiles';

// Switch to database
use(DB_NAME);

// Drop collection if exists (for clean setup - REMOVE IN PRODUCTION!)
// db.profiles.drop();

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["userId", "username", "displayName", "createdAt", "updatedAt"],
            properties: {
                userId: {
                    bsonType: "string",
                    description: "User ID from AuthService (UUID) - required"
                },
                username: {
                    bsonType: "string",
                    minLength: 3,
                    maxLength: 50,
                    description: "Unique username - required"
                },
                displayName: {
                    bsonType: "string",
                    minLength: 1,
                    maxLength: 100,
                    description: "Display name - required"
                },
                bio: {
                    bsonType: ["string", "null"],
                    maxLength: 500,
                    description: "User biography/description"
                },
                avatarUrl: {
                    bsonType: ["string", "null"],
                    description: "Profile picture URL"
                },
                bannerUrl: {
                    bsonType: ["string", "null"],
                    description: "Profile banner/cover image URL"
                },
                location: {
                    bsonType: ["string", "null"],
                    maxLength: 100,
                    description: "User location (city, country)"
                },
                website: {
                    bsonType: ["string", "null"],
                    maxLength: 200,
                    description: "Personal website or portfolio URL"
                },
                birthDate: {
                    bsonType: ["date", "null"],
                    description: "User's birth date"
                },
                gender: {
                    bsonType: ["string", "null"],
                    enum: ["male", "female", "other", "prefer_not_to_say", null],
                    description: "User's gender"
                },
                verified: {
                    bsonType: "bool",
                    description: "Whether user is verified (blue checkmark)"
                },
                verifiedType: {
                    bsonType: ["string", "null"],
                    enum: ["individual", "business", "government", "celebrity", null],
                    description: "Type of verification"
                },
                isPrivate: {
                    bsonType: "bool",
                    description: "Whether profile is private"
                },
                allowMessagesFrom: {
                    bsonType: "string",
                    enum: ["everyone", "followers", "following", "mutual_follows", "none"],
                    description: "Who can send messages to this user"
                },
                // Social Stats
                stats: {
                    bsonType: "object",
                    required: ["followersCount", "followingCount", "postsCount", "videosCount", "shortsCount"],
                    properties: {
                        followersCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of followers"
                        },
                        followingCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of users being followed"
                        },
                        postsCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of posts created"
                        },
                        videosCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of videos uploaded"
                        },
                        shortsCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of shorts uploaded"
                        },
                        likesReceived: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Total likes received across all content"
                        },
                        viewsReceived: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Total views received across all videos"
                        }
                    }
                },
                // Privacy Settings
                privacy: {
                    bsonType: "object",
                    properties: {
                        showEmail: {
                            bsonType: "bool",
                            description: "Show email on profile"
                        },
                        showPhoneNumber: {
                            bsonType: "bool",
                            description: "Show phone number on profile"
                        },
                        showBirthDate: {
                            bsonType: "bool",
                            description: "Show birth date on profile"
                        },
                        showLocation: {
                            bsonType: "bool",
                            description: "Show location on profile"
                        },
                        allowTagging: {
                            bsonType: "bool",
                            description: "Allow others to tag this user"
                        },
                        showOnlineStatus: {
                            bsonType: "bool",
                            description: "Show online/offline status"
                        },
                        indexProfile: {
                            bsonType: "bool",
                            description: "Allow profile to appear in search engines"
                        }
                    }
                },
                // Notification Preferences
                notifications: {
                    bsonType: "object",
                    properties: {
                        emailNotifications: {
                            bsonType: "bool",
                            description: "Enable email notifications"
                        },
                        pushNotifications: {
                            bsonType: "bool",
                            description: "Enable push notifications"
                        },
                        notifyOnFollow: {
                            bsonType: "bool",
                            description: "Notify when someone follows"
                        },
                        notifyOnComment: {
                            bsonType: "bool",
                            description: "Notify on comments"
                        },
                        notifyOnLike: {
                            bsonType: "bool",
                            description: "Notify on likes"
                        },
                        notifyOnMention: {
                            bsonType: "bool",
                            description: "Notify when mentioned"
                        },
                        notifyOnMessage: {
                            bsonType: "bool",
                            description: "Notify on new messages"
                        },
                        notifyOnVideoProcessed: {
                            bsonType: "bool",
                            description: "Notify when video processing completes"
                        }
                    }
                },
                // Custom fields for extensibility
                customFields: {
                    bsonType: ["object", "null"],
                    description: "Custom profile fields (JSON)"
                },
                // Social Links
                socialLinks: {
                    bsonType: ["object", "null"],
                    description: "Social media links",
                    properties: {
                        twitter: { bsonType: ["string", "null"] },
                        instagram: { bsonType: ["string", "null"] },
                        facebook: { bsonType: ["string", "null"] },
                        linkedin: { bsonType: ["string", "null"] },
                        youtube: { bsonType: ["string", "null"] },
                        tiktok: { bsonType: ["string", "null"] },
                        github: { bsonType: ["string", "null"] }
                    }
                },
                // Timestamps
                createdAt: {
                    bsonType: "date",
                    description: "Profile creation timestamp - required"
                },
                updatedAt: {
                    bsonType: "date",
                    description: "Last update timestamp - required"
                },
                lastActiveAt: {
                    bsonType: ["date", "null"],
                    description: "Last activity timestamp"
                },
                // Soft delete
                isDeleted: {
                    bsonType: "bool",
                    description: "Soft delete flag"
                },
                deletedAt: {
                    bsonType: ["date", "null"],
                    description: "Deletion timestamp"
                }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "error"
});

// Create indexes
print("Creating indexes for profiles collection...");

// Unique indexes
db.profiles.createIndex({ "userId": 1 }, { unique: true, name: "idx_userId_unique" });
db.profiles.createIndex({ "username": 1 }, { unique: true, partialFilterExpression: { isDeleted: false }, name: "idx_username_unique" });

// Query performance indexes
db.profiles.createIndex({ "username": 1, "isDeleted": 1 }, { name: "idx_username_isDeleted" });
db.profiles.createIndex({ "displayName": 1 }, { name: "idx_displayName" });
db.profiles.createIndex({ "verified": 1 }, { name: "idx_verified" });
db.profiles.createIndex({ "isPrivate": 1 }, { name: "idx_isPrivate" });

// Stats indexes for leaderboards and discovery
db.profiles.createIndex({ "stats.followersCount": -1 }, { name: "idx_followersCount_desc" });
db.profiles.createIndex({ "stats.postsCount": -1 }, { name: "idx_postsCount_desc" });
db.profiles.createIndex({ "stats.videosCount": -1 }, { name: "idx_videosCount_desc" });
db.profiles.createIndex({ "stats.likesReceived": -1 }, { name: "idx_likesReceived_desc" });
db.profiles.createIndex({ "stats.viewsReceived": -1 }, { name: "idx_viewsReceived_desc" });

// Timestamp indexes
db.profiles.createIndex({ "createdAt": -1 }, { name: "idx_createdAt_desc" });
db.profiles.createIndex({ "lastActiveAt": -1 }, { name: "idx_lastActiveAt_desc" });

// Text search index
db.profiles.createIndex(
    {
        "username": "text",
        "displayName": "text",
        "bio": "text",
        "location": "text"
    },
    {
        name: "idx_text_search",
        weights: {
            "username": 10,
            "displayName": 8,
            "bio": 5,
            "location": 2
        },
        default_language: "english"
    }
);

// Compound indexes for common queries
db.profiles.createIndex({ "verified": 1, "stats.followersCount": -1 }, { name: "idx_verified_followers" });
db.profiles.createIndex({ "isPrivate": 1, "isDeleted": 1 }, { name: "idx_privacy_deleted" });
db.profiles.createIndex({ "location": 1, "isDeleted": 1 }, { name: "idx_location_deleted" });

print("Indexes created successfully!");

// Sample document structure (for reference)
const sampleProfile = {
    userId: "123e4567-e89b-12d3-a456-426614174000",
    username: "john_doe",
    displayName: "John Doe",
    bio: "Content creator | Tech enthusiast | Travel lover",
    avatarUrl: "https://cdn.wechat.com/avatars/john_doe.jpg",
    bannerUrl: "https://cdn.wechat.com/banners/john_doe.jpg",
    location: "San Francisco, CA",
    website: "https://johndoe.com",
    birthDate: new Date("1990-01-15"),
    gender: "male",
    verified: true,
    verifiedType: "individual",
    isPrivate: false,
    allowMessagesFrom: "followers",
    stats: {
        followersCount: 15420,
        followingCount: 523,
        postsCount: 342,
        videosCount: 128,
        shortsCount: 89,
        likesReceived: 245000,
        viewsReceived: 3500000
    },
    privacy: {
        showEmail: false,
        showPhoneNumber: false,
        showBirthDate: false,
        showLocation: true,
        allowTagging: true,
        showOnlineStatus: true,
        indexProfile: true
    },
    notifications: {
        emailNotifications: true,
        pushNotifications: true,
        notifyOnFollow: true,
        notifyOnComment: true,
        notifyOnLike: true,
        notifyOnMention: true,
        notifyOnMessage: true,
        notifyOnVideoProcessed: true
    },
    socialLinks: {
        twitter: "@johndoe",
        instagram: "johndoe",
        youtube: "johndoevlogs",
        github: "johndoe"
    },
    customFields: {},
    createdAt: new Date(),
    updatedAt: new Date(),
    lastActiveAt: new Date(),
    isDeleted: false,
    deletedAt: null
};

print("\n=============================================");
print("profiles collection setup completed!");
print("Sample document structure available in code");
print("=============================================\n");
