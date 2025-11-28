FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PostFeed/PostFeedService.Api/PostFeedService.Api.csproj", "PostFeed/PostFeedService.Api/"]
COPY ["PostFeed/PostFeedService.Application/PostFeedService.Application.csproj", "PostFeed/PostFeedService.Application/"]
COPY ["PostFeed/PostFeedService.Domain/PostFeedService.Domain.csproj", "PostFeed/PostFeedService.Domain/"]
COPY ["PostFeed/PostFeedService.Infrastructure/PostFeedService.Infrastructure.csproj", "PostFeed/PostFeedService.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "PostFeed/PostFeedService.Api/PostFeedService.Api.csproj"

COPY ["PostFeed/", "PostFeed/"]
COPY ["Shared/", "Shared/"]

WORKDIR "/src/PostFeed/PostFeedService.Api"
RUN dotnet build "PostFeedService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PostFeedService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PostFeedService.Api.dll"]
