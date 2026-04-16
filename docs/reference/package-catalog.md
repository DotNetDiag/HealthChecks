---
title: Package Catalog
permalink: /reference/package-catalog/
---

Use this chapter to choose the right package family first, then open the [package references]({{ '/reference/readmes/' | relative_url }}) for package-specific defaults and code samples.

## Naming convention

- Health check packages use the `DotNetDiag.HealthChecks.<Provider>` convention.
- UI packages use the `DotNetDiag.HealthChecks.UI.*` convention and are covered in the [UI manual]({{ '/reference/ui-manual/' | relative_url }}).
- Publisher packages use the `DotNetDiag.HealthChecks.Publisher.*` convention and are covered in [publishers and metrics]({{ '/reference/publishers-and-metrics/' | relative_url }}).

## Core and platform checks

- [Application Status]({{ '/reference/readmes/src/HealthChecks-ApplicationStatus/' | relative_url }})
- [OpenID Connect Server]({{ '/reference/readmes/src/HealthChecks-OpenIdConnectServer/' | relative_url }})
- `DotNetDiag.HealthChecks.Kubernetes`
- `DotNetDiag.HealthChecks.Network`
- `DotNetDiag.HealthChecks.SignalR`
- `DotNetDiag.HealthChecks.System`
- `DotNetDiag.HealthChecks.Uris`

## AWS packages

- [Amazon S3]({{ '/reference/readmes/src/HealthChecks-Aws-S3/' | relative_url }})
- [AWS Secrets Manager]({{ '/reference/readmes/src/HealthChecks-Aws-SecretsManager/' | relative_url }})
- [AWS SNS]({{ '/reference/readmes/src/HealthChecks-Aws-Sns/' | relative_url }})
- [AWS SQS]({{ '/reference/readmes/src/HealthChecks-Aws-Sqs/' | relative_url }})
- [AWS Systems Manager]({{ '/reference/readmes/src/HealthChecks-Aws-SystemsManager/' | relative_url }})
- [Amazon DynamoDB]({{ '/reference/readmes/src/HealthChecks-DynamoDb/' | relative_url }})

## Azure packages

- [Azure Application Insights]({{ '/reference/readmes/src/HealthChecks-AzureApplicationInsights/' | relative_url }})
- [Azure Data Tables]({{ '/reference/readmes/src/HealthChecks-Azure-Data-Tables/' | relative_url }})
- [Azure IoT Hub]({{ '/reference/readmes/src/HealthChecks-Azure-IoTHub/' | relative_url }})
- [Azure Key Vault Secrets]({{ '/reference/readmes/src/HealthChecks-Azure-KeyVault-Secrets/' | relative_url }})
- [Azure Event Hubs]({{ '/reference/readmes/src/HealthChecks-Azure-Messaging-EventHubs/' | relative_url }})
- [Azure Blob Storage]({{ '/reference/readmes/src/HealthChecks-Azure-Storage-Blobs/' | relative_url }})
- [Azure File Shares]({{ '/reference/readmes/src/HealthChecks-Azure-Storage-Files-Shares/' | relative_url }})
- [Azure Queue Storage]({{ '/reference/readmes/src/HealthChecks-Azure-Storage-Queues/' | relative_url }})
- [Azure Digital Twin]({{ '/reference/readmes/src/HealthChecks-AzureDigitalTwin/' | relative_url }})
- [Azure Key Vault]({{ '/reference/readmes/src/HealthChecks-AzureKeyVault/' | relative_url }})
- [Azure Service Bus]({{ '/reference/readmes/src/HealthChecks-AzureServiceBus/' | relative_url }})
- `DotNetDiag.HealthChecks.AzureSearch`

## Databases, search, and messaging systems

- [ArangoDb]({{ '/reference/readmes/src/HealthChecks-ArangoDb/' | relative_url }})
- [ClickHouse]({{ '/reference/readmes/src/HealthChecks-ClickHouse/' | relative_url }})
- [Cosmos DB]({{ '/reference/readmes/src/HealthChecks-CosmosDb/' | relative_url }})
- `DotNetDiag.HealthChecks.Dapr`
- `DotNetDiag.HealthChecks.Elasticsearch`
- `DotNetDiag.HealthChecks.EventStore`
- `DotNetDiag.HealthChecks.EventStore.gRPC`
- `DotNetDiag.HealthChecks.Gcp.CloudFirestore`
- `DotNetDiag.HealthChecks.Gremlin`
- `DotNetDiag.HealthChecks.Hangfire`
- [IbmMQ]({{ '/reference/readmes/src/HealthChecks-IbmMQ/' | relative_url }})
- [InfluxDB]({{ '/reference/readmes/src/HealthChecks-InfluxDB/' | relative_url }})
- `DotNetDiag.HealthChecks.Kafka`
- `DotNetDiag.HealthChecks.Milvus` (preview)
- [MongoDB]({{ '/reference/readmes/src/HealthChecks-MongoDb/' | relative_url }})
- [MySQL]({{ '/reference/readmes/src/HealthChecks-MySql/' | relative_url }})
- [NATS]({{ '/reference/readmes/src/HealthChecks-Nats/' | relative_url }})
- [PostgreSQL]({{ '/reference/readmes/src/HealthChecks-NpgSql/' | relative_url }})
- `DotNetDiag.HealthChecks.Oracle`
- `DotNetDiag.HealthChecks.Qdrant`
- [RabbitMQ]({{ '/reference/readmes/src/HealthChecks-Rabbitmq/' | relative_url }})
- [RabbitMQ v6]({{ '/reference/readmes/src/HealthChecks-Rabbitmq-v6/' | relative_url }})
- `DotNetDiag.HealthChecks.RavenDB`
- `DotNetDiag.HealthChecks.Redis`
- `DotNetDiag.HealthChecks.SendGrid`
- `DotNetDiag.HealthChecks.Solr`
- `DotNetDiag.HealthChecks.Sqlite`
- `DotNetDiag.HealthChecks.SqlServer`
- [SurrealDB]({{ '/reference/readmes/src/HealthChecks-SurrealDb/' | relative_url }})

## How to use this catalog effectively

- Start with this chapter when you are selecting a provider package.
- Open the [package references]({{ '/reference/readmes/' | relative_url }}) when you need overload-specific behavior such as credentials, region selection, or connection pooling.
- Use the [getting started]({{ '/reference/getting-started/' | relative_url }}) chapter for common registration patterns that apply across packages.
- Use the [UI manual]({{ '/reference/ui-manual/' | relative_url }}) and [publishers and metrics]({{ '/reference/publishers-and-metrics/' | relative_url }}) chapters for dashboard and observability components.
