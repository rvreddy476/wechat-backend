# Required NuGet Packages for Email and SMS Integration

## 📦 Installation Instructions

### SendGrid (Email Service)

```bash
cd src/AuthService.Api
dotnet add package SendGrid
```

**Package Details:**
- Name: `SendGrid`
- Version: Latest stable (e.g., 9.28.1 or higher)
- Website: https://www.nuget.org/packages/SendGrid/

### Twilio (SMS Service)

```bash
cd src/AuthService.Api
dotnet add package Twilio
```

**Package Details:**
- Name: `Twilio`
- Version: Latest stable (e.g., 6.16.1 or higher)
- Website: https://www.nuget.org/packages/Twilio/

---

## 🔍 Verify Installation

After installing the packages, verify they're added to your `.csproj` file:

```bash
cat src/AuthService.Api/AuthService.Api.csproj
```

You should see:

```xml
<ItemGroup>
  <PackageReference Include="SendGrid" Version="9.28.1" />
  <PackageReference Include="Twilio" Version="6.16.1" />
  <!-- other packages... -->
</ItemGroup>
```

---

## 🚀 Quick Install (Both Packages)

Install both packages at once:

```bash
cd src/AuthService.Api
dotnet add package SendGrid
dotnet add package Twilio
dotnet restore
```

---

## ✅ Test the Installation

After installing, build the project to ensure everything is working:

```bash
dotnet build src/AuthService.Api/AuthService.Api.csproj
```

You should see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 📝 Alternative: Manual Installation via .csproj

You can also manually edit `src/AuthService.Api/AuthService.Api.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Email and SMS packages -->
    <PackageReference Include="SendGrid" Version="9.28.1" />
    <PackageReference Include="Twilio" Version="6.16.1" />

    <!-- Other existing packages -->
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Dapper" Version="2.1.24" />
    <PackageReference Include="Npgsql" Version="8.0.1" />
    <!-- ... -->
  </ItemGroup>
</Project>
```

Then run:
```bash
dotnet restore
```

---

## 🔒 Version Management

### Lock to Specific Versions (Recommended for Production)

To avoid unexpected breaking changes, lock to specific versions:

```bash
dotnet add package SendGrid --version 9.28.1
dotnet add package Twilio --version 6.16.1
```

### Use Latest Versions (Development)

To always use the latest stable version:

```bash
dotnet add package SendGrid
dotnet add package Twilio
```

---

## 🧪 Verify Services Are Working

After installation, start your application:

```bash
cd src/AuthService.Api
dotnet run
```

Check the logs:

**With packages installed and configured:**
```
[INF] SendGrid email service initialized successfully
[INF] Twilio SMS service initialized successfully
```

**Without configuration (but packages installed):**
```
[WRN] SendGrid is not configured. Email sending will fall back to console logging.
[WRN] Twilio is not configured. SMS sending will fall back to console logging.
```

This is expected in development mode before you add API keys.

---

## 📚 Package Documentation

### SendGrid
- Official Docs: https://github.com/sendgrid/sendgrid-csharp
- API Reference: https://docs.sendgrid.com/api-reference
- Examples: https://github.com/sendgrid/sendgrid-csharp/tree/main/examples

### Twilio
- Official Docs: https://github.com/twilio/twilio-csharp
- API Reference: https://www.twilio.com/docs/libraries/csharp
- Examples: https://www.twilio.com/docs/sms/quickstart/csharp-dotnet-core

---

## 🐛 Troubleshooting

### "Package 'SendGrid' could not be found"

**Solution:**
```bash
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet restore
```

### "The type or namespace 'SendGrid' could not be found"

**Check:**
1. Package is installed: `dotnet list package`
2. Build succeeded: `dotnet build`
3. Using correct namespace: `using SendGrid;`

### Compilation errors after package installation

**Solution:**
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## ✅ Checklist

- [ ] SendGrid package installed
- [ ] Twilio package installed
- [ ] Packages appear in .csproj file
- [ ] `dotnet restore` ran successfully
- [ ] `dotnet build` completed without errors
- [ ] Application starts without errors
- [ ] Services show in startup logs

---

**Next Step:** Follow [SENDGRID_TWILIO_SETUP.md](SENDGRID_TWILIO_SETUP.md) to configure API keys and test the integration.
