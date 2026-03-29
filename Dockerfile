# Use the official .NET 10 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["CinemaGo.Api/CinemaGo.Api.csproj", "CinemaGo.Api/"]
RUN dotnet restore "CinemaGo.Api/CinemaGo.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/CinemaGo.Api"
RUN dotnet build "CinemaGo.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CinemaGo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CinemaGo.Api.dll"]