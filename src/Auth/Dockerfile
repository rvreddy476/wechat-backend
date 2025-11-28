# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Auth/AuthService.Api/AuthService.Api.csproj", "Auth/AuthService.Api/"]
COPY ["Auth/AuthService.Application/AuthService.Application.csproj", "Auth/AuthService.Application/"]
COPY ["Auth/AuthService.Domain/AuthService.Domain.csproj", "Auth/AuthService.Domain/"]
COPY ["Auth/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "Auth/AuthService.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Auth/AuthService.Api/AuthService.Api.csproj"

# Copy everything else
COPY ["Auth/", "Auth/"]
COPY ["Shared/", "Shared/"]

# Build
WORKDIR "/src/Auth/AuthService.Api"
RUN dotnet build "AuthService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
