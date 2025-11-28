# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["PostFeedService.Api/PostFeedService.Api.csproj", "PostFeedService.Api/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "PostFeedService.Api/PostFeedService.Api.csproj"

# Copy everything else
COPY ["PostFeedService.Api/", "PostFeedService.Api/"]
COPY ["Shared/", "Shared/"]

# Build
WORKDIR "/src/PostFeedService.Api"
RUN dotnet build "PostFeedService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "PostFeedService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PostFeedService.Api.dll"]
