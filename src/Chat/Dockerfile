# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for Chat service
COPY ["Chat/ChatService.Api/ChatService.Api.csproj", "Chat/ChatService.Api/"]
COPY ["Chat/ChatService.Application/ChatService.Application.csproj", "Chat/ChatService.Application/"]
COPY ["Chat/ChatService.Domain/ChatService.Domain.csproj", "Chat/ChatService.Domain/"]
COPY ["Chat/ChatService.Infrastructure/ChatService.Infrastructure.csproj", "Chat/ChatService.Infrastructure/"]

# Copy Shared projects
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Chat/ChatService.Api/ChatService.Api.csproj"

# Copy everything else
COPY ["Chat/", "Chat/"]
COPY ["Shared/", "Shared/"]

# Build
WORKDIR "/src/Chat/ChatService.Api"
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
