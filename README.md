# Road Registry [![Build Status](https://github.com/Informatievlaanderen/road-registry/workflows/Build/badge.svg)](https://github.com/Informatievlaanderen/road-registry/actions)

The road registry is a mid-scale reference database of all Flemish roads. This code base supports

- back office tasks, such as allowing operators to download a snapshot of the registry as shape files and to upload changes to the registry
- exposing the road registry as a product, an OSLO compliant product, a snapshot for shape editing purposes

## Hosts

### Editor Projection Host

This host projects the entire event stream into a series of shape and dbase records which can be composed into shape files for editing purposes, i.e. what the operator needs to do his / her work and ultimately what is part of the download.

### Product Projection Host

This host projects the entire event stream into a series of shape and dbase records which can be composed into shape files for product release purposes, i.e. what third party consumers need.

### Sync Host

This host projects the municipality and street name registry streams into a cached shape that can be used to enrich road registry entities with municipality, street names and organizations.

### BackOffice Event Host

This host reacts to things happening in the entire event stream, selectively choosing if, when and what to do.
When the `CompletedRoadNetworkImport` event is observed, this host will create a snapshot of the road network and store it as a blob. This will speed up access to the road network.
When the `RoadNetworkChangesArchiveAccepted` event is observed, this host will translate the changes into a command that can be picked up and handled by the BackOffice Command Host.

### BackOffice Extract Host

This host reacts to things happening in the entire event stream, selectively choosing if, when and what to do.
The reason to keep it separate from the above event host is to guarantee throughput, to isolate dependencies, to be able to tune memory and cpu for the task at hand.  
When the `RoadNetworkExtractGotRequested` event is observed, this host will assemble an extract based on the requested contour, upload it and announce that the extract became available.

### BackOffice Command Host

This host react to things happening in the `roadnetwork-command-queue` event stream.
When the `ChangeRoadNetworkBasedOnArchive` command is observed, this host will try to merge the requested changes into the golden copy of the road network.
When the `AnnounceRoadNetworkExtractBecameAvailable` command is observed, this host will tell the request to announce that the extract became available.

### BackOffice API

This host exposes a private API to be used by the BackOffice UI.
It allows an operator to download, upload, get information about the registry and its recent changes.

### BackOffice UI

This host exposes the website the operator can interact with to download, upload, and view information about the registry and its recent changes.

### WMS Projection Host

This host projects the entire event stream to the WMS database to be used in the WMS service and OGC host.

### WFS Projection Host

This host projects the entire event stream to the WFS database to be used in the WFS service and OGC host.

### Integration Projection Host

This host projects the entire event stream to the Integration database.

### Admin Host

This host is a scheduled task with the purpose of running long-running commands.

### Projector

This host runs the Marten projections and exposes their status and management endpoints under `projections`.

#### Shadow rebuild and promote

The PBS (`RoadRegistryPbs`) and WMS/WFS V2 (`road`) read models can be rebuilt without taking them down. A shadow copy of the projection replays the event stream from the start into a schema of its own (`RoadRegistryPbsTemp` / `roadTemp`), while the live projection keeps serving. When it is done, the shadow read model is promoted and becomes the live one.

| Read model | Shadow switch | Live projection | Shadow projection |
|---|---|---|---|
| PBS | `Projections:Pbs:Shadow:Enabled` | `RoadNetworkChangesPbsProjection` | `RoadNetworkChangesPbsTempProjection` |
| WMS/WFS V2 | `Projections:WmsWfsV2:Shadow:Enabled` | `RoadNetworkChangesWmsWfsV2Projection` | `RoadNetworkChangesWmsWfsV2TempProjection` |

1. **Start the rebuild.** Set the shadow switch to `true` and deploy. On startup the projector creates the tables in the shadow schema, and the shadow projection starts replaying from the beginning of the event stream.
2. **Follow it.** `GET projections` lists the shadow projection with its position next to the store position. If a shadow rebuild went wrong, it can be started over with `POST projections/{shadow projection}/rebuild`.
3. **Stop the rebuild.** When you judge that the shadow got far enough, set the shadow switch back to `false` and deploy. It does not have to be fully caught up: whatever it is short of, the live projection catches up on after the promotion.
4. **Promote.** Call `POST projections/{id}/promote` with the live or the shadow projection id, for example `POST projections/RoadNetworkChangesPbsProjection/promote`. The call answers when the promotion is done:
   - `400` when the shadow projection is still switched on, or when the shadow schema has no tables, no position or no Marten progression.
   - `404` when the read model is not enabled on this host, `409` when another promotion or the PBS backfill is running.
   - `200` once the shadow read model is the live one.

What a promotion does:

- It stops the live projection, using the same drain as `projections/{id}/stop` (`drainSeconds`, 30 by default).
- It rebuilds any disabled shadow indexes, and copies the indexes the migrations added to the live tables in raw SQL (the spatial indexes, among others) to the shadow tables.
- In one transaction, it drops the live tables, moves the shadow tables into the live schema (`ALTER SCHEMA ... TRANSFER`), drops the shadow schema, and puts back the schema-bound views and the permissions.
- It hands the shadow's positions to the live projection: the SQL Server position, the Marten shard progression and the per-correlation progressions.
- It starts the live projection again, unless it was meant to be stopped. The live projection catches up from the position the shadow reached.

If a promotion fails before the swap, the live read model is unchanged and the live projection goes back to what it was doing. If it fails after the swap but before the Marten progressions are moved, the live projection stays stopped. Calling the promote again, or restarting the host, finishes the promotion.

## Docker based local setup

### What do I need to install?

- docker
- docker-compose

### Overview

Blobs that usually end up in AWS S3 will in this environment be stored in a Minio container (an S3 compatible, docker-based blob store).
Data that ends up or originates from AWS RDS SQL Server will in this environment be stored in a SQL Server container (multiple). 
Logging that usually ends up in DataDog will in this environment be accessible via the Seq container (Seq is a docker-based log server).
