// ========================================
// Media Service - Indexes
// ========================================

db.media_files.createIndex({ "uploadedBy": 1, "createdAt": -1 }, { name: "idx_uploadedBy_createdAt" });
db.media_files.createIndex({ "mediaType": 1, "status": 1, "createdAt": -1 }, { name: "idx_mediaType_status_createdAt" });
db.media_files.createIndex({ "status": 1 }, { name: "idx_status" });
db.media_files.createIndex({ "tags": 1 }, { name: "idx_tags" });
db.media_files.createIndex({ "isPublic": 1, "isDeleted": 1 }, { name: "idx_isPublic_isDeleted" });

print("Media indexes created");
