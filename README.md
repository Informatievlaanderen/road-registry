# Road Registry [![Build Status](https://github.com/Informatievlaanderen/road-registry/workflows/CI/badge.svg)](https://github.com/Informatievlaanderen/road-registry/actions)

The road registry is a mid-scale reference database of all Flemish roads. This code base supports

- back office tasks, such as allowing operators to download a snapshot of the registry as shape files and to upload changes to the registry
- exposing the road registry as a product, an OSLO compliant product, a snapshot for shape editing purposes

## Hosts

### Extract Legacy

The extract legacy program derives streams and events from the existing, legacy road registry database.
It stores its output into a blob called `import-streams.zip`, where the blob could be stored on the file system or in an AWS S3 bucket. This can be configured.
The blob contains one file called `streams.json`. This file is a (very long) array of `(stream, event)` tuples.
The order in which events appear in the file is the order they must be appended into the target event store.
Typically, it will start with a `BeganRoadNetworkImport` and end with a `CompletedRoadNetworkImport` on the `roadnetwork` stream.
In between all organizations, municipalities, road nodes, road segments, and grade separated junctions will be imported as well.
Each `organization` gets a stream of its own. Each `municipality` gets a stream of its own. Road nodes, segments, and grade separated junctions go into the `roadnetwork` stream.

### Import Legacy

The import legacy program takes the output of the extract legacy program and appends it to the various streams mentioned in the `streams.json` file, contained in the `import-streams.zip`.

### Editor Projection Host

This host projects the entire event stream into a series of shape and dbase records which can be composed into shape files for editing purposes, i.e. what the operator needs to do his / her work and ultimately what is part of the download.

### Product Projection Host

This host projects the entire event stream into a series of shape and dbase records which can be composed into shape files for product release purposes, i.e. what third party consumers need.

### Syndication Projection Host

This host projects the municipality and street name registry streams into a cached shape that can be used to enrich road registry entities with municipality and street names.

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

## Docker Based Integration Testing

### What do I need to install?

- docker
- docker-compose

### Overview

Please make sure you've ran `./build.sh publish` so that the self-contained .NET Core apps and their docker files are published (into the `dist` folder)
and ready to be consumed.

Blobs that usually end up in AWS S3 will in this environment be stored in a Localstack S3 container (an S3 compatible, docker-based blob store).
Data that ends up or originates from AWS RDS SQL Server will in this environment be stored in a SQL Server container (multiple).
Logging that usually ends up in DataDog will in this environment be accessible via the Seq container (Seq is a docker-based log server).

You will need a backup of the existing legacy database and put it into `src/RoadRegistry.LegacyDatabase/filled/legacydb.bak` or `src/RoadRegistry.LegacyDatabase/empty/legacydb.bak`.
The former is meant to represent a master copy of the legacy road registry database, the latter a stripped down version with barely any data. They will have to prove their usefulness over time.

To seed the legacy database you can type

`docker-compose up --build empty-legacy-mssql-seed`

or

`docker-compose up --build filled-legacy-mssql-seed`

To test whether the extraction works you can type

`docker-compose up --build extract-empty-legacy`

or

`docker-compose up --build extract-filled-legacy`

To test whether the import works you can type

`docker-compose up --build import-legacy`

To test whether the editor projection host works you can type

`docker-compose up --build editor-projection-host`

To test whether the product projection host works you can type

`docker-compose up --build product-projection-host`

To test whether the backoffice event host works you can type

`docker-compose up --build backoffice-event-host`

To test whether the backoffice command host works you can type

`docker-compose up --build backoffice-command-host`

To test whether the backoffice api and ui work you can type
docker-om
`docker-compose up --build backoffice-api backoffice-ui`

Several projections use a cache called the syndication projections. 
These are built from the streetname and municipality registries, using their respective syndication feeds.
You will need a copy of the Municipality Syndication table from the municipality registry

`bcp MunicipalityRegistryLegacy.MunicipalitySyndication out ./municipality-syndication.bcp -S<host>,<port> -U <user> -d municipality-registry -n -E`

and a copy of the StreetName Syndication table from the street name registry

`bcp StreetNameRegistryLegacy.StreetNameSyndication out ./streetname-syndication.bcp -S<host>,<port> -U <user> -d streetname-registry -n -E`

and copy these to `src/RoadRegistry.MunicipalityDatabase/filled` and `src/RoadRegistry.StreetNameDatabase/filled` respectively.

```
cp ./municipality-syndication.bcp src/RoadRegistry.MunicipalityDatabase/filled/syndication.bcp
cp ./streetname-syndication.bcp src/RoadRegistry.StreetNameDatabase/filled/syndication.bcp

```

To seed the database for these syndication feeds you can type

`docker-compose up --build filled-streetname-mssql-seed`

and

`docker-compose up --build filled-municipality-mssql-seed`

To test whether building the syndication projection works you can type

`docker-compose up --build syndication-projection-host`

