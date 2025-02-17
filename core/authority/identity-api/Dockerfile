# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

ARG BUILD_ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT=${BUILD_ENVIRONMENT}

# SDK image for building and publishing
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files separately and restore dependencies
COPY Authority/Identity/Agience.Authority.Identity.csproj Authority/Identity/
COPY Authority/Models/Agience.Authority.Models.csproj Authority/Models/
COPY Core/Agience.Core.csproj Core/

RUN dotnet restore "Authority/Identity/Agience.Authority.Identity.csproj"

# Install EF tools globally
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy the rest of the source code for Identity and Core
COPY Authority/Identity/ Authority/Identity/
COPY Authority/Models/ Authority/Models/
COPY Core/ Core/

# Build and publish
RUN dotnet build "Authority/Identity/Agience.Authority.Identity.csproj" -c Release -o /app/build
RUN dotnet publish "Authority/Identity/Agience.Authority.Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - runtime
FROM base AS final
WORKDIR /app

# Copy the published output
COPY --from=build /app/publish .
COPY --from=build /src /src