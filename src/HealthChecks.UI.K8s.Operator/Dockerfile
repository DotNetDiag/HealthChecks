# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGET_FRAMEWORK=net10.0
WORKDIR /src
COPY Directory.Build.props .
COPY Directory.Build.targets .
COPY Directory.Packages.props .
COPY Nuget.config .
COPY src/Directory.Build.props src/
COPY ["src/HealthChecks.UI.K8s.Operator/HealthChecks.UI.K8s.Operator.csproj", "src/HealthChecks.UI.K8s.Operator/"]
RUN --mount=type=cache,id=healthchecks-nuget,target=/root/.nuget/packages dotnet restore "src/HealthChecks.UI.K8s.Operator/HealthChecks.UI.K8s.Operator.csproj"
COPY . .
WORKDIR "/src/src/HealthChecks.UI.K8s.Operator"
RUN --mount=type=cache,id=healthchecks-nuget,target=/root/.nuget/packages dotnet build "HealthChecks.UI.K8s.Operator.csproj" -c Release -f $TARGET_FRAMEWORK -o /app/build --no-restore /p:EnableSourceControlManagerQueries=false /p:EnableSourceLink=false

FROM build AS publish
ARG TARGET_FRAMEWORK=net10.0
RUN --mount=type=cache,id=healthchecks-nuget,target=/root/.nuget/packages dotnet publish "HealthChecks.UI.K8s.Operator.csproj" -c Release -f $TARGET_FRAMEWORK -o /app/publish --no-restore /p:UseAppHost=false /p:EnableSourceControlManagerQueries=false /p:EnableSourceLink=false

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
ENTRYPOINT ["dotnet", "HealthChecks.UI.K8s.Operator.dll"]
