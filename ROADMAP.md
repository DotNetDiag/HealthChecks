# Health Checks Roadmap

This roadmap tracks candidate health check packages that fill current ecosystem gaps in .NET applications.

## Status Legend

- ✅ Done
- 🚧 Next
- 📝 Planned
- 🔎 Research

## Priority Roadmap

1. ✅ `HealthChecks.OpenSearch`
   - OpenSearch cluster connectivity, ping, and cluster health checks.

2. ✅ `HealthChecks.Gcp.CloudStorage`
   - Google Cloud Storage bucket/service reachability.

3. 🚧 `HealthChecks.Gcp.PubSub`
   - Google Cloud Pub/Sub topic and subscription reachability.

4. 📝 `HealthChecks.Gcp.BigQuery`
   - BigQuery project, dataset, or lightweight query health checks.

5. 📝 `HealthChecks.Gcp.Spanner`
   - Cloud Spanner instance/database health checks.

6. 📝 `HealthChecks.Gcp.SecretManager`
   - Secret Manager service and configured secret access checks.

7. 📝 `HealthChecks.Snowflake`
   - Snowflake connection and lightweight query health checks.

8. 📝 `HealthChecks.Neo4j`
   - Neo4j driver connectivity and database health checks.

9. 📝 `HealthChecks.Pinecone`
   - Pinecone vector index/service health checks.

10. 📝 `HealthChecks.Weaviate`
    - Weaviate readiness and schema/service health checks.

11. 📝 `HealthChecks.Chroma`
    - Chroma server or collection health checks.

12. 📝 `HealthChecks.Vault`
    - HashiCorp Vault `/sys/health` health checks.

13. 📝 `HealthChecks.Minio`
    - MinIO live, ready, and cluster health endpoint checks.

## Backlog

14. 🔎 `HealthChecks.Apache.Pulsar`
15. 🔎 `HealthChecks.ActiveMQ` / `HealthChecks.Artemis`
16. 🔎 `HealthChecks.Aws.Kinesis`
17. 🔎 `HealthChecks.Aws.EventBridge`
18. 🔎 `HealthChecks.IbmDb2`
19. 🔎 `HealthChecks.Firebird`
20. 🔎 `HealthChecks.SapHana`
21. 🔎 `HealthChecks.CockroachDb`
22. 🔎 `HealthChecks.DuckDb`
23. 🔎 `HealthChecks.Memcached`
24. 🔎 `HealthChecks.Valkey`
25. 🔎 `HealthChecks.Databricks`
26. 🔎 `HealthChecks.Azure.Synapse`
27. 🔎 `HealthChecks.Microsoft.Fabric`
28. 🔎 `HealthChecks.Aws.Redshift`
29. 🔎 `HealthChecks.Trino`
30. 🔎 `HealthChecks.Apache.Druid`
31. 🔎 `HealthChecks.Apache.Pinot`
32. 🔎 `HealthChecks.Etcd`
33. 🔎 `HealthChecks.ZooKeeper`
34. 🔎 `HealthChecks.Harbor`
35. 🔎 `HealthChecks.ContainerRegistry`
36. 🔎 `HealthChecks.OpenTelemetryCollector`
37. 🔎 `HealthChecks.Grafana`
38. 🔎 `HealthChecks.Loki`
39. 🔎 `HealthChecks.Tempo`
40. 🔎 `HealthChecks.Jaeger`
41. 🔎 `HealthChecks.Alertmanager`
42. 🔎 `HealthChecks.Keycloak`
43. 🔎 `HealthChecks.Auth0`
44. 🔎 `HealthChecks.Okta`
45. 🔎 `HealthChecks.LaunchDarkly`
46. 🔎 `HealthChecks.Azure.AppConfiguration`

## Notes

- MassTransit and Quartz.NET are not prioritized because their ASP.NET Core integrations already expose health checks.
- Packages should follow the existing `DotNetDiag.HealthChecks.<Provider>` naming convention.
- New .NET or MSBuild changes must pass the repository format gate before being considered complete.
