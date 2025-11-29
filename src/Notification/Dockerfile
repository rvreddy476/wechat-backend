FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Notification/Notification.Api/Notification.Api.csproj", "Notification/Notification.Api/"]
COPY ["Notification/Notification.Application/Notification.Application.csproj", "Notification/Notification.Application/"]
COPY ["Notification/Notification.Domain/Notification.Domain.csproj", "Notification/Notification.Domain/"]
COPY ["Notification/Notification.Infrastructure/Notification.Infrastructure.csproj", "Notification/Notification.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "Notification/Notification.Api/Notification.Api.csproj"
COPY ["Notification/", "Notification/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/Notification/Notification.Api"
RUN dotnet build "Notification.Api.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Notification.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Api.dll"]
