FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PostFeed/PostFeed.Api/PostFeed.Api.csproj", "PostFeed/PostFeed.Api/"]
COPY ["PostFeed/PostFeed.Application/PostFeed.Application.csproj", "PostFeed/PostFeed.Application/"]
COPY ["PostFeed/PostFeed.Domain/PostFeed.Domain.csproj", "PostFeed/PostFeed.Domain/"]
COPY ["PostFeed/PostFeed.Infrastructure/PostFeed.Infrastructure.csproj", "PostFeed/PostFeed.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "PostFeed/PostFeed.Api/PostFeed.Api.csproj"
COPY ["PostFeed/", "PostFeed/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/PostFeed/PostFeed.Api"
RUN dotnet build "PostFeed.Api.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "PostFeed.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PostFeed.Api.dll"]
