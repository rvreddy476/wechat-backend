FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Media/MediaService.Api/MediaService.Api.csproj", "Media/MediaService.Api/"]
COPY ["Media/MediaService.Application/MediaService.Application.csproj", "Media/MediaService.Application/"]
COPY ["Media/MediaService.Domain/MediaService.Domain.csproj", "Media/MediaService.Domain/"]
COPY ["Media/MediaService.Infrastructure/MediaService.Infrastructure.csproj", "Media/MediaService.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "Media/MediaService.Api/MediaService.Api.csproj"

COPY ["Media/", "Media/"]
COPY ["Shared/", "Shared/"]

WORKDIR "/src/Media/MediaService.Api"
RUN dotnet build "MediaService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediaService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediaService.Api.dll"]
