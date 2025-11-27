# SendGrid and Twilio Integration Guide

This guide explains how to configure SendGrid (email) and Twilio (SMS) for the WeChat backend verification system.

---

## 📧 SendGrid Setup (Email)

### 1. Create SendGrid Account

1. Go to https://sendgrid.com/
2. Sign up for a free account (100 emails/day free)
3. Verify your email address

### 2. Get API Key

1. Log in to SendGrid Console
2. Go to **Settings** → **API Keys**
3. Click **Create API Key**
4. Name it "WeChat Backend"
5. Select **Full Access** (or **Mail Send** permission)
6. Click **Create & View**
7. **Copy the API key** (you won't see it again!)

Example API key format:
```
SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### 3. Verify Sender Identity

Before sending emails, you need to verify your sender email address:

**Option A: Single Sender Verification (Quick, for development)**
1. Go to **Settings** → **Sender Authentication**
2. Click **Verify a Single Sender**
3. Enter your email (e.g., `noreply@yourdomain.com`)
4. Fill in the form and submit
5. Check your email and click the verification link

**Option B: Domain Authentication (Recommended for production)**
1. Go to **Settings** → **Sender Authentication**
2. Click **Authenticate Your Domain**
3. Follow the DNS configuration steps for your domain
4. This allows you to send from any email @yourdomain.com

### 4. Configure in appsettings.json

Update your `appsettings.json`:

```json
{
  "SendGrid": {
    "ApiKey": "SG.your-actual-sendgrid-api-key-here",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "WeChat"
  },
  "AppUrl": "https://yourdomain.com"
}
```

### 5. Install NuGet Package

```bash
cd src/AuthService.Api
dotnet add package SendGrid
```

### 6. Test Email Sending

After configuration, restart your service. You should see:
```
[INF] SendGrid email service initialized successfully
```

If not configured:
```
[WRN] SendGrid is not configured. Email sending will fall back to console logging.
```

---

## 📱 Twilio Setup (SMS)

### 1. Create Twilio Account

1. Go to https://www.twilio.com/
2. Sign up for a free account (trial credits included)
3. Complete phone verification

### 2. Get Account Credentials

1. Log in to Twilio Console
2. Go to **Dashboard**
3. Find your credentials:
   - **Account SID**: AC... (starts with AC)
   - **Auth Token**: Click "Show" to reveal

Example credentials format:
```
Account SID: ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
Auth Token:  xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### 3. Get a Phone Number

**Trial Account:**
1. Go to **Phone Numbers** → **Manage** → **Active numbers**
2. You'll see your trial number (e.g., +1234567890)
3. Trial accounts can only send to **verified phone numbers**

**To verify a test phone number (Trial):**
1. Go to **Phone Numbers** → **Manage** → **Verified Caller IDs**
2. Click **Add a new number**
3. Enter the phone number and verify via SMS

**Paid Account:**
1. Go to **Phone Numbers** → **Buy a number**
2. Search for a number (starting at ~$1/month)
3. Purchase the number
4. No verification needed for recipients

### 4. Configure in appsettings.json

Update your `appsettings.json`:

```json
{
  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your-twilio-auth-token-here",
    "PhoneNumber": "+1234567890"
  }
}
```

### 5. Install NuGet Package

```bash
cd src/AuthService.Api
dotnet add package Twilio
```

### 6. Test SMS Sending

After configuration, restart your service. You should see:
```
[INF] Twilio SMS service initialized successfully
```

If not configured:
```
[WRN] Twilio is not configured. SMS sending will fall back to console logging.
```

---

## 🔐 Secure Configuration (Production)

### Option 1: Environment Variables

Instead of storing secrets in `appsettings.json`, use environment variables:

```bash
# Linux/Mac
export SendGrid__ApiKey="SG.your-key-here"
export Twilio__AccountSid="ACxxxxx"
export Twilio__AuthToken="your-token"
export Twilio__PhoneNumber="+1234567890"

# Windows
set SendGrid__ApiKey=SG.your-key-here
set Twilio__AccountSid=ACxxxxx
set Twilio__AuthToken=your-token
set Twilio__PhoneNumber=+1234567890
```

### Option 2: User Secrets (Development)

```bash
cd src/AuthService.Api

# Initialize user secrets
dotnet user-secrets init

# Add secrets
dotnet user-secrets set "SendGrid:ApiKey" "SG.your-key-here"
dotnet user-secrets set "Twilio:AccountSid" "ACxxxxx"
dotnet user-secrets set "Twilio:AuthToken" "your-token"
dotnet user-secrets set "Twilio:PhoneNumber" "+1234567890"
```

### Option 3: GCP Secret Manager (Production)

```bash
# Create secrets in GCP
gcloud secrets create sendgrid-api-key --data-file=- <<< "SG.your-key-here"
gcloud secrets create twilio-account-sid --data-file=- <<< "ACxxxxx"
gcloud secrets create twilio-auth-token --data-file=- <<< "your-token"

# Grant access to service account
gcloud secrets add-iam-policy-binding sendgrid-api-key \
  --member="serviceAccount:your-service-account@project.iam.gserviceaccount.com" \
  --role="roles/secretmanager.secretAccessor"
```

Then update your code to read from Secret Manager (see GCP documentation).

### Option 4: Azure Key Vault / AWS Secrets Manager

Similar process - store secrets in your cloud provider's secret management service.

---

## 📋 Complete appsettings.json Example

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=wechat_auth;Username=postgres;Password=your-password",
    "AuthDb": "Host=localhost;Database=wechat_auth;Username=postgres;Password=your-password"
  },

  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-min-32-characters-long",
    "Issuer": "WeChat",
    "Audience": "WeChat-Users",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },

  "Redis": {
    "ConnectionString": "localhost:6379"
  },

  "SendGrid": {
    "ApiKey": "SG.your-sendgrid-api-key-here",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "WeChat"
  },

  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your-twilio-auth-token-here",
    "PhoneNumber": "+1234567890"
  },

  "AppUrl": "https://yourdomain.com"
}
```

---

## 🧪 Testing the Integration

### Test Email Service

```bash
# Start your API
dotnet run --project src/AuthService.Api

# Register a new user (this triggers email and SMS)
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test@youremail.com",
    "phoneNumber": "+1234567890",
    "password": "SecurePass123!",
    "gender": "Male",
    "dateOfBirth": "1990-01-01"
  }'
```

### Check Logs

**With SendGrid configured:**
```
[INF] SendGrid email service initialized successfully
[INF] Email sent successfully via SendGrid to test@youremail.com. Subject: Verify Your WeChat Account
```

**Without SendGrid (console mode):**
```
[WRN] SendGrid is not configured. Email sending will fall back to console logging.
[INF] === EMAIL (CONSOLE MODE) ===
To: test@youremail.com
Verification Code: 123456
```

### Check Your Inbox

1. Check your email inbox for verification code
2. Check your phone for SMS (if Twilio is configured)
3. Both should arrive within seconds

---

## 💰 Pricing Reference

### SendGrid
- **Free**: 100 emails/day forever
- **Essentials**: $14.95/month for 40,000 emails
- **Pro**: $89.95/month for 100,000 emails

### Twilio
- **Trial**: Free credits (~$15-20)
- **SMS (US)**: $0.0075 per message
- **SMS (International)**: Varies by country (typically $0.01-0.10)
- **Phone Number**: ~$1/month

**Example Costs for 10,000 users/month:**
- Emails: 20,000 emails (registration + verification) = Free or $14.95/month
- SMS: 20,000 SMS = $150/month

---

## 🚨 Common Issues & Solutions

### SendGrid Issues

**"Unauthorized" error:**
- Check that your API key is correct
- Ensure API key has "Mail Send" permission
- Verify the key hasn't expired

**"Sender not verified" error:**
- Complete sender verification in SendGrid console
- Wait a few minutes after verification
- Use exact email address from verification

**Emails go to spam:**
- Complete domain authentication
- Add SPF and DKIM records
- Warm up your sending (start with small volumes)

### Twilio Issues

**"Unverified number" error (Trial):**
- You're on a trial account
- Add recipient phone number to Verified Caller IDs
- Or upgrade to paid account

**"Invalid phone number" error:**
- Use E.164 format: +[country code][number]
- Example: +12025551234 (not 202-555-1234)
- Include the + prefix

**"Account suspended" error:**
- Complete identity verification in Twilio console
- Provide business details
- May take 24-48 hours

**SMS not delivered:**
- Check phone number format
- Verify recipient can receive SMS
- Check Twilio console logs for details
- Some carriers block shortcode messages

---

## 📞 Support

### SendGrid Support
- Documentation: https://docs.sendgrid.com/
- Support: https://support.sendgrid.com/

### Twilio Support
- Documentation: https://www.twilio.com/docs/
- Console Logs: Check message logs in Twilio console
- Support: https://support.twilio.com/

---

## ✅ Checklist

### Development Setup
- [ ] SendGrid account created
- [ ] SendGrid API key obtained
- [ ] Sender email verified
- [ ] SendGrid NuGet package installed
- [ ] Twilio account created
- [ ] Twilio credentials obtained
- [ ] Twilio phone number acquired
- [ ] Test phone number verified (if trial)
- [ ] Twilio NuGet package installed
- [ ] Configuration added to appsettings.json
- [ ] Services tested with real user registration

### Production Setup
- [ ] SendGrid domain authenticated
- [ ] Twilio account upgraded (if needed)
- [ ] Secrets moved to secure storage (GCP Secret Manager, etc.)
- [ ] Monitoring set up for failed deliveries
- [ ] Error handling tested
- [ ] Rate limits configured
- [ ] Backup notification method considered

---

## 🎯 Next Steps

After completing the integration:

1. **Monitor Delivery Rates**
   - Check SendGrid/Twilio dashboards daily
   - Track bounce rates and failures
   - Adjust sender reputation

2. **Optimize Costs**
   - Monitor monthly usage
   - Consider bulk pricing
   - Implement smart retry logic

3. **Add Templates**
   - Create email templates in SendGrid
   - Use dynamic template IDs
   - A/B test email designs

4. **Enhance User Experience**
   - Add email preview text
   - Include unsubscribe links
   - Personalize content

---

**Your verification system is now production-ready! 🚀**
