# This Linux image supports CI/testing and the later hosted server role.
# It is not the runtime or installation path for the native Windows LMU-facing MVP.
# See agent/specs/architecture.md for the canonical deployment boundary.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["telemetry-tracker.csproj", "."]
RUN dotnet restore "./telemetry-tracker.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./telemetry-tracker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./telemetry-tracker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "telemetry-tracker.dll"]
