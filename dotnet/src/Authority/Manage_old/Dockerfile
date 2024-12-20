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
COPY Authority/Manage/Agience.Authority.Manage.csproj Authority/Manage/
COPY Authority/Models/Agience.Authority.Models.csproj Authority/Models/
COPY Plugins/Core/Agience.Plugins.Core.csproj Plugins/Core/
COPY Core/Agience.Core.csproj Core/

RUN dotnet restore "Authority/Manage/Agience.Authority.Manage.csproj"

# Copy the rest of the source code
COPY Authority/Manage/ Authority/Manage/
COPY Authority/Models/ Authority/Models/
COPY Plugins/Core/ Plugins/Core/
COPY Core/ Core/

# Build the application
RUN dotnet build "Authority/Manage/Agience.Authority.Manage.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Authority/Manage/Agience.Authority.Manage.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "Agience.Authority.Manage.dll"]
