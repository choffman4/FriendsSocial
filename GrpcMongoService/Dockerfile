# Use the official .NET SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy the project files and restore as distinct layers
COPY ./ ./
RUN dotnet restore

# Publish the application
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Copy the published files from the build environment
COPY --from=build-env /app/out/ .

# Specify the entry point for your application
ENTRYPOINT ["dotnet", "GrpcMongoService.dll"]