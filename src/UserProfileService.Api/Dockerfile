# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/UserProfileService.Api/UserProfileService.Api.csproj", "UserProfileService.Api/"]
COPY ["src/Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "UserProfileService.Api/UserProfileService.Api.csproj"

COPY src/UserProfileService.Api/. UserProfileService.Api/
COPY src/Shared/. Shared/

WORKDIR "/src/UserProfileService.Api"
RUN dotnet build "UserProfileService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserProfileService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "UserProfileService.Api.dll"]
