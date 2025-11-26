# Use .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/VideoProcessing.Worker/VideoProcessing.Worker.csproj", "VideoProcessing.Worker/"]
COPY ["src/Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "VideoProcessing.Worker/VideoProcessing.Worker.csproj"

# Copy source code
COPY src/VideoProcessing.Worker/. VideoProcessing.Worker/
COPY src/Shared/. Shared/

# Build
WORKDIR "/src/VideoProcessing.Worker"
RUN dotnet build "VideoProcessing.Worker.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "VideoProcessing.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image with FFmpeg
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Verify FFmpeg installation
RUN ffmpeg -version && ffprobe -version

# Create directories
RUN mkdir -p /tmp/video-processing /app/logs /app/config

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD pgrep -f VideoProcessing.Worker || exit 1

ENTRYPOINT ["dotnet", "VideoProcessing.Worker.dll"]
