# Emoji Handling Guide - WeChat Social Media Platform

## Overview

Emojis are Unicode characters that require proper encoding throughout the entire stack:
- **Frontend**: JavaScript/TypeScript (UTF-16)
- **API**: JSON (UTF-8)
- **Backend**: C# (.NET uses UTF-16 internally)
- **Database**: UTF-8 encoding

---

## 1. Database Configuration

### MongoDB (Already Configured ✅)

**MongoDB stores all strings as UTF-8 by default** - no special configuration needed!

```javascript
// Example document with emojis
{
  "_id": "65b4f3c2a1b2c3d4e5f60001",
  "content": "Hello! 👋 Check out this awesome post! 🚀🎉",
  "reactions": ["❤️", "👍", "😂", "🔥"]
}
```

**MongoDB Configuration** (already correct):
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_posts"
  }
}
```

### PostgreSQL (Auth Service)

**PostgreSQL** also uses UTF-8 by default, but verify encoding:

```sql
-- Check current encoding
SHOW SERVER_ENCODING;  -- Should show UTF8

-- Set encoding when creating database
CREATE DATABASE wechat_auth
  ENCODING 'UTF8'
  LC_COLLATE 'en_US.UTF-8'
  LC_CTYPE 'en_US.UTF-8';
```

**Connection String** (already correct):
```
Host=localhost;Port=5432;Database=wechat_auth;Username=wechat_admin;Password=***;Encoding=UTF8
```

---

## 2. Frontend to Backend Flow

### Frontend (React/Next.js)

#### Sending Emojis in Requests

```typescript
// src/services/postService.ts
import { api } from './api';

export const postService = {
  async createPost(content: string, mediaUrls: string[] = []) {
    // Emojis are automatically handled by JavaScript
    // No special encoding needed!
    const response = await api.post('/api/posts', {
      content: content, // Can contain emojis: "Hello! 👋"
      mediaUrls: mediaUrls
    });

    return response.data;
  },

  async addComment(postId: string, content: string) {
    const response = await api.post(`/api/posts/${postId}/comments`, {
      content: content // "Great post! 😍🔥"
    });

    return response.data;
  },

  async sendReaction(postId: string, emoji: string) {
    // Send single emoji as reaction
    const response = await api.post(`/api/posts/${postId}/reactions`, {
      reactionType: emoji // "❤️" or "👍" or "😂"
    });

    return response.data;
  }
};
```

#### Example React Component

```tsx
// src/components/PostComposer.tsx
import { useState } from 'react';
import EmojiPicker from 'emoji-picker-react';

export function PostComposer() {
  const [content, setContent] = useState('');
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);

  const handleEmojiClick = (emojiObject: any) => {
    // Append emoji to content
    setContent(prevContent => prevContent + emojiObject.emoji);
  };

  const handleSubmit = async () => {
    // Content can contain emojis: "Hello! 👋🎉"
    await postService.createPost(content);
  };

  return (
    <div>
      <textarea
        value={content}
        onChange={(e) => setContent(e.target.value)}
        placeholder="What's on your mind? 😊"
      />

      <button onClick={() => setShowEmojiPicker(!showEmojiPicker)}>
        😀 Emoji
      </button>

      {showEmojiPicker && (
        <EmojiPicker onEmojiClick={handleEmojiClick} />
      )}

      <button onClick={handleSubmit}>Post</button>
    </div>
  );
}
```

#### Axios Configuration (Already Correct)

```typescript
// src/services/api.ts
import axios from 'axios';

export const api = axios.create({
  baseURL: 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json; charset=utf-8' // UTF-8 encoding
  }
});

// Axios automatically handles UTF-8 encoding for JSON
```

---

## 3. Backend (C# / .NET)

### API Controllers

**.NET automatically handles UTF-8 JSON encoding** - no special configuration needed!

```csharp
// src/PostFeedService.Api/Controllers/PostsController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Post>>> CreatePost(
        [FromBody] CreatePostRequest request)
    {
        // request.Content can contain emojis: "Hello! 👋🎉"
        // .NET handles UTF-8 automatically

        var post = new Post
        {
            UserId = GetCurrentUserId(),
            Content = request.Content, // Emojis preserved
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.CreatePostAsync(post);

        return Ok(ApiResponse<Post>.SuccessResponse(result.Value));
    }

    [HttpPost("{postId}/reactions")]
    public async Task<ActionResult<ApiResponse<Reaction>>> AddReaction(
        string postId,
        [FromBody] AddReactionRequest request)
    {
        // request.ReactionType can be emoji: "❤️", "👍", "😂"

        var reaction = new Reaction
        {
            PostId = postId,
            UserId = GetCurrentUserId(),
            ReactionType = request.ReactionType, // Emoji stored as-is
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddReactionAsync(reaction);

        return Ok(ApiResponse<Reaction>.SuccessResponse(result.Value));
    }
}
```

### Request Models

```csharp
// Models/Requests.cs
public class CreatePostRequest
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty; // Can contain emojis
}

public class AddReactionRequest
{
    [Required]
    [StringLength(10)] // Emojis can be 1-4 bytes in UTF-8
    public string ReactionType { get; set; } = string.Empty;
}

public class CreateCommentRequest
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty; // Can contain emojis
}
```

### MongoDB Models

```csharp
// Models/Post.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Post : Entity<string>
{
    [BsonElement("content")]
    public string Content { get; set; } = string.Empty; // Emojis stored as UTF-8

    [BsonElement("hashtags")]
    public List<string> Hashtags { get; set; } = new(); // Can include emoji hashtags

    [BsonElement("reactions")]
    public Dictionary<string, int> ReactionCounts { get; set; } = new(); // Key can be emoji

    // Example: { "❤️": 42, "👍": 25, "😂": 18 }
}

public class Comment : Entity<string>
{
    [BsonElement("content")]
    public string Content { get; set; } = string.Empty; // Emojis stored as UTF-8
}

public class Reaction : Entity<string>
{
    [BsonElement("reactionType")]
    public string ReactionType { get; set; } = string.Empty; // Emoji: "❤️", "👍", etc.
}
```

### MongoDB Repository

```csharp
// Repositories/PostRepository.cs
using MongoDB.Driver;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _posts;

    public async Task<Result<Post>> CreatePostAsync(Post post)
    {
        try
        {
            // MongoDB automatically handles UTF-8 encoding
            await _posts.InsertOneAsync(post);

            // Emojis are preserved in database
            return Result<Post>.Success(post);
        }
        catch (Exception ex)
        {
            return Result<Post>.Failure($"Failed to create post: {ex.Message}");
        }
    }

    public async Task<Result<Post>> GetPostByIdAsync(string postId)
    {
        try
        {
            var post = await _posts
                .Find(p => p.Id == postId && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return Result<Post>.Failure("Post not found");
            }

            // Emojis are automatically decoded from database
            return Result<Post>.Success(post);
        }
        catch (Exception ex)
        {
            return Result<Post>.Failure($"Error retrieving post: {ex.Message}");
        }
    }
}
```

---

## 4. Special Considerations

### Emoji Length Calculation

⚠️ **Important**: Emojis can be 1-4 bytes in UTF-8, but C# counts them differently!

```csharp
// Example: String length calculation
string text = "Hello! 👋";

// C# string.Length counts UTF-16 code units
Console.WriteLine(text.Length); // 9 (7 chars + 2 for emoji)

// To get actual character count including emojis:
var stringInfo = new System.Globalization.StringInfo(text);
Console.WriteLine(stringInfo.LengthInTextElements); // 8 (7 chars + 1 emoji)

// UTF-8 byte count
var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text);
Console.WriteLine(utf8Bytes.Length); // 11 bytes (7 ASCII + 4 for emoji)
```

### Validation with Emojis

```csharp
// Helper class for emoji-aware validation
public static class EmojiHelper
{
    /// <summary>
    /// Get actual character count including emojis
    /// </summary>
    public static int GetTextLength(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        var stringInfo = new System.Globalization.StringInfo(text);
        return stringInfo.LengthInTextElements;
    }

    /// <summary>
    /// Validate text length including emojis
    /// </summary>
    public static bool IsValidLength(string text, int maxLength)
    {
        return GetTextLength(text) <= maxLength;
    }

    /// <summary>
    /// Check if string contains emojis
    /// </summary>
    public static bool ContainsEmoji(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        foreach (var c in text)
        {
            if (char.IsHighSurrogate(c) || char.IsLowSurrogate(c))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Extract emojis from text
    /// </summary>
    public static List<string> ExtractEmojis(string text)
    {
        var emojis = new List<string>();
        var stringInfo = new System.Globalization.StringInfo(text);

        for (int i = 0; i < stringInfo.LengthInTextElements; i++)
        {
            var element = stringInfo.SubstringByTextElements(i, 1);
            if (element.Length > 1 || char.IsHighSurrogate(element[0]))
            {
                emojis.Add(element);
            }
        }

        return emojis;
    }
}
```

### Using Emoji Helper in Validation

```csharp
// Enhanced validation attribute
public class EmojiAwareMaxLengthAttribute : ValidationAttribute
{
    private readonly int _maxLength;

    public EmojiAwareMaxLengthAttribute(int maxLength)
    {
        _maxLength = maxLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string text)
        {
            var actualLength = EmojiHelper.GetTextLength(text);
            if (actualLength > _maxLength)
            {
                return new ValidationResult(
                    $"Content cannot exceed {_maxLength} characters (currently {actualLength})");
            }
        }

        return ValidationResult.Success;
    }
}

// Usage in request models
public class CreatePostRequest
{
    [Required]
    [EmojiAwareMaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}
```

---

## 5. Common Emoji Use Cases

### Use Case 1: Post Content with Emojis

**Frontend**:
```typescript
const createPost = async () => {
  const response = await api.post('/api/posts', {
    content: "Just launched my new project! 🚀🎉 #coding #webdev"
  });
};
```

**Backend**:
```csharp
// Automatically stored in MongoDB as UTF-8
var post = new Post { Content = "Just launched my new project! 🚀🎉 #coding #webdev" };
```

**Database (MongoDB)**:
```json
{
  "_id": "65b4f3c2a1b2c3d4e5f60001",
  "content": "Just launched my new project! 🚀🎉 #coding #webdev"
}
```

### Use Case 2: Reactions (Like, Love, etc.)

**Frontend**:
```typescript
const addReaction = async (postId: string, emoji: string) => {
  await api.post(`/api/posts/${postId}/reactions`, {
    reactionType: emoji // "❤️" or "👍" or "😂" or "🔥"
  });
};
```

**Backend**:
```csharp
[HttpPost("{postId}/reactions")]
public async Task<ActionResult> AddReaction(string postId, [FromBody] AddReactionRequest request)
{
    var reaction = new Reaction
    {
        PostId = postId,
        UserId = GetCurrentUserId(),
        ReactionType = request.ReactionType // Emoji stored
    };

    await _repository.AddReactionAsync(reaction);

    // Update reaction counts
    await _repository.IncrementReactionCountAsync(postId, request.ReactionType);

    return Ok();
}
```

**Database**:
```json
{
  "_id": "65b4f3c2a1b2c3d4e5f60002",
  "postId": "65b4f3c2a1b2c3d4e5f60001",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "reactionType": "❤️",
  "createdAt": "2025-01-26T10:30:00Z"
}
```

### Use Case 3: Chat Messages with Emojis

**Frontend**:
```typescript
const sendMessage = async (conversationId: string, content: string) => {
  await api.post(`/api/chats/conversations/${conversationId}/messages`, {
    content: "See you tomorrow! 👋😊",
    messageType: "Text"
  });
};
```

**Backend** (automatically handled):
```csharp
var message = new Message
{
    ConversationId = conversationId,
    SenderId = GetCurrentUserId(),
    Content = "See you tomorrow! 👋😊",
    MessageType = MessageType.Text
};
```

### Use Case 4: Emoji-Only Reactions

**Frontend**:
```typescript
const REACTIONS = ['❤️', '👍', '😂', '😮', '😢', '🔥'];

const ReactionButtons = ({ postId }: { postId: string }) => (
  <div>
    {REACTIONS.map(emoji => (
      <button key={emoji} onClick={() => addReaction(postId, emoji)}>
        {emoji}
      </button>
    ))}
  </div>
);
```

**Backend** - Track reaction counts:
```csharp
// Update reaction statistics
public async Task IncrementReactionCountAsync(string postId, string reactionType)
{
    var update = Builders<Post>.Update
        .Inc($"reactionCounts.{reactionType}", 1);

    await _posts.UpdateOneAsync(p => p.Id == postId, update);
}
```

**Database Result**:
```json
{
  "_id": "65b4f3c2a1b2c3d4e5f60001",
  "content": "My post content",
  "reactionCounts": {
    "❤️": 42,
    "👍": 25,
    "😂": 18,
    "🔥": 15,
    "😮": 8
  }
}
```

---

## 6. Testing Emoji Handling

### Unit Test Example

```csharp
using Xunit;

public class EmojiHandlingTests
{
    [Fact]
    public void Should_Store_And_Retrieve_Emojis()
    {
        // Arrange
        var content = "Hello! 👋 This is a test 🚀🎉";
        var post = new Post { Content = content };

        // Act
        var retrieved = post.Content;

        // Assert
        Assert.Equal(content, retrieved);
        Assert.Contains("👋", retrieved);
        Assert.Contains("🚀", retrieved);
    }

    [Fact]
    public void Should_Calculate_Emoji_Length_Correctly()
    {
        // Arrange
        var text = "Hi 👋";

        // Act
        var length = EmojiHelper.GetTextLength(text);

        // Assert
        Assert.Equal(4, length); // "H", "i", " ", "👋"
    }

    [Fact]
    public void Should_Extract_Emojis_From_Text()
    {
        // Arrange
        var text = "Hello! 👋 Nice to meet you 😊";

        // Act
        var emojis = EmojiHelper.ExtractEmojis(text);

        // Assert
        Assert.Equal(2, emojis.Count);
        Assert.Contains("👋", emojis);
        Assert.Contains("😊", emojis);
    }
}
```

---

## 7. Best Practices

### ✅ DO:
1. **Use UTF-8 everywhere** (already configured)
2. **Let frameworks handle encoding** (JSON, MongoDB driver)
3. **Validate emoji-aware length** for user input
4. **Store emojis directly** in database (no conversion needed)
5. **Test with various emojis** including multi-byte ones

### ❌ DON'T:
1. **Don't encode emojis to HTML entities** (❤️ → `&#10084;`)
2. **Don't strip emojis** from user content
3. **Don't use string.Length** for validation (use text elements)
4. **Don't worry about storage** (MongoDB/PostgreSQL handle it)

---

## 8. Configuration Checklist

✅ **MongoDB**: UTF-8 by default (no config needed)
✅ **PostgreSQL**: UTF-8 encoding (already configured)
✅ **Redis**: Binary safe, handles UTF-8
✅ **.NET API**: UTF-8 JSON encoding (automatic)
✅ **Frontend**: UTF-8 in JSON requests (automatic)

---

## Summary

**The good news**: You don't need to do anything special! ✨

1. **Frontend** → **Backend**: Emojis sent in JSON as UTF-8 (automatic)
2. **Backend** → **Database**: Emojis stored as UTF-8 (automatic)
3. **Database** → **Backend** → **Frontend**: Emojis retrieved as-is (automatic)

**Only consideration**: Use emoji-aware length validation in backend for user input limits.

Your current stack already handles emojis perfectly! 🎉
