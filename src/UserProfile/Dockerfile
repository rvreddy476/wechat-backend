FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserProfile/UserProfile.Api/UserProfile.Api.csproj", "UserProfile/UserProfile.Api/"]
COPY ["UserProfile/UserProfile.Application/UserProfile.Application.csproj", "UserProfile/UserProfile.Application/"]
COPY ["UserProfile/UserProfile.Domain/UserProfile.Domain.csproj", "UserProfile/UserProfile.Domain/"]
COPY ["UserProfile/UserProfile.Infrastructure/UserProfile.Infrastructure.csproj", "UserProfile/UserProfile.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "UserProfile/UserProfile.Api/UserProfile.Api.csproj"
COPY ["UserProfile/", "UserProfile/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/UserProfile/UserProfile.Api"
RUN dotnet build "UserProfile.Api.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "UserProfile.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProfile.Api.dll"]
