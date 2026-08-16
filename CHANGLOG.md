# Changelog

## 2026-07-06

- Added health check packages for ActiveMQ, Apache Pulsar, Artemis, CockroachDB, DuckDB, Firebird, Grafana, IBM Db2, Memcached, Neo4j, and Valkey.
- Added dependency-injection registration APIs, package READMEs, API approval baselines, conformance tests, and behavior or functional tests for the new packages.
- Registered the new packages in the solution, central package management, README package list, documentation catalog, Codecov flags, GitHub labeler entries, and CI/CD workflows.
- Updated the roadmap to mark the new health checks as completed and renumber the remaining backlog.
- Moved HealthChecks UI and Kubernetes operator image references to `ghcr.io/dotnetdiag`, updated container defaults to port `8080`, and refreshed Docker build scripts and image documentation for the new registry and .NET 10 base images.
- Fixed ZooKeeper health check cancellation compatibility for `netstandard2.0` builds.
