# Media Service Database Documentation

## Overview

The Media service uses **MongoDB** for storing media file metadata and references.

## Collections

### media_files
Stores metadata for uploaded files (images, videos, audio, documents).

**Key Fields:**
- `uploadedBy` - User ID who uploaded
- `fileName` - Original file name
- `fileUrl` - Storage URL
- `thumbnailUrl` - Thumbnail URL
- `mediaType` - Image, Video, Audio, Document, Other
- `fileSize` - Size in bytes
- `status` - Uploading, Processing, Ready, Failed

## Connection String

```bash
mongodb://localhost:27017/wechat_media
```

## Setup

```bash
mongosh
use wechat_media
load('Collections/01_media_files.js')
load('Indexes/01_media_indexes.js')
```

## Common Operations

```bash
# Backup
mongodump --db=wechat_media --gzip --archive=/backup/media_$(date +%Y%m%d).gz

# Restore
mongorestore --gzip --archive=/backup/media_20240115.gz
```
