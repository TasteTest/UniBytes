# Stage 1: Build and Testc
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Build argument to force test execution (change this to invalidate cache)
ARG TEST_RUN_ID=1
ARG BUILD_DATE
ARG BUILD_VERSION

# Copy solution and project files
COPY UniBytes.sln .
COPY backend.csproj .
COPY Tests/Backend.Tests.csproj Tests/

# Restore dependencies for both projects
RUN dotnet restore UniBytes.sln

# Copy all source files
COPY . .

# Run tests - if any test fails, the Docker build will fail
# Using TEST_RUN_ID build arg to ensure tests always run (even with cache)
# To force test execution: docker compose build --build-arg TEST_RUN_ID=$(date +%s) backend
RUN echo "========================================" && \
    echo "Running tests..." && \
    echo "Test Run ID: ${TEST_RUN_ID}" && \
    echo "Build Date: ${BUILD_DATE}" && \
    echo "Build Version: ${BUILD_VERSION}" && \
    echo "========================================" && \
    dotnet test UniBytes.sln --no-restore --verbosity normal --logger "console;verbosity=detailed" && \
    echo "========================================" && \
    echo "✓ All tests passed successfully!" && \
    echo "========================================"

# Build and publish the application
RUN dotnet publish backend.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]

