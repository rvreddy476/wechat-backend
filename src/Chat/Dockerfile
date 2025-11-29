FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Chat/Chat.Api/Chat.Api.csproj", "Chat/Chat.Api/"]
COPY ["Chat/Chat.Application/Chat.Application.csproj", "Chat/Chat.Application/"]
COPY ["Chat/Chat.Domain/Chat.Domain.csproj", "Chat/Chat.Domain/"]
COPY ["Chat/Chat.Infrastructure/Chat.Infrastructure.csproj", "Chat/Chat.Infrastructure/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "Chat/Chat.Api/Chat.Api.csproj"
COPY ["Chat/", "Chat/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/Chat/Chat.Api"
RUN dotnet build "Chat.Api.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Chat.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Api.dll"]
