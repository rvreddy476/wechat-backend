// =============================================
// WeChat.com - UserProfileService MongoDB Queries
// Purpose: Common query patterns and aggregations
// =============================================

const DB_NAME = 'wechat_profiles';
use(DB_NAME);

print("UserProfileService - Common Query Patterns");
print("============================================\n");

// =============================================
// 1. Get Profile by UserId
// =============================================
print("1. Get Profile by UserId:");
const getProfileByUserId = (userId) => {
    return db.profiles.findOne(
        { userId: userId, isDeleted: false },
        {
            projection: {
                _id: 0,
                userId: 1,
                username: 1,
                displayName: 1,
                bio: 1,
                avatarUrl: 1,
                bannerUrl: 1,
                location: 1,
                website: 1,
                verified: 1,
                verifiedType: 1,
                stats: 1,
                socialLinks: 1,
                createdAt: 1,
                lastActiveAt: 1
            }
        }
    );
};
print("Usage: getProfileByUserId('user-uuid')");
print("");

// =============================================
// 2. Get Profile by Username
// =============================================
print("2. Get Profile by Username:");
const getProfileByUsername = (username) => {
    return db.profiles.findOne(
        { username: username.toLowerCase(), isDeleted: false },
        {
            projection: { _id: 0 }
        }
    );
};
print("Usage: getProfileByUsername('john_doe')");
print("");

// =============================================
// 3. Update Profile Stats (Increment/Decrement)
// =============================================
print("3. Update Profile Stats:");
const incrementFollowersCount = (userId, increment = 1) => {
    return db.profiles.updateOne(
        { userId: userId },
        {
            $inc: { "stats.followersCount": increment },
            $set: { updatedAt: new Date() }
        }
    );
};

const incrementPostsCount = (userId, increment = 1) => {
    return db.profiles.updateOne(
        { userId: userId },
        {
            $inc: { "stats.postsCount": increment },
            $set: { updatedAt: new Date() }
        }
    );
};

const incrementVideosCount = (userId, increment = 1) => {
    return db.profiles.updateOne(
        { userId: userId },
        {
            $inc: { "stats.videosCount": increment },
            $set: { updatedAt: new Date() }
        }
    );
};
print("Usage: incrementFollowersCount('user-uuid', 1)");
print("");

// =============================================
// 4. Search Profiles (Text Search)
// =============================================
print("4. Search Profiles:");
const searchProfiles = (searchTerm, limit = 20) => {
    return db.profiles.find(
        {
            $text: { $search: searchTerm },
            isDeleted: false,
            isPrivate: false
        },
        {
            score: { $meta: "textScore" }
        }
    )
    .sort({ score: { $meta: "textScore" } })
    .limit(limit)
    .project({
        _id: 0,
        userId: 1,
        username: 1,
        displayName: 1,
        bio: 1,
        avatarUrl: 1,
        verified: 1,
        stats: 1
    })
    .toArray();
};
print("Usage: searchProfiles('tech creator')");
print("");

// =============================================
// 5. Get Trending Profiles (Most Followers)
// =============================================
print("5. Get Trending Profiles:");
const getTrendingProfiles = (limit = 50) => {
    return db.profiles.find(
        { isDeleted: false, isPrivate: false },
        {
            projection: {
                _id: 0,
                userId: 1,
                username: 1,
                displayName: 1,
                bio: 1,
                avatarUrl: 1,
                verified: 1,
                verifiedType: 1,
                stats: 1
            }
        }
    )
    .sort({ "stats.followersCount": -1 })
    .limit(limit)
    .toArray();
};
print("Usage: getTrendingProfiles(50)");
print("");

// =============================================
// 6. Get Top Creators (Most Content)
// =============================================
print("6. Get Top Creators:");
const getTopCreators = (limit = 50) => {
    return db.profiles.aggregate([
        {
            $match: { isDeleted: false, isPrivate: false }
        },
        {
            $addFields: {
                totalContent: {
                    $add: [
                        "$stats.postsCount",
                        "$stats.videosCount",
                        "$stats.shortsCount"
                    ]
                }
            }
        },
        {
            $sort: { totalContent: -1, "stats.viewsReceived": -1 }
        },
        {
            $limit: limit
        },
        {
            $project: {
                _id: 0,
                userId: 1,
                username: 1,
                displayName: 1,
                bio: 1,
                avatarUrl: 1,
                verified: 1,
                stats: 1,
                totalContent: 1
            }
        }
    ]).toArray();
};
print("Usage: getTopCreators(50)");
print("");

// =============================================
// 7. Get User's Followers
// =============================================
print("7. Get User's Followers:");
const getFollowers = (userId, limit = 50, skip = 0) => {
    return db.follows.aggregate([
        {
            $match: {
                followingId: userId,
                isAccepted: true
            }
        },
        {
            $lookup: {
                from: "profiles",
                localField: "followerId",
                foreignField: "userId",
                as: "followerProfile"
            }
        },
        {
            $unwind: "$followerProfile"
        },
        {
            $match: {
                "followerProfile.isDeleted": false
            }
        },
        {
            $sort: { createdAt: -1 }
        },
        {
            $skip: skip
        },
        {
            $limit: limit
        },
        {
            $project: {
                _id: 0,
                userId: "$followerProfile.userId",
                username: "$followerProfile.username",
                displayName: "$followerProfile.displayName",
                avatarUrl: "$followerProfile.avatarUrl",
                verified: "$followerProfile.verified",
                followedAt: "$createdAt",
                stats: "$followerProfile.stats"
            }
        }
    ]).toArray();
};
print("Usage: getFollowers('user-uuid', 50, 0)");
print("");

// =============================================
// 8. Get Users Being Followed
// =============================================
print("8. Get Users Being Followed:");
const getFollowing = (userId, limit = 50, skip = 0) => {
    return db.follows.aggregate([
        {
            $match: {
                followerId: userId,
                isAccepted: true
            }
        },
        {
            $lookup: {
                from: "profiles",
                localField: "followingId",
                foreignField: "userId",
                as: "followingProfile"
            }
        },
        {
            $unwind: "$followingProfile"
        },
        {
            $match: {
                "followingProfile.isDeleted": false
            }
        },
        {
            $sort: { createdAt: -1 }
        },
        {
            $skip: skip
        },
        {
            $limit: limit
        },
        {
            $project: {
                _id: 0,
                userId: "$followingProfile.userId",
                username: "$followingProfile.username",
                displayName: "$followingProfile.displayName",
                avatarUrl: "$followingProfile.avatarUrl",
                verified: "$followingProfile.verified",
                followedAt: "$createdAt",
                notificationsEnabled: "$notificationsEnabled",
                stats: "$followingProfile.stats"
            }
        }
    ]).toArray();
};
print("Usage: getFollowing('user-uuid', 50, 0)");
print("");

// =============================================
// 9. Check if User A follows User B
// =============================================
print("9. Check Follow Relationship:");
const isFollowing = (followerUserId, followingUserId) => {
    return db.follows.findOne({
        followerId: followerUserId,
        followingId: followingUserId,
        isAccepted: true
    }) !== null;
};
print("Usage: isFollowing('userA-uuid', 'userB-uuid')");
print("");

// =============================================
// 10. Get Mutual Followers
// =============================================
print("10. Get Mutual Followers:");
const getMutualFollowers = (userId1, userId2) => {
    return db.follows.aggregate([
        // Get userId1's followers
        {
            $match: {
                followingId: userId1,
                isAccepted: true
            }
        },
        // Check if they also follow userId2
        {
            $lookup: {
                from: "follows",
                let: { follower: "$followerId" },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $and: [
                                    { $eq: ["$followerId", "$$follower"] },
                                    { $eq: ["$followingId", userId2] },
                                    { $eq: ["$isAccepted", true] }
                                ]
                            }
                        }
                    }
                ],
                as: "alsoFollowsUser2"
            }
        },
        {
            $match: {
                alsoFollowsUser2: { $ne: [] }
            }
        },
        // Get profile info
        {
            $lookup: {
                from: "profiles",
                localField: "followerId",
                foreignField: "userId",
                as: "profile"
            }
        },
        {
            $unwind: "$profile"
        },
        {
            $project: {
                _id: 0,
                userId: "$profile.userId",
                username: "$profile.username",
                displayName: "$profile.displayName",
                avatarUrl: "$profile.avatarUrl",
                verified: "$profile.verified"
            }
        }
    ]).toArray();
};
print("Usage: getMutualFollowers('userA-uuid', 'userB-uuid')");
print("");

// =============================================
// 11. Get Suggested Users to Follow
// =============================================
print("11. Get Suggested Users:");
const getSuggestedUsers = (userId, limit = 20) => {
    return db.follows.aggregate([
        // Get users that current user follows
        {
            $match: {
                followerId: userId,
                isAccepted: true
            }
        },
        // Get who those users follow
        {
            $lookup: {
                from: "follows",
                localField: "followingId",
                foreignField: "followerId",
                as: "theirFollowing"
            }
        },
        {
            $unwind: "$theirFollowing"
        },
        // Exclude already following and self
        {
            $match: {
                "theirFollowing.isAccepted": true,
                "theirFollowing.followingId": { $ne: userId }
            }
        },
        {
            $lookup: {
                from: "follows",
                let: { suggestedUserId: "$theirFollowing.followingId" },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $and: [
                                    { $eq: ["$followerId", userId] },
                                    { $eq: ["$followingId", "$$suggestedUserId"] }
                                ]
                            }
                        }
                    }
                ],
                as: "alreadyFollowing"
            }
        },
        {
            $match: {
                alreadyFollowing: []
            }
        },
        // Count how many mutual connections
        {
            $group: {
                _id: "$theirFollowing.followingId",
                mutualCount: { $sum: 1 }
            }
        },
        {
            $sort: { mutualCount: -1 }
        },
        {
            $limit: limit
        },
        // Get profile info
        {
            $lookup: {
                from: "profiles",
                localField: "_id",
                foreignField: "userId",
                as: "profile"
            }
        },
        {
            $unwind: "$profile"
        },
        {
            $match: {
                "profile.isDeleted": false,
                "profile.isPrivate": false
            }
        },
        {
            $project: {
                _id: 0,
                userId: "$profile.userId",
                username: "$profile.username",
                displayName: "$profile.displayName",
                bio: "$profile.bio",
                avatarUrl: "$profile.avatarUrl",
                verified: "$profile.verified",
                stats: "$profile.stats",
                mutualFollowers: "$mutualCount"
            }
        }
    ]).toArray();
};
print("Usage: getSuggestedUsers('user-uuid', 20)");
print("");

// =============================================
// 12. Check if User is Blocked
// =============================================
print("12. Check if User is Blocked:");
const isBlocked = (blockerId, blockedUserId) => {
    return db.blockedUsers.findOne({
        blockerId: blockerId,
        blockedUserId: blockedUserId,
        isActive: true
    }) !== null;
};

const isBlockedByEither = (userId1, userId2) => {
    return db.blockedUsers.findOne({
        $or: [
            { blockerId: userId1, blockedUserId: userId2, isActive: true },
            { blockerId: userId2, blockedUserId: userId1, isActive: true }
        ]
    }) !== null;
};
print("Usage: isBlocked('blocker-uuid', 'blocked-uuid')");
print("Usage: isBlockedByEither('userA-uuid', 'userB-uuid')");
print("");

// =============================================
// 13. Get Profile Statistics
// =============================================
print("13. Get Profile Statistics:");
const getProfileStats = () => {
    return db.profiles.aggregate([
        {
            $match: { isDeleted: false }
        },
        {
            $group: {
                _id: null,
                totalProfiles: { $sum: 1 },
                verifiedProfiles: {
                    $sum: { $cond: ["$verified", 1, 0] }
                },
                privateProfiles: {
                    $sum: { $cond: ["$isPrivate", 1, 0] }
                },
                avgFollowers: { $avg: "$stats.followersCount" },
                avgPosts: { $avg: "$stats.postsCount" },
                avgVideos: { $avg: "$stats.videosCount" },
                totalLikes: { $sum: "$stats.likesReceived" },
                totalViews: { $sum: "$stats.viewsReceived" }
            }
        },
        {
            $project: {
                _id: 0,
                totalProfiles: 1,
                verifiedProfiles: 1,
                privateProfiles: 1,
                avgFollowers: { $round: ["$avgFollowers", 0] },
                avgPosts: { $round: ["$avgPosts", 0] },
                avgVideos: { $round: ["$avgVideos", 0] },
                totalLikes: 1,
                totalViews: 1
            }
        }
    ]).toArray();
};
print("Usage: getProfileStats()");
print("");

// =============================================
// 14. Get Profiles by Location
// =============================================
print("14. Get Profiles by Location:");
const getProfilesByLocation = (location, limit = 50) => {
    return db.profiles.find(
        {
            location: { $regex: location, $options: "i" },
            isDeleted: false,
            isPrivate: false,
            "privacy.showLocation": true
        },
        {
            projection: {
                _id: 0,
                userId: 1,
                username: 1,
                displayName: 1,
                bio: 1,
                avatarUrl: 1,
                location: 1,
                verified: 1,
                stats: 1
            }
        }
    )
    .sort({ "stats.followersCount": -1 })
    .limit(limit)
    .toArray();
};
print("Usage: getProfilesByLocation('San Francisco', 50)");
print("");

print("\n=============================================");
print("Query patterns defined successfully!");
print("Copy these functions to your application code");
print("=============================================\n");
