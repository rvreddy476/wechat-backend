FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Video/VideoService.Api/VideoService.Api.csproj", "Video/VideoService.Api/"]
COPY ["Video/VideoService.Application/VideoService.Application.csproj", "Video/VideoService.Application/"]
COPY ["Video/VideoService.Domain/VideoService.Domain.csproj", "Video/VideoService.Domain/"]
COPY ["Video/VideoService.Infrastructure/VideoService.Infrastructure.csproj", "Video/VideoService.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "Video/VideoService.Api/VideoService.Api.csproj"

COPY ["Video/", "Video/"]
COPY ["Shared/", "Shared/"]

WORKDIR "/src/Video/VideoService.Api"
RUN dotnet build "VideoService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoService.Api.dll"]
