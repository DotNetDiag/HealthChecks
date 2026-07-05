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

## Priority Roadmap

1. 📝 `HealthChecks.Vault`
   - Summary: HashiCorp Vault `/sys/health` checks, including sealed, standby, initialized, and active states.
   - Why it matters: Vault is often on the critical path for secrets, certificates, and service credentials. A health check helps applications fail fast when secret infrastructure is unavailable or intentionally sealed.

2. 📝 `HealthChecks.Etcd`
   - Summary: etcd endpoint status, leader, and cluster health checks.
   - Why it matters: etcd backs Kubernetes and many distributed systems. When it is degraded, service discovery, configuration, and orchestration can fail even when application pods are still running.

3. 📝 `HealthChecks.ZooKeeper`
   - Summary: ZooKeeper server connectivity, ruok/stat-style health checks, and quorum-aware validation where available.
   - Why it matters: ZooKeeper is still widely used by messaging, coordination, and legacy distributed platforms. A health check covers a real operational dependency that many .NET services inherit indirectly.

4. 📝 `HealthChecks.Harbor`
   - Summary: Harbor API and registry health checks for core services such as registry, database, job service, and portal status.
   - Why it matters: Harbor is a common self-hosted container registry in enterprise Kubernetes environments. Checking it directly helps detect image distribution and deployment pipeline failures before rollouts stall.

5. 📝 `HealthChecks.ContainerRegistry`
   - Summary: Generic OCI/Docker Registry HTTP API checks for registry reachability and authentication readiness.
   - Why it matters: Not every team uses Harbor or a cloud registry. A generic registry check covers self-hosted and vendor-neutral image registries that are essential to deployment and recovery workflows.

6. 📝 `HealthChecks.Neo4j`
   - Summary: Neo4j driver connectivity and database health checks.
   - Why it matters: Neo4j is a mature graph database with clear application dependencies. Promoting it keeps the roadmap grounded in recognizable data stores rather than newer niche vector-only services.

7. 📝 `HealthChecks.Memcached`
   - Summary: Memcached connectivity and lightweight cache operation checks.
   - Why it matters: Memcached remains a simple, common distributed cache. Applications often rely on it for latency and load shedding, so explicit health reporting is more useful than treating cache failures as incidental.

8. 📝 `HealthChecks.Valkey`
   - Summary: Valkey connectivity and lightweight command checks.
   - Why it matters: Valkey is an open Redis-compatible datastore with growing adoption. Supporting it gives users a path that matches newer open-source infrastructure choices while staying close to the existing Redis package family.

9. 📝 `HealthChecks.CockroachDb`
    - Summary: CockroachDB SQL connectivity and node health endpoint checks.
    - Why it matters: CockroachDB is a distributed SQL database used where PostgreSQL-like access and horizontal resilience matter. A package can reuse familiar SQL health-check patterns while exposing distributed-node readiness.

10. 📝 `HealthChecks.ActiveMQ` / `HealthChecks.Artemis`
    - Summary: ActiveMQ Classic or Artemis broker connectivity and management health checks.
    - Why it matters: ActiveMQ and Artemis are established message brokers in enterprise systems. They fit the existing messaging package family and cover teams that are not on Kafka, RabbitMQ, NATS, or cloud queues.

11. 📝 `HealthChecks.Apache.Pulsar`
    - Summary: Pulsar broker and admin API health checks.
    - Why it matters: Pulsar is a durable messaging and streaming platform with clear broker/admin health surfaces. It fills a gap for distributed messaging users without making cloud services a roadmap dependency.

12. 📝 `HealthChecks.IbmDb2`
    - Summary: IBM Db2 connection and lightweight query health checks.
    - Why it matters: Db2 remains important in enterprise and regulated environments. A package gives those users the same first-class database health-check story as PostgreSQL, SQL Server, MySQL, and Oracle users.

13. 📝 `HealthChecks.Firebird`
    - Summary: Firebird connection and lightweight query health checks.
    - Why it matters: Firebird is a long-lived embedded and server database used in packaged and vertical applications. Supporting it broadens the database coverage without tying the roadmap to a cloud vendor.

## Research Backlog

14. 🔎 `HealthChecks.SapHana`
15. 🔎 `HealthChecks.DuckDb`
16. 🔎 `HealthChecks.Weaviate`
17. 🔎 `HealthChecks.Chroma`
18. 🔎 `HealthChecks.Trino`
19. 🔎 `HealthChecks.Apache.Druid`
20. 🔎 `HealthChecks.Apache.Pinot`
21. 🔎 `HealthChecks.OpenTelemetryCollector`
22. 🔎 `HealthChecks.Grafana`
23. 🔎 `HealthChecks.Loki`
24. 🔎 `HealthChecks.Tempo`
25. 🔎 `HealthChecks.Jaeger`
26. 🔎 `HealthChecks.Alertmanager`
27. 🔎 `HealthChecks.Keycloak`

## Community-Owned Cloud And SaaS Candidates

These are possible contributions, not core maintainer roadmap commitments.

28. 🤝 `HealthChecks.Aws.Kinesis`
29. 🤝 `HealthChecks.Aws.EventBridge`
30. 🤝 `HealthChecks.Aws.Redshift`
31. 🤝 `HealthChecks.Azure.Synapse`
32. 🤝 `HealthChecks.Azure.AppConfiguration`
33. 🤝 `HealthChecks.Gcp.PubSub`
34. 🤝 `HealthChecks.Gcp.BigQuery`
35. 🤝 `HealthChecks.Gcp.Spanner`
36. 🤝 `HealthChecks.Gcp.SecretManager`
37. 🤝 `HealthChecks.Microsoft.Fabric`
38. 🤝 `HealthChecks.Databricks`
39. 🤝 `HealthChecks.Snowflake`
40. 🤝 `HealthChecks.Pinecone`
41. 🤝 `HealthChecks.Auth0`
42. 🤝 `HealthChecks.Okta`
43. 🤝 `HealthChecks.LaunchDarkly`

## Notes

- MassTransit and Quartz.NET are not prioritized because their ASP.NET Core integrations already expose health checks.
- Packages should follow the existing `DotNetDiag.HealthChecks.<Provider>` naming convention.
- Cloud-provider and proprietary SaaS candidates may be accepted when a contributor commits to the implementation, tests, documentation, and ongoing maintenance surface expected for new packages.
- New .NET or MSBuild changes must pass the repository format gate before being considered complete.
