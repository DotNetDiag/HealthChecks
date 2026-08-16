# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app
EXPOSE 8080

FROM node:latest AS node

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS sdk-with-node

COPY --from=node /usr/local /usr/local
RUN ln -sf /usr/local/bin/node /usr/local/bin/nodejs

FROM sdk-with-node AS updated-npm
RUN npm i -g npm@latest

FROM updated-npm AS build
WORKDIR /src

COPY AspNetCore.Diagnostics.HealthChecks.sln ./
COPY Directory.Build.props ./
COPY Directory.Build.targets ./
COPY Directory.Packages.props ./
COPY Nuget.config ./
COPY ./src/Directory.Build.props ./src/
COPY ./src/HealthChecks.UI/package.json ./src/HealthChecks.UI/package-lock.json ./src/HealthChecks.UI/
COPY ./build/docker-images/HealthChecks.UI.Image/HealthChecks.UI.Image.csproj ./build/docker-images/HealthChecks.UI.Image/
COPY ./src/HealthChecks.UI/HealthChecks.UI.csproj ./src/HealthChecks.UI/
COPY ./src/HealthChecks.UI.Core/HealthChecks.UI.Core.csproj ./src/HealthChecks.UI.Core/
COPY ./src/HealthChecks.UI.Data/HealthChecks.UI.Data.csproj ./src/HealthChecks.UI.Data/
COPY ./src/HealthChecks.UI.InMemory.Storage/HealthChecks.UI.InMemory.Storage.csproj ./src/HealthChecks.UI.InMemory.Storage/
COPY ./src/HealthChecks.UI.MySql.Storage/HealthChecks.UI.MySql.Storage.csproj ./src/HealthChecks.UI.MySql.Storage/
COPY ./src/HealthChecks.UI.PostgreSQL.Storage/HealthChecks.UI.PostgreSQL.Storage.csproj ./src/HealthChecks.UI.PostgreSQL.Storage/
COPY ./src/HealthChecks.UI.SQLite.Storage/HealthChecks.UI.SQLite.Storage.csproj ./src/HealthChecks.UI.SQLite.Storage/
COPY ./src/HealthChecks.UI.SqlServer.Storage/HealthChecks.UI.SqlServer.Storage.csproj ./src/HealthChecks.UI.SqlServer.Storage/
WORKDIR /src/src/HealthChecks.UI
RUN --mount=type=cache,id=healthchecks-ui-npm,target=/root/.npm npm ci
WORKDIR /src
RUN --mount=type=cache,id=healthchecks-nuget,target=/root/.nuget/packages dotnet restore ./build/docker-images/HealthChecks.UI.Image/HealthChecks.UI.Image.csproj -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/src/HealthChecks.UI
RUN --mount=type=cache,id=healthchecks-ui-npm,target=/root/.npm npm run build
RUN npm run buildcss:prod
WORKDIR /src/build/docker-images/HealthChecks.UI.Image
RUN --mount=type=cache,id=healthchecks-nuget,target=/root/.nuget/packages dotnet build -c Release --no-restore /p:SkipNpmInstall=true /p:EnableSourceControlManagerQueries=false /p:EnableSourceLink=false

FROM build AS publish
RUN --mount=type=cache,id=healthchecks-nuget,target=/root/.nuget/packages dotnet publish -c Release -o /app/publish --no-build /p:UseAppHost=false /p:EnableSourceControlManagerQueries=false /p:EnableSourceLink=false

FROM base AS final
ARG IMAGE_SOURCE="https://github.com/DotNetDiag/HealthChecks"
ARG IMAGE_REVISION=""
ARG IMAGE_VERSION=""
LABEL org.opencontainers.image.source=$IMAGE_SOURCE \
      org.opencontainers.image.revision=$IMAGE_REVISION \
      org.opencontainers.image.version=$IMAGE_VERSION \
      org.opencontainers.image.licenses="Apache-2.0"
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "HealthChecks.UI.Image.dll"]
