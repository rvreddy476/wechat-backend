FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Media/Media.Api/Media.Api.csproj", "Media/Media.Api/"]
COPY ["Media/Media.Application/Media.Application.csproj", "Media/Media.Application/"]
COPY ["Media/Media.Domain/Media.Domain.csproj", "Media/Media.Domain/"]
COPY ["Media/Media.Infrastructure/Media.Infrastructure.csproj", "Media/Media.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "Media/Media.Api/Media.Api.csproj"
COPY ["Media/", "Media/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/Media/Media.Api"
RUN dotnet build "Media.Api.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Media.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Media.Api.dll"]
