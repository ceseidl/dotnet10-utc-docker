
# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ./src/ApiUtc/ ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Etapa de runtime (Ubuntu 24.04, .NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Configura UTC e instala curl para healthcheck
RUN apt-get update &&     apt-get install -y --no-install-recommends tzdata curl &&     ln -fs /usr/share/zoneinfo/Etc/UTC /etc/localtime &&     dpkg-reconfigure -f noninteractive tzdata &&     rm -rf /var/lib/apt/lists/*

ENV TZ=Etc/UTC
COPY --from=build /app ./

# ASP.NET Core (containers oficiais) escuta 8080 por padrão
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3   CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ApiUtc.dll"]
