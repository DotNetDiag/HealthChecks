# Health Checks Roadmap

This roadmap tracks health check packages that core maintainers are actively considering, plus separate community-owned candidates that may fill current ecosystem gaps in .NET applications.

## Status Legend

- ✅ Done
- 🚧 Next
- 📝 Planned
- 🔎 Research
- 🤝 Community-owned

## Priority Guidelines

- Prefer widely deployed infrastructure that .NET services commonly depend on directly.
- Prefer vendor-neutral or self-hostable infrastructure over cloud-provider catalog coverage.
- Prefer checks with clear, stable health semantics or SDK support.
- Prefer candidates that extend an existing provider family in this repository.
- Do not treat AWS, Azure, or Google Cloud service coverage as core roadmap debt. Cloud-provider packages need a committed owner, sponsor, or community contribution because those services move quickly and otherwise drift behind.
- Keep more specialized analytics, AI/vector, cloud, and proprietary SaaS work outside the active roadmap until there is a clear owner.

## Completed

1. ✅ `HealthChecks.OpenSearch`
   - OpenSearch cluster connectivity, ping, and cluster health checks.

2. ✅ `HealthChecks.Gcp.CloudStorage`
   - Google Cloud Storage bucket/service reachability.

3. ✅ `HealthChecks.Minio`
   - MinIO live, ready, cluster, and cluster read endpoint checks, plus bucket and service reachability through the S3-compatible API.

4. ✅ `HealthChecks.Harbor`
   - Harbor API health endpoint checks, including overall status, component status validation, and optional required component enforcement.

5. ✅ `HealthChecks.SonnetDB`
   - SonnetDB `/healthz` status checks, readiness metadata capture, optional authenticated probes, optional Copilot readiness enforcement, and HealthChecks UI storage support.

6. ✅ `HealthChecks.Vault`
   - HashiCorp Vault `/sys/health` checks, including sealed, standby, initialized, active, performance standby, disaster recovery secondary, HA unhealthy, and removed-from-cluster states.

7. ✅ `HealthChecks.ContainerRegistry`
   - Generic OCI/Docker Registry HTTP API v2 endpoint checks, including successful registry reachability and authenticated registry challenge validation.

8. ✅ `HealthChecks.ZooKeeper`
   - Apache ZooKeeper connectivity checks, configurable session timeout, and required znode existence validation.

9. ✅ `HealthChecks.Etcd`
   - etcd v3 status endpoint checks, configurable client settings, authenticated client options, and direct `EtcdClient` registration support.

10. ✅ `HealthChecks.Grafana`
    - Grafana `/api/health` endpoint checks, including database status validation and version/commit metadata capture.

11. ✅ `HealthChecks.Neo4j`
    - Neo4j driver connectivity checks, optional database query checks, DI registration, package docs, CI metadata, API approval, and Testcontainers-backed functional coverage.

12. ✅ `HealthChecks.Firebird`
    - Firebird connection and lightweight query health checks, DI registration, package docs, CI metadata, API approval, and Testcontainers-backed functional coverage.

13. ✅ `HealthChecks.IbmDb2`
    - IBM Db2 connection and lightweight query health checks, DI registration, package docs, CI metadata, API approval, and Testcontainers-backed functional coverage.

14. ✅ `HealthChecks.CockroachDb`
    - CockroachDB SQL connectivity and node health endpoint checks, DI registration, package docs, CI metadata, API approval, and Testcontainers-backed functional coverage.

15. ✅ `HealthChecks.Valkey`
    - Valkey connectivity and lightweight command checks, Redis-compatible StackExchange.Redis integration, DI registration, package docs, CI metadata, API approval, and Testcontainers-backed functional coverage.

16. ✅ `HealthChecks.DuckDb`
    - DuckDB embedded database connection and lightweight query health checks, DI registration, package docs, CI metadata, API approval, and in-memory functional coverage.

17. ✅ `HealthChecks.Memcached`
    - Memcached cache operation checks, Enyim client integration, DI registration, package docs, CI metadata, API approval, conformance coverage, and Testcontainers-backed functional coverage.

18. ✅ `HealthChecks.ActiveMQ` / `HealthChecks.Artemis`
    - ActiveMQ Classic and ActiveMQ Artemis broker connectivity checks using Apache.NMS, DI registration, package docs, CI metadata, API approval, and Testcontainers-backed functional coverage.

19. ✅ `HealthChecks.Apache.Pulsar`
    - Apache Pulsar broker publishing checks and admin health endpoint checks, DI registration, package docs, CI metadata, API approval, and Testcontainers-backed functional coverage.

## Priority Roadmap

## Research Backlog

10. 🔎 `HealthChecks.SapHana`
11. 🔎 `HealthChecks.Weaviate`
12. 🔎 `HealthChecks.Chroma`
13. 🔎 `HealthChecks.Trino`
14. 🔎 `HealthChecks.Apache.Druid`
15. 🔎 `HealthChecks.Apache.Pinot`
16. 🔎 `HealthChecks.OpenTelemetryCollector`
17. 🔎 `HealthChecks.Loki`
18. 🔎 `HealthChecks.Tempo`
19. 🔎 `HealthChecks.Jaeger`
20. 🔎 `HealthChecks.Alertmanager`
21. 🔎 `HealthChecks.Keycloak`

## Community-Owned Cloud And SaaS Candidates

These are possible contributions, not core maintainer roadmap commitments.

22. 🤝 `HealthChecks.Aws.Kinesis`
23. 🤝 `HealthChecks.Aws.EventBridge`
24. 🤝 `HealthChecks.Aws.Redshift`
25. 🤝 `HealthChecks.Azure.Synapse`
26. 🤝 `HealthChecks.Azure.AppConfiguration`
27. 🤝 `HealthChecks.Gcp.PubSub`
28. 🤝 `HealthChecks.Gcp.BigQuery`
29. 🤝 `HealthChecks.Gcp.Spanner`
30. 🤝 `HealthChecks.Gcp.SecretManager`
31. 🤝 `HealthChecks.Microsoft.Fabric`
32. 🤝 `HealthChecks.Databricks`
33. 🤝 `HealthChecks.Snowflake`
34. 🤝 `HealthChecks.Pinecone`
35. 🤝 `HealthChecks.Auth0`
36. 🤝 `HealthChecks.Okta`
37. 🤝 `HealthChecks.LaunchDarkly`

## Notes

- MassTransit and Quartz.NET are not prioritized because their ASP.NET Core integrations already expose health checks.
- Packages should follow the existing `DotNetDiag.HealthChecks.<Provider>` naming convention.
- Cloud-provider and proprietary SaaS candidates may be accepted when a contributor commits to the implementation, tests, documentation, and ongoing maintenance surface expected for new packages.
- New .NET or MSBuild changes must pass the repository format gate before being considered complete.
