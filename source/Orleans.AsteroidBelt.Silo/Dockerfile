#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Orleans.AsteroidBelt.Silo/Orleans.AsteroidBelt.Silo.csproj", "Orleans.AsteroidBelt.Silo/"]
RUN dotnet restore "Orleans.AsteroidBelt.Silo/Orleans.AsteroidBelt.Silo.csproj"
COPY . .
WORKDIR "/src/Orleans.AsteroidBelt.Silo"
RUN dotnet build "Orleans.AsteroidBelt.Silo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orleans.AsteroidBelt.Silo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orleans.AsteroidBelt.Silo.dll"]