# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["UserProfileService.Api/UserProfileService.Api.csproj", "UserProfileService.Api/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "UserProfileService.Api/UserProfileService.Api.csproj"

# Copy everything else
COPY ["UserProfileService.Api/", "UserProfileService.Api/"]
COPY ["Shared/", "Shared/"]

# Build
WORKDIR "/src/UserProfileService.Api"
RUN dotnet build "UserProfileService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "UserProfileService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProfileService.Api.dll"]
