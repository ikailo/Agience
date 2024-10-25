# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

ARG BUILD_ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT=${BUILD_ENVIRONMENT}

# SDK image for building and publishing
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files separately and restore to cache dependencies
COPY stack/Authority/Identity/Agience.Authority.Identity.csproj Identity/
COPY stack/Authority/Models/Agience.Authority.Models.csproj Models/
COPY src/Core/Agience.Core.csproj Core/

# Restore dependencies for the Identity project (including Core and Models)
RUN dotnet restore "Identity/Agience.Authority.Identity.csproj"

# Copy the rest of the source code for Identity and Core
COPY stack/Authority/Identity/ Identity/
COPY stack/Authority/Models/ Models/
COPY src/Core/ Core/

# Build the application
RUN dotnet build "Identity/Agience.Authority.Identity.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Identity/Agience.Authority.Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy SSL certificates
COPY stack/Authority/Build/certs/agience-net.pfx /app/https/agience-net.pfx
COPY stack/Authority/Build/certs/agience-net.crt /usr/local/share/ca-certificates/agience-net.crt

# Update CA certificates
RUN update-ca-certificates

# Set the entry point
ENTRYPOINT ["dotnet", "Agience.Authority.Identity.dll"]
