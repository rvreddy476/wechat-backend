# NotificationService.Api - Firebase Cloud Messaging (FCM) Setup

This service uses **Firebase Cloud Messaging (FCM)** for push notifications to iOS, Android, and Web platforms.

## Firebase Setup Instructions

### 1. Create Firebase Project

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click **"Add project"**
3. Enter project name: `wechat-notifications` (or your preferred name)
4. (Optional) Enable Google Analytics
5. Click **"Create project"**

### 2. Enable Cloud Messaging

1. In Firebase Console, select your project
2. Click the **gear icon** (Project Settings) in the left sidebar
3. Go to **"Cloud Messaging"** tab
4. Note down your **Server Key** (legacy) and **Sender ID** (for reference)

### 3. Generate Service Account Key

1. In Firebase Console, go to **Project Settings** → **Service Accounts**
2. Click **"Generate new private key"**
3. Confirm by clicking **"Generate key"**
4. A JSON file will be downloaded (e.g., `wechat-notifications-firebase-adminsdk-xxxxx.json`)

### 4. Configure NotificationService.Api

#### Option A: Place JSON in config folder (Recommended)

```bash
# Create config directory
mkdir -p src/NotificationService.Api/config

# Copy your Firebase service account JSON file
cp ~/Downloads/wechat-notifications-firebase-adminsdk-xxxxx.json \
   src/NotificationService.Api/config/firebase-service-account.json
```

**Update appsettings.json:**
```json
{
  "NotificationSettings": {
    "FirebaseCredentialsPath": "config/firebase-service-account.json",
    "EnablePushNotifications": true
  }
}
```

#### Option B: Use Environment Variables (Production)

```bash
# Set environment variable pointing to your JSON file
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/firebase-service-account.json"
```

**Update appsettings.json:**
```json
{
  "NotificationSettings": {
    "EnablePushNotifications": true
  }
}
```

### 5. Add Firebase to Your Mobile/Web Apps

#### For Android:
1. In Firebase Console, click **"Add app"** → Android
2. Register your app with package name (e.g., `com.wechat.app`)
3. Download `google-services.json`
4. Add to your Android project's `app/` directory

#### For iOS:
1. In Firebase Console, click **"Add app"** → iOS
2. Register your app with bundle ID (e.g., `com.wechat.app`)
3. Download `GoogleService-Info.plist`
4. Add to your Xcode project

#### For Web:
1. In Firebase Console, click **"Add app"** → Web
2. Register your web app
3. Copy the Firebase config
4. Add Firebase SDK to your web app

### 6. Test Push Notifications

Run the NotificationService.Api:

```bash
cd src/NotificationService.Api
dotnet run
```

Send a test notification via API:

```bash
curl -X POST http://localhost:5006/api/notifications/send \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "recipientId": "USER_GUID",
    "type": "SystemAnnouncement",
    "title": "Test Notification",
    "message": "Hello from FCM!",
    "deliveryChannels": ["Push"],
    "priority": "High"
  }'
```

## Features

### Push Notification Capabilities

✅ **Multi-Platform Support**
- iOS (APNs via FCM)
- Android (FCM)
- Web Push (FCM)

✅ **Platform-Specific Customization**
- Android notification channels
- iOS badge counts
- Web notification actions
- Custom sounds and vibrations

✅ **Advanced Features**
- Topic-based messaging (broadcast to groups)
- Condition-based targeting
- Batch sending (up to 500 devices)
- Automatic invalid token cleanup
- Message priority (High/Normal)
- Rich notifications (images, actions)

### Notification Channels (Android)

The service automatically assigns notifications to appropriate channels:

| Channel ID | Notification Types |
|------------|-------------------|
| `messages` | NewMessage, MessageReaction, GroupMessageMention |
| `interactions` | Like, Comment, Reply, Mention |
| `social` | Follow, FollowRequestAccepted, FollowRequestReceived |
| `videos` | VideoLike, VideoComment, NewVideoFromSubscription |
| `posts` | PostComment, PostLike, PostShare |
| `system` | SystemAnnouncement, SecurityAlert |
| `default` | All other types |

## Client Integration

### Register Device Token

When users install your app, register their device token:

```bash
POST /api/notifications/device-tokens
{
  "token": "FCM_DEVICE_TOKEN_FROM_CLIENT",
  "platform": "Android",  # or "iOS", "Web"
  "deviceId": "unique-device-id",
  "deviceName": "Samsung Galaxy S21",
  "appVersion": "1.0.0"
}
```

### Subscribe to Topics

Allow users to subscribe to topics for broadcast notifications:

```bash
POST /api/notifications/topics/subscribe
{
  "topic": "breaking-news",
  "deviceTokens": ["token1", "token2"]
}
```

### Unregister Device

When users log out or uninstall:

```bash
DELETE /api/notifications/device-tokens/{tokenId}
```

## Monitoring & Logs

The service logs all FCM operations:

- ✅ Successful sends with message IDs
- ⚠️ Failed sends with error details
- 🔧 Invalid token removal
- 📊 Batch send statistics

Example logs:
```
[INFO] Successfully sent FCM message: projects/wechat-notifications/messages/0:1234567890
[INFO] Sent 95/100 FCM notifications to user 550e8400-e29b-41d4-a716-446655440000
[WARN] Removed invalid token for user 550e8400-e29b-41d4-a716-446655440000
```

## Production Considerations

### Security

1. **Never commit the service account JSON to Git**
   ```bash
   # Add to .gitignore
   echo "config/firebase-service-account.json" >> .gitignore
   ```

2. **Use environment variables in production**
   ```bash
   export GOOGLE_APPLICATION_CREDENTIALS="/secure/path/firebase-credentials.json"
   ```

3. **Restrict API access with proper authentication**

### Scalability

- FCM is **completely free** with no limits
- Supports millions of messages per day
- Batch API handles up to 500 messages per request
- Service uses Redis backplane for horizontal scaling

### Error Handling

The service automatically:
- Retries failed sends
- Removes invalid/unregistered tokens
- Handles platform-specific errors
- Updates delivery status in database

## Troubleshooting

### Push notifications not working?

1. **Check Firebase credentials**
   ```bash
   # Verify file exists
   ls -la src/NotificationService.Api/config/firebase-service-account.json
   ```

2. **Check logs for errors**
   ```bash
   # Look for Firebase initialization message
   grep "Firebase Admin SDK initialized" logs/app.log
   ```

3. **Verify device tokens**
   ```bash
   GET /api/notifications/device-tokens
   ```

4. **Test with Firebase Console**
   - Go to Firebase Console → Cloud Messaging
   - Send a test message to a specific device token

### Common Errors

| Error | Solution |
|-------|----------|
| `Firebase credentials not found` | Check `FirebaseCredentialsPath` in appsettings.json |
| `InvalidArgument` | Device token is malformed or expired |
| `Unregistered` | User uninstalled app, token removed automatically |
| `SenderIdMismatch` | Token belongs to different Firebase project |

## Additional Resources

- [Firebase Admin SDK Documentation](https://firebase.google.com/docs/admin/setup)
- [FCM Architecture](https://firebase.google.com/docs/cloud-messaging/fcm-architecture)
- [Best Practices](https://firebase.google.com/docs/cloud-messaging/concept-options)
- [Message Payload](https://firebase.google.com/docs/reference/admin/dotnet/class/firebase-admin/messaging/message)

## Cost

**Firebase Cloud Messaging is 100% FREE** with unlimited notifications! 🎉

No usage limits, no quotas, completely free forever.
