# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Road Registry is a mid-scale reference database of all Flemish roads, built on **event-driven CQRS** with multiple specialized hosts. The primary language is **C# (.NET 9)**.

## Build & Development Commands

### Prerequisites
```bash
./pre-restore.sh          # Run first to set up prerequisites
dotnet tool restore       # Restore .NET CLI tools
dotnet restore            # Restore NuGet packages
```

### Build
```bash
dotnet build --nologo --no-restore --configuration Debug RoadRegistry.sln
```

### Test
```bash
# All tests excluding integration tests (fast)
dotnet test --nologo --no-build --filter 'FullyQualifiedName!~IntegrationTests' RoadRegistry.sln

# All tests including integration tests
dotnet test --nologo --no-build RoadRegistry.sln

# Single test project
dotnet test --nologo --no-build path/to/TestProject.csproj
```

### Local Infrastructure
```bash
# Start all required services (SQL Server, PostgreSQL+PostGIS, MinIO, Seq, Ticketing)
docker-compose -f ./docker-compose.yml up --remove-orphans --force-recreate
# or
./run.sh
```

**Local infrastructure services:**
- **SQL Server 2019** on port 21433 (password: `E@syP@ssw0rd`)
- **PostgreSQL 15 + PostGIS 3** on port 29050 (for Marten event store)
- **MinIO** on ports 9010/9011 (replaces AWS S3)
- **Seq** on port 5341 (replaces Datadog for logging)
- **Ticketing service** on port 9100

## Architecture

### Event-Driven CQRS Pattern
The system uses an event stream as the source of truth. Multiple projection hosts consume this stream to build specialized read models. The `BackOffice Command Host` is the only writer to the road network golden copy.

**Write path:** BackOffice API → SQS → Lambda → Command Host → Event Stream

**Read path:** Event Stream → Projection Hosts → SQL Server read databases → APIs/Services

### Key Technologies
- **Event Store**: Marten (PostgreSQL) + SqlStreamStore
- **Messaging**: AWS SQS, Kafka, MediatR
- **Spatial data**: NetTopologySuite, DotSpatial.Projections, Shaperon (shapefiles)
- **Databases**: SQL Server (read models), PostgreSQL (event store)
- **Serverless**: AWS Lambda for SQS message handling
- **DI**: Autofac
- **Testing**: xUnit, AutoFixture, FluentAssertions

### Host Responsibilities

| Host | Responsibility |
|------|---------------|
| `BackOffice.CommandHost` | Merges changes into the road network golden copy |
| `BackOffice.EventHost` | Creates snapshots, translates archive events to commands |
| `BackOffice.ExtractHost` | Assembles and uploads road network extracts |
| `BackOffice.Api` | Private REST API for BackOffice UI |
| `Editor.ProjectionHost` | Projects events → shape/dbase records for operator editing |
| `Product.ProjectionHost` | Projects events → shape files for third-party consumers |
| `Wms/Wfs.ProjectionHost` | OGC web services (WMS/WFS) |
| `Integration.ProjectionHost` | Integration database projections |
| `SyncHost` | Caches municipality, street name, and organization data |
| `AdminHost` | Scheduled long-running tasks |

### Source Layout
- `src/` — All production code (C# projects)
- `test/` — All test projects (35+ projects, mirroring src structure)
- `docs/` — Architecture docs, ADRs, Structurizr C4 diagrams
- `docker/` — Docker configuration files

### Project Naming Convention
Each domain area follows the pattern:
- `RoadRegistry.<Area>` — Domain/business logic
- `RoadRegistry.<Area>.Schema` — EF Core database models
- `RoadRegistry.<Area>.ProjectionHost` — Projection host executable
- `RoadRegistry.<Area>.Handlers.Sqs.Lambda` — AWS Lambda handlers

### Domain Model
Core domain lives in `src/RoadRegistry/` with aggregates for `RoadNode`, `RoadSegment`, `GradeSeparatedJunction`, and the enclosing `RoadNetwork`. The domain uses event sourcing via `AggregateSource`.

## Commit Convention
Commits must follow Conventional Commits format (enforced by commitlint + Husky hooks). Use `npm run commit` (commitizen) for interactive commit creation.
