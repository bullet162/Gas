# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/out

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render sets $PORT automatically
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 5000

ENTRYPOINT ["dotnet", "forecastingGas.dll"]
