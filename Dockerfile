# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY FirePorker/FirePorker.csproj FirePorker/
RUN dotnet restore FirePorker/FirePorker.csproj

# Copy everything else and build
COPY FirePorker/ FirePorker/
WORKDIR /src/FirePorker
RUN dotnet build FirePorker.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish FirePorker.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FirePorker.dll"]
