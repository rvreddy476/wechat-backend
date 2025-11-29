// ========================================
// PostFeed Service - Hashtags Collection
// ========================================

db.createCollection("hashtags", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["tag", "usageCount", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique hashtag identifier (lowercase tag)"
        },
        tag: {
          bsonType: "string",
          description: "Hashtag text (without #, lowercase)"
        },
        displayTag: {
          bsonType: "string",
          description: "Original case hashtag for display"
        },
        usageCount: {
          bsonType: "int",
          description: "Number of times this hashtag has been used",
          minimum: 0
        },
        trendingScore: {
          bsonType: ["double", "null"],
          description: "Calculated trending score"
        },
        lastUsedAt: {
          bsonType: "date",
          description: "Last time this hashtag was used"
        },
        createdAt: {
          bsonType: "date",
          description: "When hashtag was first created"
        },
        updatedAt: {
          bsonType: "date",
          description: "Last update timestamp"
        }
      }
    }
  }
});

// Unique index on tag (case-insensitive)
db.hashtags.createIndex(
  { "tag": 1 },
  { unique: true, collation: { locale: "en", strength: 2 }, name: "idx_tag_unique_ci" }
);

print("Hashtags collection created with validation schema and unique index");
