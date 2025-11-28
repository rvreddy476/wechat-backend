# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["ChatService.Api/ChatService.Api.csproj", "ChatService.Api/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ChatService.Api/ChatService.Api.csproj"

# Copy everything else
COPY ["ChatService.Api/", "ChatService.Api/"]
COPY ["Shared/", "Shared/"]

# Build
WORKDIR "/src/ChatService.Api"
RUN dotnet build "ChatService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ChatService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatService.Api.dll"]
