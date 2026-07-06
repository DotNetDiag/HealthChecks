FROM mcr.microsoft.com/dotnet/runtime:10.0-noble AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGET_FRAMEWORK=net10.0
WORKDIR /src
COPY Directory.Build.props .
COPY Directory.Build.targets .
COPY Directory.Packages.props .
COPY src/Directory.Build.props src/
COPY ["src/HealthChecks.UI.K8s.Operator/HealthChecks.UI.K8s.Operator.csproj", "src/HealthChecks.UI.K8s.Operator/"]
RUN dotnet restore "src/HealthChecks.UI.K8s.Operator/HealthChecks.UI.K8s.Operator.csproj"
COPY . .
WORKDIR "/src/src/HealthChecks.UI.K8s.Operator"
RUN dotnet build "HealthChecks.UI.K8s.Operator.csproj" -c Release -f $TARGET_FRAMEWORK -o /app/build --no-restore

FROM build AS publish
ARG TARGET_FRAMEWORK=net10.0
RUN dotnet publish "HealthChecks.UI.K8s.Operator.csproj" -c Release -f $TARGET_FRAMEWORK -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthChecks.UI.K8s.Operator.dll"]
