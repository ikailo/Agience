# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

EXPOSE 5002

ARG BUILD_ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT=${BUILD_ENVIRONMENT} 

# SDK image for building and publishing
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files separately and restore to cache dependencies
COPY stack/Authority/Manage/Agience.Authority.Manage.csproj Authority/Manage/
COPY stack/Authority/Models/Agience.Authority.Models.csproj Authority/Models/
COPY src/Plugins/Core/Agience.Plugins.Core.csproj Plugins/Core/
COPY src/Core/Agience.Core.csproj Core/

RUN dotnet restore "Authority/Manage/Agience.Authority.Manage.csproj"

# Copy the rest of the source code
COPY stack/Authority/Manage/ Authority/Manage/
COPY stack/Authority/Models/ Authority/Models/
COPY src/Plugins/Core/ Plugins/Core/
COPY src/Core/ Core/

# Build the application
RUN dotnet build "Authority/Manage/Agience.Authority.Manage.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Authority/Manage/Agience.Authority.Manage.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

COPY stack/Authority/Build/certs/agience-net.pfx /app/https/agience-net.pfx
COPY stack/Authority/Build/certs/agience-net.crt /usr/local/share/ca-certificates/agience-net.crt

RUN update-ca-certificates

# Set the entry point
ENTRYPOINT ["dotnet", "Agience.Authority.Manage.dll"]
