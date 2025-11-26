# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/PostFeedService.Api/PostFeedService.Api.csproj", "PostFeedService.Api/"]
COPY ["src/Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "PostFeedService.Api/PostFeedService.Api.csproj"

COPY src/PostFeedService.Api/. PostFeedService.Api/
COPY src/Shared/. Shared/

WORKDIR "/src/PostFeedService.Api"
RUN dotnet build "PostFeedService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PostFeedService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PostFeedService.Api.dll"]
