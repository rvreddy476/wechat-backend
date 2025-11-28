FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Realtime/RealtimeService.Api/RealtimeService.Api.csproj", "Realtime/RealtimeService.Api/"]
COPY ["Realtime/RealtimeService.Application/RealtimeService.Application.csproj", "Realtime/RealtimeService.Application/"]
COPY ["Realtime/RealtimeService.Domain/RealtimeService.Domain.csproj", "Realtime/RealtimeService.Domain/"]
COPY ["Realtime/RealtimeService.Infrastructure/RealtimeService.Infrastructure.csproj", "Realtime/RealtimeService.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "Realtime/RealtimeService.Api/RealtimeService.Api.csproj"

COPY ["Realtime/", "Realtime/"]
COPY ["Shared/", "Shared/"]

WORKDIR "/src/Realtime/RealtimeService.Api"
RUN dotnet build "RealtimeService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealtimeService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealtimeService.Api.dll"]
