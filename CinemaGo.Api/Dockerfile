FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CinemaGo.Api/CinemaGo.Api.csproj", "CinemaGo.Api/"]
RUN dotnet restore "CinemaGo.Api/CinemaGo.Api.csproj"
COPY . .
WORKDIR "/src/CinemaGo.Api"
RUN dotnet build "CinemaGo.Api.csproj" -c Release -o /app/build
RUN dotnet publish "CinemaGo.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CinemaGo.Api.dll"]