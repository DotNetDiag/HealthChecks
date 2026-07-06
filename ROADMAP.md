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

## Priority Roadmap

1. 📝 `HealthChecks.Etcd`
   - Summary: etcd endpoint status, leader, and cluster health checks.
   - Why it matters: etcd backs Kubernetes and many distributed systems. When it is degraded, service discovery, configuration, and orchestration can fail even when application pods are still running.

2. 📝 `HealthChecks.ZooKeeper`
   - Summary: ZooKeeper server connectivity, ruok/stat-style health checks, and quorum-aware validation where available.
   - Why it matters: ZooKeeper is still widely used by messaging, coordination, and legacy distributed platforms. A health check covers a real operational dependency that many .NET services inherit indirectly.

3. 📝 `HealthChecks.ContainerRegistry`
   - Summary: Generic OCI/Docker Registry HTTP API checks for registry reachability and authentication readiness.
   - Why it matters: Not every team uses Harbor or a cloud registry. A generic registry check covers self-hosted and vendor-neutral image registries that are essential to deployment and recovery workflows.

4. 📝 `HealthChecks.Neo4j`
   - Summary: Neo4j driver connectivity and database health checks.
   - Why it matters: Neo4j is a mature graph database with clear application dependencies. Promoting it keeps the roadmap grounded in recognizable data stores rather than newer niche vector-only services.

5. 📝 `HealthChecks.Memcached`
   - Summary: Memcached connectivity and lightweight cache operation checks.
   - Why it matters: Memcached remains a simple, common distributed cache. Applications often rely on it for latency and load shedding, so explicit health reporting is more useful than treating cache failures as incidental.

6. 📝 `HealthChecks.Valkey`
   - Summary: Valkey connectivity and lightweight command checks.
   - Why it matters: Valkey is an open Redis-compatible datastore with growing adoption. Supporting it gives users a path that matches newer open-source infrastructure choices while staying close to the existing Redis package family.

7. 📝 `HealthChecks.CockroachDb`
    - Summary: CockroachDB SQL connectivity and node health endpoint checks.
    - Why it matters: CockroachDB is a distributed SQL database used where PostgreSQL-like access and horizontal resilience matter. A package can reuse familiar SQL health-check patterns while exposing distributed-node readiness.

8. 📝 `HealthChecks.ActiveMQ` / `HealthChecks.Artemis`
    - Summary: ActiveMQ Classic or Artemis broker connectivity and management health checks.
    - Why it matters: ActiveMQ and Artemis are established message brokers in enterprise systems. They fit the existing messaging package family and cover teams that are not on Kafka, RabbitMQ, NATS, or cloud queues.

9. 📝 `HealthChecks.Apache.Pulsar`
    - Summary: Pulsar broker and admin API health checks.
    - Why it matters: Pulsar is a durable messaging and streaming platform with clear broker/admin health surfaces. It fills a gap for distributed messaging users without making cloud services a roadmap dependency.

10. 📝 `HealthChecks.IbmDb2`
    - Summary: IBM Db2 connection and lightweight query health checks.
    - Why it matters: Db2 remains important in enterprise and regulated environments. A package gives those users the same first-class database health-check story as PostgreSQL, SQL Server, MySQL, and Oracle users.

11. 📝 `HealthChecks.Firebird`
    - Summary: Firebird connection and lightweight query health checks.
    - Why it matters: Firebird is a long-lived embedded and server database used in packaged and vertical applications. Supporting it broadens the database coverage without tying the roadmap to a cloud vendor.

## Research Backlog

12. 🔎 `HealthChecks.SapHana`
13. 🔎 `HealthChecks.DuckDb`
14. 🔎 `HealthChecks.Weaviate`
15. 🔎 `HealthChecks.Chroma`
16. 🔎 `HealthChecks.Trino`
17. 🔎 `HealthChecks.Apache.Druid`
18. 🔎 `HealthChecks.Apache.Pinot`
19. 🔎 `HealthChecks.OpenTelemetryCollector`
20. 🔎 `HealthChecks.Grafana`
21. 🔎 `HealthChecks.Loki`
22. 🔎 `HealthChecks.Tempo`
23. 🔎 `HealthChecks.Jaeger`
24. 🔎 `HealthChecks.Alertmanager`
25. 🔎 `HealthChecks.Keycloak`

## Community-Owned Cloud And SaaS Candidates

These are possible contributions, not core maintainer roadmap commitments.

26. 🤝 `HealthChecks.Aws.Kinesis`
27. 🤝 `HealthChecks.Aws.EventBridge`
28. 🤝 `HealthChecks.Aws.Redshift`
29. 🤝 `HealthChecks.Azure.Synapse`
30. 🤝 `HealthChecks.Azure.AppConfiguration`
31. 🤝 `HealthChecks.Gcp.PubSub`
32. 🤝 `HealthChecks.Gcp.BigQuery`
33. 🤝 `HealthChecks.Gcp.Spanner`
34. 🤝 `HealthChecks.Gcp.SecretManager`
35. 🤝 `HealthChecks.Microsoft.Fabric`
36. 🤝 `HealthChecks.Databricks`
37. 🤝 `HealthChecks.Snowflake`
38. 🤝 `HealthChecks.Pinecone`
39. 🤝 `HealthChecks.Auth0`
40. 🤝 `HealthChecks.Okta`
41. 🤝 `HealthChecks.LaunchDarkly`

## Notes

- MassTransit and Quartz.NET are not prioritized because their ASP.NET Core integrations already expose health checks.
- Packages should follow the existing `DotNetDiag.HealthChecks.<Provider>` naming convention.
- Cloud-provider and proprietary SaaS candidates may be accepted when a contributor commits to the implementation, tests, documentation, and ongoing maintenance surface expected for new packages.
- New .NET or MSBuild changes must pass the repository format gate before being considered complete.
