[![License](https://img.shields.io/github/license/DotNetDiag/HealthChecks)](LICENSE)
[![codecov](https://codecov.io/github/DotNetDiag/HealthChecks/coverage.svg?branch=master)](https://codecov.io/github/DotNetDiag/HealthChecks?branch=master)
[![GitHub Release Date](https://img.shields.io/github/release-date/DotNetDiag/HealthChecks?label=released)](https://github.com/DotNetDiag/HealthChecks/releases)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/DotNetDiag/HealthChecks/latest?label=new+commits)](https://github.com/DotNetDiag/HealthChecks/commits/master)
![Size](https://img.shields.io/github/repo-size/DotNetDiag/HealthChecks)

[![GitHub contributors](https://img.shields.io/github/contributors/DotNetDiag/HealthChecks)](https://github.com/DotNetDiag/HealthChecks/contributors)
![Activity](https://img.shields.io/github/commit-activity/w/DotNetDiag/HealthChecks)
![Activity](https://img.shields.io/github/commit-activity/m/DotNetDiag/HealthChecks)
![Activity](https://img.shields.io/github/commit-activity/y/DotNetDiag/HealthChecks)

# DotNetDiag HealthChecks

DotNetDiag HealthChecks is the actively maintained fork of AspNetCore.Diagnostics.HealthChecks. This repository offers a wide collection of **ASP.NET Core** health check packages for widely used services and platforms.

**ASP.NET Core** versions supported: 10.0,,8.0 and netstandard2.0 !

# Sections

## HealthChecks

- [Health Checks](#Health-Checks)
- [Health Checks Push Results](#HealthCheck-push-results)

## HealthChecks UI

- [UI](#HealthCheckUI)
- [UI Storage Providers](#UI-Storage-Providers)
- [UI Database Migrations](#UI-Database-Migrations)
- [History Timeline](#Health-status-history-timeline)
- [Configuration](#Configuration)
- [Webhooks and Failure Notifications](#Webhooks-and-Failure-Notifications)
- [HttpClient and HttpMessageHandler Configuration](#UI-Configure-HttpClient-and-HttpMessageHandler-for-Api-and-Webhooks-endpoints)

## HealthChecks UI and Kubernetes

- [Kubernetes Operator](#UI-Kubernetes-Operator)
- [Kubernetes automatic services discovery](#UI-Kubernetes-automatic-services-discovery)

## HealthChecks and Devops

- [Releases Gates for Azure DevOps Pipelines](#HealthChecks-as-Release-Gates-for-Azure-DevOps-Pipelines)

## HealthChecks Tutorials

- [Tutorials, Demos and walkthroughs](#tutorials-demos-and-walkthroughs-on-aspnet-core-healthchecks)

## Docker images

HealthChecks repo provides following images:

> The current published container images still use the `xabarilcoding/*` namespace. Package IDs, repository links, and documentation branding have moved to DotNetDiag.

| Image | Downloads | Latest | Issues |
|------|--------|---|---|
| UI | ![ui pulls](https://img.shields.io/docker/pulls/xabarilcoding/healthchecksui.svg?label=downloads) | ![ui version](https://img.shields.io/docker/v/xabarilcoding/healthchecksui?label=docker&logo=dsd&sort=date) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/ui)](https://github.com/DotNetDiag/HealthChecks/labels/ui)
| K8s operator | ![k8s pulls](https://img.shields.io/docker/pulls/xabarilcoding/healthchecksui-k8s-operator.svg?label=downloads) | ![k8s version](https://img.shields.io/docker/v/xabarilcoding/healthchecksui-k8s-operator?label=docker&logo=dsd&sort=date)

## Health Checks

HealthChecks packages include health checks for:

> NuGet package IDs follow the convention: `DotNetDiag.<project name>`.

| Package | Downloads | NuGet Latest | Issues | Notes |
| --- | --- | --- | --- | --- |
| ApplicationStatus | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.ApplicationStatus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.ApplicationStatus) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.ApplicationStatus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.ApplicationStatus) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/applicationstatus)](https://github.com/DotNetDiag/HealthChecks/labels/applicationstatus) | |
| ArangoDB | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.ArangoDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.ArangoDb) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.ArangoDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.ArangoDb) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/arrangodb)](https://github.com/DotNetDiag/HealthChecks/labels/arrangodb) | |
| Amazon S3 | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Aws.S3)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.S3) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Aws.S3)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.S3) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/aws)](https://github.com/DotNetDiag/HealthChecks/labels/aws) | |
| Amazon Secrets Manager | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Aws.SecretsManager)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.SecretsManager) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Aws.SecretsManager)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.SecretsManager) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/aws)](https://github.com/DotNetDiag/HealthChecks/labels/aws) | |
| Amazon SNS | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Aws.Sns)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.Sns) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Aws.Sns)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.Sns) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/aws)](https://github.com/DotNetDiag/HealthChecks/labels/aws) | |
| Amazon SQS | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Aws.Sqs)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.Sqs) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Aws.Sqs)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.Sqs) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/aws)](https://github.com/DotNetDiag/HealthChecks/labels/aws) | |
| Amazon Systems Manager | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Aws.SystemsManager)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.SystemsManager) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Aws.SystemsManager)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Aws.SystemsManager) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/aws)](https://github.com/DotNetDiag/HealthChecks/labels/aws) | |
| Azure Application Insights | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.AzureApplicationInsights)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureApplicationInsights) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.AzureApplicationInsights)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureApplicationInsights) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/applicationinsights)](https://github.com/DotNetDiag/HealthChecks/labels/applicationinsights) | |
| Azure Tables | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Azure.Data.Tables)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Data.Tables) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Azure.Data.Tables)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Data.Tables) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure IoT Hub | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Azure.IoTHub)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.IoTHub) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Azure.IoTHub)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.IoTHub) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure Key Vault Secrets | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Azure.KeyVault.Secrets)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.KeyVault.Secrets) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Azure.KeyVault.Secrets)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.KeyVault.Secrets) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure Event Hubs | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Azure.Messaging.EventHubs)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Messaging.EventHubs) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Azure.Messaging.EventHubs)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Messaging.EventHubs) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure Blob Storage | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Azure.Storage.Blobs)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Storage.Blobs) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Azure.Storage.Blobs)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Storage.Blobs) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure File Storage | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Azure.Storage.Files.Shares)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Storage.Files.Shares) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Azure.Storage.Files.Shares)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Storage.Files.Shares) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure Queue Storage | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Azure.Storage.Queues)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Storage.Queues) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Azure.Storage.Queues)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Azure.Storage.Queues) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure DigitalTwin | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.AzureDigitalTwin)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureDigitalTwin) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.AzureDigitalTwin)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureDigitalTwin) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | Subscription status, models and instances |
| Azure Key Vault | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.AzureKeyVault)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureKeyVault) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.AzureKeyVault)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureKeyVault) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure Search | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.AzureSearch)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureSearch) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.AzureSearch)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureSearch) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | |
| Azure Service Bus | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.AzureServiceBus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureServiceBus) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.AzureServiceBus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.AzureServiceBus) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/azure)](https://github.com/DotNetDiag/HealthChecks/labels/azure) | Queue and Topics |
| ClickHouse | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.ClickHouse)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.ClickHouse) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.ClickHouse)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.ClickHouse) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/clickhouse)](https://github.com/DotNetDiag/HealthChecks/labels/clickhouse) | |
| Consul | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Consul)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Consul) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Consul)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Consul) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/consul)](https://github.com/DotNetDiag/HealthChecks/labels/consul) | |
| CosmosDb | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.CosmosDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.CosmosDb) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.CosmosDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.CosmosDb) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/cosmosdb)](https://github.com/DotNetDiag/HealthChecks/labels/cosmosdb) | CosmosDb and Azure Table |
| Dapr | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Dapr)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Dapr) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Dapr)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Dapr) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/dapr)](https://github.com/DotNetDiag/HealthChecks/labels/dapr) | |
| Amazon DynamoDb | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.DynamoDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.DynamoDb) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.DynamoDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.DynamoDb) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/dynamodb)](https://github.com/DotNetDiag/HealthChecks/labels/dynamodb) | |
| Elasticsearch | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Elasticsearch)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Elasticsearch) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Elasticsearch)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Elasticsearch) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/elasticsearch)](https://github.com/DotNetDiag/HealthChecks/labels/elasticsearch) | |
| EventStore | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.EventStore)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.EventStore) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.EventStore)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.EventStore) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/eventstore)](https://github.com/DotNetDiag/HealthChecks/labels/eventstore) | [TCP EventStore](https://github.com/EventStore/EventStoreDB-Client-Dotnet-Legacy) |
| EventStore gRPC | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.EventStore.gRPC)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.EventStore.gRPC) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.EventStore.gRPC)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.EventStore.gRPC) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/eventstore)](https://github.com/DotNetDiag/HealthChecks/labels/eventstore) | [gRPC EventStore](https://github.com/EventStore/EventStore-Client-Dotnet) |
| Google Cloud Firestore | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Gcp.CloudFirestore)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Gcp.CloudFirestore) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Gcp.CloudFirestore)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Gcp.CloudFirestore) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/cloudfirestore)](https://github.com/DotNetDiag/HealthChecks/labels/cloudfirestore) | |
| Gremlin | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Gremlin)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Gremlin) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Gremlin)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Gremlin) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/gremlin)](https://github.com/DotNetDiag/HealthChecks/labels/gremlin) | |
| Hangfire | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Hangfire)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Hangfire) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Hangfire)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Hangfire) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/hangfire)](https://github.com/DotNetDiag/HealthChecks/labels/hangfire) | |
| IbmMQ | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.IbmMQ)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.IbmMQ) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.IbmMQ)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.IbmMQ) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/ibmmq)](https://github.com/DotNetDiag/HealthChecks/labels/ibmmq) | |
| InfluxDB | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.InfluxDB)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.InfluxDB) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.InfluxDB)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.InfluxDB) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/influxdb)](https://github.com/DotNetDiag/HealthChecks/labels/influxdb) | |
| Kafka | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Kafka)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Kafka) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Kafka)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Kafka) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/kafka)](https://github.com/DotNetDiag/HealthChecks/labels/kafka) | |
| Kubernetes | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Kubernetes)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Kubernetes) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Kubernetes)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Kubernetes) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/kubernetes)](https://github.com/DotNetDiag/HealthChecks/labels/kubernetes) | |
| Milvus | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Milvus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Milvus) | [![Nuget](https://img.shields.io/nuget/vpre/DotNetDiag.HealthChecks.Milvus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Milvus) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/milvus)](https://github.com/DotNetDiag/HealthChecks/labels/milvus) | Preview-only because `Milvus.Client` is prerelease-only |
| MongoDB | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.MongoDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.MongoDb) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.MongoDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.MongoDb) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/mongodb)](https://github.com/DotNetDiag/HealthChecks/labels/mongodb) | |
| MySql | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.MySql)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.MySql) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.MySql)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.MySql) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/mysql)](https://github.com/DotNetDiag/HealthChecks/labels/mysql) | |
| Nats | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Nats)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Nats) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Nats)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Nats) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/nats)](https://github.com/DotNetDiag/HealthChecks/labels/nats) | NATS, messaging, message-bus, pubsub |
| Network | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Network)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Network) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Network)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Network) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/network)](https://github.com/DotNetDiag/HealthChecks/labels/network) | Ftp, SFtp, Dns, Tcp port, Smtp, Imap, Ssl |
| Postgres | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.NpgSql)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.NpgSql) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.NpgSql)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.NpgSql) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/npgsql)](https://github.com/DotNetDiag/HealthChecks/labels/npgsql) | |
| OpenID Connect Server | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.OpenIdConnectServer)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.OpenIdConnectServer) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.OpenIdConnectServer)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.OpenIdConnectServer) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/openidconnect)](https://github.com/DotNetDiag/HealthChecks/labels/openidconnect) | |
| Oracle | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Oracle)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Oracle) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Oracle)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Oracle) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/oracle)](https://github.com/DotNetDiag/HealthChecks/labels/oracle) | |
| Qdrant | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Qdrant)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Qdrant) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Qdrant)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Qdrant) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/qdrant)](https://github.com/DotNetDiag/HealthChecks/labels/qdrant) | |
| RabbitMQ | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Rabbitmq)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Rabbitmq) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Rabbitmq)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Rabbitmq) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/rabbitmq)](https://github.com/DotNetDiag/HealthChecks/labels/rabbitmq) | |
| RavenDB | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.RavenDB)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.RavenDB) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.RavenDB)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.RavenDB) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/ravendb)](https://github.com/DotNetDiag/HealthChecks/labels/ravendb) | |
| Redis | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Redis)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Redis) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Redis)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Redis) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/redis)](https://github.com/DotNetDiag/HealthChecks/labels/redis) | |
| SendGrid | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.SendGrid)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SendGrid) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.SendGrid)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SendGrid) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/sendgrid)](https://github.com/DotNetDiag/HealthChecks/labels/sendgrid) | |
| SignalR | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.SignalR)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SignalR) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.SignalR)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SignalR) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/signalr)](https://github.com/DotNetDiag/HealthChecks/labels/signalr) | |
| Solr | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Solr)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Solr) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Solr)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Solr) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/solr)](https://github.com/DotNetDiag/HealthChecks/labels/solr) | |
| Sqlite | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Sqlite)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Sqlite) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Sqlite)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Sqlite) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/sqlite)](https://github.com/DotNetDiag/HealthChecks/labels/sqlite) | |
| Sql Server | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.SqlServer)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SqlServer) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.SqlServer)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SqlServer) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/sqlserver)](https://github.com/DotNetDiag/HealthChecks/labels/sqlserver) | |
| SurrealDB | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.SurrealDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SurrealDb) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.SurrealDb)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.SurrealDb) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/surrealdb)](https://github.com/DotNetDiag/HealthChecks/labels/surrealdb) | |
| System | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.System)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.System) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.System)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.System) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/system)](https://github.com/DotNetDiag/HealthChecks/labels/system) | Disk Storage, Folder, File, Private Memory, Virtual Memory, Process, Windows Service |
| Uris | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Uris)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Uris) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Uris)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Uris) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/uris)](https://github.com/DotNetDiag/HealthChecks/labels/uris) | Single uri and uri groups |

> We support netcoreapp 2.2, 3.0 and 3.1. Please use package versions 2.2.X, 3.0.X and 3.1.X to target different versions.

```PowerShell
Install-Package DotNetDiag.HealthChecks.ApplicationStatus
Install-Package DotNetDiag.HealthChecks.ArangoDb
Install-Package DotNetDiag.HealthChecks.Aws.S3
Install-Package DotNetDiag.HealthChecks.Aws.SecretsManager
Install-Package DotNetDiag.HealthChecks.Aws.Sns
Install-Package DotNetDiag.HealthChecks.Aws.Sqs
Install-Package DotNetDiag.HealthChecks.Aws.SystemsManager
Install-Package DotNetDiag.HealthChecks.Azure.Data.Tables
Install-Package DotNetDiag.HealthChecks.Azure.IoTHub
Install-Package DotNetDiag.HealthChecks.Azure.KeyVault.Secrets
Install-Package DotNetDiag.HealthChecks.Azure.Messaging.EventHubs
Install-Package DotNetDiag.HealthChecks.Azure.Storage.Blobs
Install-Package DotNetDiag.HealthChecks.Azure.Storage.Files.Shares
Install-Package DotNetDiag.HealthChecks.Azure.Storage.Queues
Install-Package DotNetDiag.HealthChecks.AzureApplicationInsights
Install-Package DotNetDiag.HealthChecks.AzureDigitalTwin
Install-Package DotNetDiag.HealthChecks.AzureKeyVault
Install-Package DotNetDiag.HealthChecks.AzureSearch
Install-Package DotNetDiag.HealthChecks.AzureServiceBus
Install-Package DotNetDiag.HealthChecks.ClickHouse
Install-Package DotNetDiag.HealthChecks.Consul
Install-Package DotNetDiag.HealthChecks.CosmosDb
Install-Package DotNetDiag.HealthChecks.Dapr
Install-Package DotNetDiag.HealthChecks.DynamoDb
Install-Package DotNetDiag.HealthChecks.Elasticsearch
Install-Package DotNetDiag.HealthChecks.EventStore
Install-Package DotNetDiag.HealthChecks.EventStore.gRPC
Install-Package DotNetDiag.HealthChecks.Gcp.CloudFirestore
Install-Package DotNetDiag.HealthChecks.Gremlin
Install-Package DotNetDiag.HealthChecks.Hangfire
Install-Package DotNetDiag.HealthChecks.IbmMQ
Install-Package DotNetDiag.HealthChecks.InfluxDB
Install-Package DotNetDiag.HealthChecks.Kafka
Install-Package DotNetDiag.HealthChecks.Kubernetes
Install-Package DotNetDiag.HealthChecks.Milvus -Prerelease
Install-Package DotNetDiag.HealthChecks.MongoDb
Install-Package DotNetDiag.HealthChecks.MySql
Install-Package DotNetDiag.HealthChecks.Nats
Install-Package DotNetDiag.HealthChecks.Network
Install-Package DotNetDiag.HealthChecks.NpgSql
Install-Package DotNetDiag.HealthChecks.OpenIdConnectServer
Install-Package DotNetDiag.HealthChecks.Oracle
Install-Package DotNetDiag.HealthChecks.Qdrant
Install-Package DotNetDiag.HealthChecks.Rabbitmq
Install-Package DotNetDiag.HealthChecks.RavenDB
Install-Package DotNetDiag.HealthChecks.Redis
Install-Package DotNetDiag.HealthChecks.SendGrid
Install-Package DotNetDiag.HealthChecks.SignalR
Install-Package DotNetDiag.HealthChecks.Solr
Install-Package DotNetDiag.HealthChecks.Sqlite
Install-Package DotNetDiag.HealthChecks.SqlServer
Install-Package DotNetDiag.HealthChecks.SurrealDb
Install-Package DotNetDiag.HealthChecks.System
Install-Package DotNetDiag.HealthChecks.Uris
```

Once the package is installed you can add the HealthCheck using the **AddXXX** `IServiceCollection` extension methods.

> Preview and integration packages are published through [GitHub Packages](https://nuget.pkg.github.com/DotNetDiag/index.json).

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddSqlServer(Configuration["Data:ConnectionStrings:Sql"])
        .AddRedis(Configuration["Data:ConnectionStrings:Redis"]);
}
```

Each HealthCheck registration supports also name, tags, failure status and other optional parameters.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSqlServer(
            connectionString: Configuration["Data:ConnectionStrings:Sql"],
            healthQuery: "SELECT 1;",
            name: "sql",
            failureStatus: HealthStatus.Degraded,
            tags: ["db", "sql", "sqlserver"]);
}
```

## HealthCheck push results

HealthChecks include a _push model_ to send HealthCheckReport results into configured consumers.
The project **DotNetDiag.HealthChecks.Publisher.ApplicationInsights**, **DotNetDiag.HealthChecks.Publisher.Datadog**,
**DotNetDiag.HealthChecks.Publisher.Prometheus**, **DotNetDiag.HealthChecks.Publisher.Seq** or
**DotNetDiag.HealthChecks.Publisher.CloudWatch** define a consumers to send report results to
Application Insights, Datadog, Prometheus, Seq or CloudWatch.

| Package              | Downloads                                                                                                                                                                               | NuGet Latest | Issues | Notes |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------ | ----- |
| Application Insights | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Publisher.ApplicationInsights)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.ApplicationInsights) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Publisher.ApplicationInsights)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.ApplicationInsights) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/applicationinsights)](https://github.com/DotNetDiag/HealthChecks/labels/applicationinsights)
| CloudWatch           | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Publisher.CloudWatch)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.CloudWatch)                   | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Publisher.CloudWatch)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.CloudWatch) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/cloudwatch)](https://github.com/DotNetDiag/HealthChecks/labels/cloudwatch)
| Datadog              | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Publisher.Datadog)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.Datadog)                         | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Publisher.Datadog)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.Datadog) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/datadog)](https://github.com/DotNetDiag/HealthChecks/labels/datadog)
| Prometheus Gateway   | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Publisher.Prometheus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.Prometheus)                   | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Publisher.Prometheus)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.Prometheus) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/prometheus)](https://github.com/DotNetDiag/HealthChecks/labels/prometheus) | **DEPRECATED** |
| Seq                  | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.Publisher.Seq)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.Seq)                                 | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Publisher.Seq)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Publisher.Seq) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/seq)](https://github.com/DotNetDiag/HealthChecks/labels/seq)

Include the package in your project:

```powershell
install-package DotNetDiag.HealthChecks.Publisher.ApplicationInsights
install-package DotNetDiag.HealthChecks.Publisher.CloudWatch
install-package DotNetDiag.HealthChecks.Publisher.Datadog
install-package DotNetDiag.HealthChecks.Publisher.Prometheus
install-package DotNetDiag.HealthChecks.Publisher.Seq
```

Add publisher[s] into the `IHealthCheckBuilder`:

```csharp
services
    .AddHealthChecks()
    .AddSqlServer(connectionString: Configuration["Data:ConnectionStrings:Sample"])
    .AddCheck<RandomHealthCheck>("random")
    .AddApplicationInsightsPublisher()
    .AddCloudWatchPublisher()
    .AddDatadogPublisher("myservice.healthchecks")
    .AddPrometheusGatewayPublisher();
```

## HealthChecks Prometheus Exporter

If you need an endpoint to consume from prometheus instead of using Prometheus Gateway you could install **DotNetDiag.HealthChecks.Prometheus.Metrics**.

```powershell
install-package DotNetDiag.HealthChecks.Prometheus.Metrics
```

Use the `ApplicationBuilder` extension method to add the endpoint with the metrics:

```csharp
// default endpoint: /healthmetrics
app.UseHealthChecksPrometheusExporter();

// You could customize the endpoint
app.UseHealthChecksPrometheusExporter("/my-health-metrics");

// Customize HTTP status code returned(prometheus will not read health metrics when a default HTTP 503 is returned)
app.UseHealthChecksPrometheusExporter("/my-health-metrics", options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK);
```

## HealthCheckUI

![HealthChecksUI](./docs/images/ui-home.png)

[UI Changelog](./docs/ui-changelog.md)

The project HealthChecks.UI is a minimal UI interface that stores and shows the health checks results from the configured HealthChecks URIs.

For UI, we provide the following packages:

| Package   | Downloads                                                                                                                                       | NuGet Latest | Issues | Notes |
| --------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------ | ------|
| UI        | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI)               | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/ui)](https://github.com/DotNetDiag/HealthChecks/labels/ui) | ASP.NET Core UI viewer of ASP.NET Core HealthChecks |
| UI.Client | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.Client)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.Client) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.Client)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.Client) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/ui)](https://github.com/DotNetDiag/HealthChecks/labels/ui) | Mandatory abstractions to work with HealthChecks.UI |
| UI.Core   | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.Core)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.Core)     | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.Core)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.Core) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/ui)](https://github.com/DotNetDiag/HealthChecks/labels/ui) | |
| UI.Data   | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.Data)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.Data)     | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.Data)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.Data) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/ui)](https://github.com/DotNetDiag/HealthChecks/labels/ui) | Data models and database context definition |

To integrate HealthChecks.UI in your project you just need to add the HealthChecks.UI services and middlewares available in the package: **DotNetDiag.HealthChecks.UI**

```csharp
using HealthChecks.UI.Core;
using HealthChecks.UI.InMemory.Storage;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecksUI()
            .AddInMemoryStorage();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app
            .UseRouting()
            .UseEndpoints(config => config.MapHealthChecksUI());
    }
}
```

This automatically registers a new interface on **/healthchecks-ui** where the SPA will be served.

> Optionally, `MapHealthChecksUI` can be configured to serve its health API, webhooks API and the front-end resources in
> different endpoints using the `MapHealthChecksUI(setup => { })` method overload. The default configured URLs for these endpoints
> can be found [here](https://github.com/DotNetDiag/HealthChecks/blob/master/src/HealthChecks.UI/Configuration/Options.cs)

**Important note:** It is important to understand that the API endpoint that the UI serves is used by the frontend SPA to receive the result
of all processed checks. The health reports are collected by a background hosted service and the API endpoint served at /healthchecks-api by
default is the URL that the SPA queries.

Do not confuse this UI API endpoint with the endpoints we have to configure to declare the target APIs to be checked on the UI project in
the [appsettings HealthChecks configuration section](https://github.com/DotNetDiag/HealthChecks/blob/master/samples/HealthChecks.UI.Sample/appsettings.json)

When we target applications to be tested and shown on the UI interface, those endpoints have to register the `UIResponseWriter` that is present
on the **DotNetDiag.HealthChecks.UI.Client** as their [ResponseWriter in the HealthChecksOptions](https://github.com/DotNetDiag/HealthChecks/blob/master/samples/HealthChecks.Sample/Startup.cs#L48) when configuring MapHealthChecks method.

### UI Polling interval

You can configure the polling interval in seconds for the UI inside the setup method. Default value is 10 seconds:

```csharp
.AddHealthChecksUI(setupSettings: setup =>
{
    setup.SetEvaluationTimeInSeconds(5); // Configures the UI to poll for healthchecks updates every 5 seconds
});
```

### UI API max active requests

You can configure max active requests to the HealthChecks UI backend api using the setup method. Default value is 3 active requests:

```csharp
.AddHealthChecksUI(setupSettings: setup =>
{
    setup.SetApiMaxActiveRequests(1);
    //Only one active request will be executed at a time.
    //All the excedent requests will result in 429 (Too many requests)
});
```

### UI Storage Providers

HealthChecks UI offers several storage providers, available as different nuget packages.

The current supported databases are:

| Package    | Downloads                                                                                                                                                               | NuGet Latest | Issues | Notes |
| ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------ | ----- |
| InMemory   | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.InMemory.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.InMemory.Storage)     | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.InMemory.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.InMemory.Storage) |
| SqlServer  | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.SqlServer.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.SqlServer.Storage)   | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.SqlServer.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.SqlServer.Storage) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/sqlserver)](https://github.com/DotNetDiag/HealthChecks/labels/sqlserver)
| SQLite     | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.SQLite.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.SQLite.Storage)         | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.SQLite.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.SQLite.Storage) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/sqlite)](https://github.com/DotNetDiag/HealthChecks/labels/sqlite)
| PostgreSQL | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.PostgreSQL.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.PostgreSQL.Storage) | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.PostgreSQL.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.PostgreSQL.Storage) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/npgsql)](https://github.com/DotNetDiag/HealthChecks/labels/npgsql)
| MySql      | [![Nuget](https://img.shields.io/nuget/dt/DotNetDiag.HealthChecks.UI.MySql.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.MySql.Storage)           | [![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.UI.MySql.Storage)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.UI.MySql.Storage) | [![view](https://img.shields.io/github/issues/DotNetDiag/HealthChecks/mysql)](https://github.com/DotNetDiag/HealthChecks/labels/mysql)

All the storage providers are extensions of `HealthChecksUIBuilder`:

**InMemory**

```csharp
services
    .AddHealthChecksUI()
    .AddInMemoryStorage();
```

**Sql Server**

```csharp
services
    .AddHealthChecksUI()
    .AddSqlServerStorage("connectionString");
```

**Postgre SQL**

```csharp
services
    .AddHealthChecksUI()
    .AddPostgreSqlStorage("connectionString");
```

**MySql**

```csharp
services
    .AddHealthChecksUI()
    .AddMySqlStorage("connectionString");
```

**Sqlite**

```csharp
services
    .AddHealthChecksUI()
    .AddSqliteStorage($"Data Source=sqlite.db");
```

### UI Database Migrations

**Database Migrations** are enabled by default, if you need to disable migrations you can use the `AddHealthChecksUI` setup:

```csharp
services
    .AddHealthChecksUI(setup => setup.DisableDatabaseMigrations())
    .AddInMemoryStorage();
```

Or you can use `IConfiguration` providers, like json file or environment variables:

```json
"HealthChecksUI": {
  "DisableMigrations": true
}
```

### Health status history timeline

By clicking details button in the healthcheck row, you can preview the health status history timeline:

![Timeline](./docs/images/timeline.png)

**Note**: HealthChecks UI saves an execution history entry in the database whenever a HealthCheck status changes from Healthy to Unhealthy and viceversa.

This information is displayed in the status history timeline, but we do not perform purge or cleanup tasks in users' databases.
In order to limit the maximum history entries that are sent by the UI API middleware to the frontend, you can do a database cleanup or set the maximum history entries served by endpoint using:

```csharp
services.AddHealthChecksUI(setup =>
{
    // Set the maximum history entries by endpoint that will be served by the UI api middleware
    setup.MaximumHistoryEntriesPerEndpoint(50);
});
```

**HealthChecksUI** is also available as a _docker image_ You can read more about [HealthChecks UI Docker image](./docs/ui-docker.md).

### Configuration

By default, HealthChecks return a simple Status Code (200 or 503) without the HealthReport data. If you want the
HealthCheck-UI to show the HealthReport data from your HealthCheck, you can enable it by adding a specific `ResponseWriter`.

```csharp
app
    .UseRouting()
    .UseEndpoints(config =>
    {
        config.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    });
```

> _WriteHealthCheckUIResponse_ is defined on the DotNetDiag.HealthChecks.UI.Client NuGet package.

To show these HealthChecks in HealthCheck-UI, they have to be configured through the **HealthCheck-UI** settings.

You can configure these Healthchecks and webhooks by using `IConfiguration` providers (appsettings, user secrets, env variables) or the `AddHealthChecksUI(setupSettings: setup => { })` method can be used too.

#### Sample 2: Configuration using appsettings.json

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api-Basic",
        "Uri": "http://localhost:6457/healthz"
      }
    ],
    "Webhooks": [
      {
        "Name": "",
        "Uri": "",
        "Payload": "",
        "RestoredPayload": ""
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

#### Sample 2: Configuration using setupSettings method:

```csharp
services
    .AddHealthChecksUI(setupSettings: setup =>
    {
       setup.AddHealthCheckEndpoint("endpoint1", "http://localhost:8001/healthz");
       setup.AddHealthCheckEndpoint("endpoint2", "http://remoteendpoint:9000/healthz");
       setup.AddWebhookNotification("webhook1", uri: "http://httpbin.org/status/200?code=ax3rt56s", payload: "{...}");
    })
    .AddSqlServer("connectionString");
```

**Note**: The previous configuration section was HealthChecks-UI, but due to incompatibilies with Azure Web App environment variables, the section has been moved to HealthChecksUI. The UI is retro compatible and it will check the new section first, and fallback to the old section if the new section has not been declared.

    1.- HealthChecks: The collection of health checks uris to evaluate.
    2.- EvaluationTimeInSeconds: Number of elapsed seconds between health checks.
    3.- Webhooks: If any health check returns a *Failure* result, this collections will be used to notify the error status. (Payload is the json payload and must be escaped. For more information see the notifications documentation section)
    4.- MinimumSecondsBetweenFailureNotifications: The minimum seconds between failure notifications to avoid receiver flooding.

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api-Basic",
        "Uri": "http://localhost:6457/healthz"
      }
    ],
    "Webhooks": [
      {
        "Name": "",
        "Uri": "",
        "Payload": "",
        "RestoredPayload": ""
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

### Using relative URLs in Health Checks and Webhooks configurations (UI 3.0.5 onwards)

If you are configuring the UI in the same process where the HealthChecks and Webhooks are listening, from version 3.0.5 onwards the UI can use relative URLs,
and it will automatically discover the listening endpoints by using server `IServerAddressesFeature`.

Sample:

```csharp
//Configuration sample with relative URL health checks and webhooks
services
    .AddHealthChecksUI(setupSettings: setup =>
    {
       setup.AddHealthCheckEndpoint("endpoint1", "/health-databases");
       setup.AddHealthCheckEndpoint("endpoint2", "health-messagebrokers");
       setup.AddWebhookNotification("webhook1", uri: "/notify", payload: "{...}");
    })
    .AddSqlServer("connectionString");
```

You can also use relative URLs when using `IConfiguration` providers like appsettings.json.

### Webhooks and Failure Notifications

If the **WebHooks** section is configured, HealthCheck-UI automatically posts a new notification into the webhook collection. HealthCheckUI uses a simple replace method for values in the webhook's **Payload** and **RestorePayload** properties. At this moment we support two bookmarks:

[[LIVENESS]] The name of the liveness that returns _Down_.

[[FAILURE]] A detail message with the failure.

[[DESCRIPTIONS]] Failure descriptions

Webhooks can be configured with configuration providers and also by code. Using code allows greater customization as you can setup you own user functions to customize output messages or configuring if a payload should be sent to a given webhook endpoint.

The [web hooks section](./docs/webhooks.md) contains more information and webhooks samples for Microsoft Teams, Azure Functions, Slack and more.

**Avoid Fail notification spam**

To prevent you from receiving several failure notifications from your application, a configuration was created to meet this scenario.

```csharp
services.AddHealthChecksUI(setup =>
{
    setup.SetNotifyUnHealthyOneTimeUntilChange(); // You will only receive one failure notification until the status changes.
});
```

## UI Style and branding customization

### Sample of dotnet styled UI

![HealthChecksUIBranding](./docs/images/ui-branding.png)

Since version 2.2.34, UI supports custom styles and branding by using a **custom style sheet** and **css variables**.
To add your custom styles sheet, use the UI setup method:

```csharp
app
    .UseRouting()
    .UseEndpoints(config =>
    {
        config.MapHealthChecksUI(setup =>
        {
            setup.AddCustomStylesheet("dotnet.css");
        });
    });
```

You can visit the section [custom styles and branding](./docs/styles-branding.md) to find source samples and get further information about custom css properties.

## UI Configure HttpClient and HttpMessageHandler for Api and Webhooks endpoints

If you need to configure a proxy, or set an authentication header, the UI allows you to configure the `HttpMessageHandler` and the `HttpClient` for the webhooks and healtheck api endpoints. You can also register custom delegating handlers for the API and WebHooks HTTP clients.

```csharp
services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.ConfigureApiEndpointHttpclient((sp, client) =>
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "supertoken");
    })
    .UseApiEndpointHttpMessageHandler(sp =>
    {
        return new HttpClientHandler
        {
            Proxy = new WebProxy("http://proxy:8080")
        };
    })
    .UseApiEndpointDelegatingHandler<CustomDelegatingHandler>()
    .ConfigureWebhooksEndpointHttpclient((sp, client) =>
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sampletoken");
    })
    .UseWebhookEndpointHttpMessageHandler(sp =>
    {
        return new HttpClientHandler()
        {
            Properties =
            {
                ["prop"] = "value"
            }
        };
    })
    .UseWebHooksEndpointDelegatingHandler<CustomDelegatingHandler2>();
})
.AddInMemoryStorage();
```

## UI Kubernetes Operator

If you are running your workloads in kubernetes, you can benefit from it and have your healthchecks environment ready and monitoring in seconds.

You can get for information in our [HealthChecks Operator docs](./docs/k8s-operator.md)

## UI Kubernetes automatic services discovery

<!-- ![k8s-discovery](./docs/images/k8s-discovery-service.png) -->

HealthChecks UI supports automatic discovery of k8s services exposing pods that have health checks endpoints. This means, you can benefit from it and avoid registering all the endpoints you want to check and let the UI discover them using the k8s api.

You can get more information [here](./docs/k8s-ui-discovery.md)

## HealthChecks as Release Gates for Azure DevOps Pipelines

HealthChecks can be used as [Release Gates for Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/approvals/gates?view=azure-devops) using this [Visual Studio Market place Extension](https://marketplace.visualstudio.com/items?itemName=luisfraile.vss-services-aspnetcorehealthcheck-extensions).

Check this [README](./extensions/README.md) on how to configure it.

## Protected HealthChecks.UI with OpenID Connect

There are some scenarios where you can find useful to restrict access for users on HealthChecks UI, maybe for users who belong to some role, based on some claim value etc.

We can leverage the ASP.NET Core Authentication/Authorization features to easily implement it. You can see a fully functional example using IdentityServer4 [here](https://github.com/DotNetDiag/HealthChecks/tree/master/samples/HealthChecks.UI.Oidc) but you can use Azure AD, Auth0, Okta, etc.

Check this [README](./extensions/README.md) on how to configure it.

## Tutorials, demos and walkthroughs on ASP.NET Core HealthChecks

- [ASP.NET Core HealthChecks and Kubernetes Liveness / Readiness by Carlos Landeras](./docs/kubernetes-liveness.md)
- [ASP.NET Core HealthChecks, BeatPulse UI, Webhooks and Kubernetes Liveness / Readiness probes demos at SDN.nl live WebCast by Carlos Landeras](https://www.youtube.com/watch?v=kzRKGCmGbqo)
- [ASP.NET Core HealthChecks features video by @condrong](https://t.co/YriQ6cLWVm)
- [How to set up ASP.NET Core 2.2 Health Checks with BeatPulse's AspNetCore.Diagnostics.HealthChecks by Scott Hanselman](https://www.hanselman.com/blog/HowToSetUpASPNETCore22HealthChecksWithBeatPulsesAspNetCoreDiagnosticsHealthChecks.aspx)
- [ASP.NET Core HealthChecks announcement](https://t.co/47M9FBfpWF)
- [ASP.NET Core 2.2 HealthChecks Explained by Thomas Ardal](https://blog.elmah.io/asp-net-core-2-2-health-checks-explained/)
- [Health Monitoring on ASP.NET Core 2.2 / eShopOnContainers](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/monitor-app-health)

## Contributing

DotNetDiag HealthChecks wouldn't be possible without the time and effort of its contributors.
The team is made up of Unai Zorrilla Castro [@unaizorrilla](https://github.com/unaizorrilla),
Luis Ruiz Pavón [@lurumad](https://github.com/lurumad), Carlos Landeras [@carloslanderas](https://github.com/carloslanderas),
Eduard Tomás [@eiximenis](https://github.com/eiximenis), Eva Crespo [@evacrespob](https://github.com/evacrespob) and
Ivan Maximov [@sungam3r](https://github.com/sungam3r).

Thanks to all the people who already contributed!

<a href="https://github.com/DotNetDiag/HealthChecks/graphs/contributors">
    <img src="https://contributors-img.web.app/image?repo=DotNetDiag/HealthChecks" />
</a>

If you want to contribute to the project and make it better, your help is very welcome.
You can contribute with helpful bug reports, features requests, submitting new features with pull requests and also
answering [questions](https://github.com/DotNetDiag/HealthChecks/labels/question).

1. Read and follow the [Don't push your pull requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/)
2. Follow the code guidelines and conventions.
3. New features are not only code, tests and documentation are also mandatory.
4. PRs with [`Ups for grabs`](https://github.com/DotNetDiag/HealthChecks/labels/Ups%20for%20grabs)
and [help wanted](https://github.com/DotNetDiag/HealthChecks/labels/help%20wanted) tags are good candidates to contribute.
