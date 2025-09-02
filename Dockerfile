# Stage 1: Build the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy all files and restore dependencies
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port your app will run on
EXPOSE 10000

# Start the application
ENTRYPOINT ["dotnet", "forecastingGas.dll"]
