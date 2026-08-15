# Multi-stage build for Quorum.Server (P6): the hosted Blazor WASM app — API + BFF + the
# compiled client. Build context is the REPOSITORY ROOT (fly.toml declares context = "..").
#
# Project files are copied and restored before the source, so a code change does not
# invalidate the restore layer. The runtime image major version equals the TFM major
# version (net10.0 -> aspnet:10.0); default roll-forward does not cross a major.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json .
COPY Server/Quorum.Server.csproj Server/
COPY Client/Quorum.Client.csproj Client/
COPY Shared/Quorum.Shared.csproj Shared/
COPY Quorum.Domain/Quorum.Domain.csproj Quorum.Domain/
COPY Quorum.Persistence/Quorum.Persistence.csproj Quorum.Persistence/
COPY Quorum.Persistence.Migrations.PostgreSQL/Quorum.Persistence.Migrations.PostgreSQL.csproj Quorum.Persistence.Migrations.PostgreSQL/
COPY Quorum.Persistence.Migrations.SqlServer/Quorum.Persistence.Migrations.SqlServer.csproj Quorum.Persistence.Migrations.SqlServer/
COPY Quorum.Service/Quorum.Service.csproj Quorum.Service/
COPY Quorum.Infrastructure/Quorum.Infrastructure.csproj Quorum.Infrastructure/
RUN dotnet restore Server/Quorum.Server.csproj

COPY . .
RUN dotnet publish Server/Quorum.Server.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
LABEL org.opencontainers.image.source=https://github.com/konradcinkusz/quorum

COPY --from=build /app/publish .

# The base image ships a non-root `app` user; 8080 needs no privileges.
USER $APP_UID

ENTRYPOINT ["dotnet", "Quorum.Server.dll"]
