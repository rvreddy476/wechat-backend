FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Notification/NotificationService.Api/NotificationService.Api.csproj", "Notification/NotificationService.Api/"]
COPY ["Notification/NotificationService.Application/NotificationService.Application.csproj", "Notification/NotificationService.Application/"]
COPY ["Notification/NotificationService.Domain/NotificationService.Domain.csproj", "Notification/NotificationService.Domain/"]
COPY ["Notification/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "Notification/NotificationService.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "Notification/NotificationService.Api/NotificationService.Api.csproj"

COPY ["Notification/", "Notification/"]
COPY ["Shared/", "Shared/"]

WORKDIR "/src/Notification/NotificationService.Api"
RUN dotnet build "NotificationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
