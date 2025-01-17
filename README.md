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

## Docker based local setup

### What do I need to install?

- docker
- docker-compose

### Overview

Blobs that usually end up in AWS S3 will in this environment be stored in a Minio container (an S3 compatible, docker-based blob store).
Data that ends up or originates from AWS RDS SQL Server will in this environment be stored in a SQL Server container (multiple).
Logging that usually ends up in DataDog will in this environment be accessible via the Seq container (Seq is a docker-based log server).
