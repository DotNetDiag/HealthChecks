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
COPY ./src/HealthChecks.UI/build ./src/HealthChecks.UI/build
COPY ./src/HealthChecks.UI/assets ./src/HealthChecks.UI/assets
COPY ./src/HealthChecks.UI/client ./src/HealthChecks.UI/client
COPY ./build/docker-images/HealthChecks.UI.Image/HealthChecks.UI.Image.csproj ./build/docker-images/HealthChecks.UI.Image/
COPY . .
WORKDIR /src/build/docker-images/HealthChecks.UI.Image
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build /src/src/HealthChecks.UI/HealthChecks.UI.csproj -c Release
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HealthChecks.UI.Image.dll"]
