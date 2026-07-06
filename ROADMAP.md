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
   - SonnetDB `/healthz` status checks, readiness metadata capture, optional authenticated probes, and optional Copilot readiness enforcement.

6. ✅ `HealthChecks.Vault`
   - HashiCorp Vault `/sys/health` checks, including sealed, standby, initialized, active, performance standby, disaster recovery secondary, HA unhealthy, and removed-from-cluster states.

7. ✅ `HealthChecks.ContainerRegistry`
   - Generic OCI/Docker Registry HTTP API v2 endpoint checks, including successful registry reachability and authenticated registry challenge validation.

8. ✅ `HealthChecks.ZooKeeper`
   - Apache ZooKeeper connectivity checks, configurable session timeout, and required znode existence validation.

9. ✅ `HealthChecks.Etcd`
   - etcd v3 status endpoint checks, configurable client settings, authenticated client options, and direct `EtcdClient` registration support.

## Priority Roadmap

1. 📝 `HealthChecks.Neo4j`
   - Summary: Neo4j driver connectivity and database health checks.
   - Why it matters: Neo4j is a mature graph database with clear application dependencies. Promoting it keeps the roadmap grounded in recognizable data stores rather than newer niche vector-only services.

2. 📝 `HealthChecks.Memcached`
   - Summary: Memcached connectivity and lightweight cache operation checks.
   - Why it matters: Memcached remains a simple, common distributed cache. Applications often rely on it for latency and load shedding, so explicit health reporting is more useful than treating cache failures as incidental.

3. 📝 `HealthChecks.Valkey`
   - Summary: Valkey connectivity and lightweight command checks.
   - Why it matters: Valkey is an open Redis-compatible datastore with growing adoption. Supporting it gives users a path that matches newer open-source infrastructure choices while staying close to the existing Redis package family.

4. 📝 `HealthChecks.CockroachDb`
    - Summary: CockroachDB SQL connectivity and node health endpoint checks.
    - Why it matters: CockroachDB is a distributed SQL database used where PostgreSQL-like access and horizontal resilience matter. A package can reuse familiar SQL health-check patterns while exposing distributed-node readiness.

5. 📝 `HealthChecks.ActiveMQ` / `HealthChecks.Artemis`
    - Summary: ActiveMQ Classic or Artemis broker connectivity and management health checks.
    - Why it matters: ActiveMQ and Artemis are established message brokers in enterprise systems. They fit the existing messaging package family and cover teams that are not on Kafka, RabbitMQ, NATS, or cloud queues.

6. 📝 `HealthChecks.Apache.Pulsar`
    - Summary: Pulsar broker and admin API health checks.
    - Why it matters: Pulsar is a durable messaging and streaming platform with clear broker/admin health surfaces. It fills a gap for distributed messaging users without making cloud services a roadmap dependency.

7. 📝 `HealthChecks.IbmDb2`
    - Summary: IBM Db2 connection and lightweight query health checks.
    - Why it matters: Db2 remains important in enterprise and regulated environments. A package gives those users the same first-class database health-check story as PostgreSQL, SQL Server, MySQL, and Oracle users.

8. 📝 `HealthChecks.Firebird`
    - Summary: Firebird connection and lightweight query health checks.
    - Why it matters: Firebird is a long-lived embedded and server database used in packaged and vertical applications. Supporting it broadens the database coverage without tying the roadmap to a cloud vendor.

## Research Backlog

10. 🔎 `HealthChecks.SapHana`
11. 🔎 `HealthChecks.DuckDb`
12. 🔎 `HealthChecks.Weaviate`
13. 🔎 `HealthChecks.Chroma`
14. 🔎 `HealthChecks.Trino`
15. 🔎 `HealthChecks.Apache.Druid`
16. 🔎 `HealthChecks.Apache.Pinot`
17. 🔎 `HealthChecks.OpenTelemetryCollector`
18. 🔎 `HealthChecks.Grafana`
19. 🔎 `HealthChecks.Loki`
20. 🔎 `HealthChecks.Tempo`
21. 🔎 `HealthChecks.Jaeger`
22. 🔎 `HealthChecks.Alertmanager`
23. 🔎 `HealthChecks.Keycloak`

## Community-Owned Cloud And SaaS Candidates

These are possible contributions, not core maintainer roadmap commitments.

24. 🤝 `HealthChecks.Aws.Kinesis`
25. 🤝 `HealthChecks.Aws.EventBridge`
26. 🤝 `HealthChecks.Aws.Redshift`
27. 🤝 `HealthChecks.Azure.Synapse`
28. 🤝 `HealthChecks.Azure.AppConfiguration`
29. 🤝 `HealthChecks.Gcp.PubSub`
30. 🤝 `HealthChecks.Gcp.BigQuery`
31. 🤝 `HealthChecks.Gcp.Spanner`
32. 🤝 `HealthChecks.Gcp.SecretManager`
33. 🤝 `HealthChecks.Microsoft.Fabric`
34. 🤝 `HealthChecks.Databricks`
35. 🤝 `HealthChecks.Snowflake`
36. 🤝 `HealthChecks.Pinecone`
37. 🤝 `HealthChecks.Auth0`
38. 🤝 `HealthChecks.Okta`
39. 🤝 `HealthChecks.LaunchDarkly`

## Notes

- MassTransit and Quartz.NET are not prioritized because their ASP.NET Core integrations already expose health checks.
- Packages should follow the existing `DotNetDiag.HealthChecks.<Provider>` naming convention.
- Cloud-provider and proprietary SaaS candidates may be accepted when a contributor commits to the implementation, tests, documentation, and ongoing maintenance surface expected for new packages.
- New .NET or MSBuild changes must pass the repository format gate before being considered complete.
