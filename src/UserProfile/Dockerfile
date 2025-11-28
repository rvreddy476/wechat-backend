FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UserProfile/UserProfileService.Api/UserProfileService.Api.csproj", "UserProfile/UserProfileService.Api/"]
COPY ["UserProfile/UserProfileService.Application/UserProfileService.Application.csproj", "UserProfile/UserProfileService.Application/"]
COPY ["UserProfile/UserProfileService.Domain/UserProfileService.Domain.csproj", "UserProfile/UserProfileService.Domain/"]
COPY ["UserProfile/UserProfileService.Infrastructure/UserProfileService.Infrastructure.csproj", "UserProfile/UserProfileService.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]

RUN dotnet restore "UserProfile/UserProfileService.Api/UserProfileService.Api.csproj"

COPY ["UserProfile/", "UserProfile/"]
COPY ["Shared/", "Shared/"]

WORKDIR "/src/UserProfile/UserProfileService.Api"
RUN dotnet build "UserProfileService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserProfileService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProfileService.Api.dll"]
