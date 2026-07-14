namespace RoadRegistry.MartenMigration.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Extensions;
using GradeSeparatedJunction;
using Marten;
using NodaTime;
using NodaTime.Text;
using RoadNode;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using Dapper;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.ValueObjects;
using RoadSegment;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using ImportedGradeSeparatedJunction = GradeSeparatedJunction.Events.V1.ImportedGradeSeparatedJunction;
using ImportedRoadNode = RoadNode.Events.V1.ImportedRoadNode;
using ImportedRoadSegment = RoadSegment.Events.V1.ImportedRoadSegment;
using ImportedRoadSegmentEuropeanRoadAttribute = RoadSegment.Events.V1.ValueObjects.ImportedRoadSegmentEuropeanRoadAttribute;
using ImportedRoadSegmentLaneAttribute = RoadSegment.Events.V1.ValueObjects.ImportedRoadSegmentLaneAttribute;
using ImportedRoadSegmentNationalRoadAttribute = RoadSegment.Events.V1.ValueObjects.ImportedRoadSegmentNationalRoadAttribute;
using ImportedRoadSegmentNumberedRoadAttribute = RoadSegment.Events.V1.ValueObjects.ImportedRoadSegmentNumberedRoadAttribute;
using ImportedRoadSegmentSurfaceAttribute = RoadSegment.Events.V1.ValueObjects.ImportedRoadSegmentSurfaceAttribute;
using ImportedRoadSegmentWidthAttribute = RoadSegment.Events.V1.ValueObjects.ImportedRoadSegmentWidthAttribute;
using MaintenanceAuthority = RoadSegment.Events.V1.ValueObjects.MaintenanceAuthority;
using RoadSegmentLaneAttributes = RoadSegment.Events.V1.ValueObjects.RoadSegmentLaneAttributes;
using RoadSegmentSurfaceAttributes = RoadSegment.Events.V1.ValueObjects.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadSegment.Events.V1.ValueObjects.RoadSegmentWidthAttributes;
using RoadSegmentSideAttributes = RoadSegment.Events.V1.ValueObjects.RoadSegmentSideAttributes;
using Reason = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason;
using RoadSegmentStreetNamesChanged = RoadSegment.Events.V1.RoadSegmentStreetNamesChanged;

public class MartenMigrationProjection : ConnectedProjection<MartenMigrationContext>
{
    private readonly IDocumentStore _store;

    public MartenMigrationProjection(IDocumentStore store)
    {
        _store = store.ThrowIfNull();

        When<Envelope<BackOffice.Messages.ImportedRoadNode>>((_, envelope, token) =>
        {
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                var point = GeometryTranslator.Translate(envelope.Message.Geometry).ToRoadNodeGeometry();
                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.Origin.Since),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var roadNodeId = new RoadNodeId(envelope.Message.Id);
                var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
                var legacyEvent = new ImportedRoadNode
                {
                    Geometry = point,
                    RoadNodeId = envelope.Message.Id,
                    Origin = new()
                    {
                        Application = envelope.Message.Origin.Application,
                        Operator = envelope.Message.Origin.Operator,
                        Organization = envelope.Message.Origin.Organization,
                        OrganizationId = envelope.Message.Origin.OrganizationId,
                        Since = envelope.Message.Origin.Since,
                        TransactionId = envelope.Message.Origin.TransactionId,
                    },
                    Type = envelope.Message.Type,
                    Version = envelope.Message.Version,
                    When = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset(),
                    Provenance = new ProvenanceData(provenance)
                };
                session.Events.Append(streamKey, legacyEvent);

                var roadNode = RoadNode.CreateForMigration(
                    roadNodeId: roadNodeId,
                    geometry: point.EnsureLambert08()
                );
                session.Store(roadNode);
            }, token);
        });

        When<Envelope<BackOffice.Messages.ImportedRoadSegment>>((_, envelope, token) =>
        {
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var geometry = GeometryTranslator.Translate(envelope.Message.Geometry).ToRoadSegmentGeometry();
                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.RecordingDate),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                var roadSegmentId = new RoadSegmentId(envelope.Message.Id);
                var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
                var legacyEvent = new ImportedRoadSegment
                {
                    RoadSegmentId = roadSegmentId,
                    Geometry = geometry,
                    StartNodeId = envelope.Message.StartNodeId,
                    EndNodeId = envelope.Message.EndNodeId,
                    GeometryDrawMethod = envelope.Message.GeometryDrawMethod,
                    AccessRestriction = envelope.Message.AccessRestriction,
                    Category = envelope.Message.Category,
                    Morphology = envelope.Message.Morphology,
                    Status = envelope.Message.Status,
                    LeftSide = new()
                    {
                        Municipality = envelope.Message.LeftSide.Municipality,
                        MunicipalityNISCode = envelope.Message.LeftSide.MunicipalityNISCode,
                        StreetName = envelope.Message.LeftSide.StreetName,
                        StreetNameId = envelope.Message.LeftSide.StreetNameId
                    },
                    RightSide = new()
                    {
                        Municipality = envelope.Message.RightSide.Municipality,
                        MunicipalityNISCode = envelope.Message.RightSide.MunicipalityNISCode,
                        StreetName = envelope.Message.RightSide.StreetName,
                        StreetNameId = envelope.Message.RightSide.StreetNameId
                    },
                    MaintenanceAuthority = new()
                    {
                        Code = envelope.Message.MaintenanceAuthority.Code,
                        Name = envelope.Message.MaintenanceAuthority.Name
                    },
                    PartOfEuropeanRoads = envelope.Message.PartOfEuropeanRoads
                        .Select(x => new ImportedRoadSegmentEuropeanRoadAttribute
                        {
                            Origin = new()
                            {
                                Application = x.Origin.Application,
                                Operator = x.Origin.Operator,
                                Organization = x.Origin.Organization,
                                OrganizationId = x.Origin.OrganizationId,
                                Since = x.Origin.Since,
                                TransactionId = x.Origin.TransactionId,
                            },
                            AttributeId = x.AttributeId,
                            Number = x.Number
                        })
                        .ToArray(),
                    PartOfNationalRoads = envelope.Message.PartOfNationalRoads
                        .Select(x => new ImportedRoadSegmentNationalRoadAttribute
                        {
                            Origin = new()
                            {
                                Application = x.Origin.Application,
                                Operator = x.Origin.Operator,
                                Organization = x.Origin.Organization,
                                OrganizationId = x.Origin.OrganizationId,
                                Since = x.Origin.Since,
                                TransactionId = x.Origin.TransactionId
                            },
                            AttributeId = x.AttributeId,
                            Number = x.Number
                        })
                        .ToArray(),
                    PartOfNumberedRoads = envelope.Message.PartOfNumberedRoads
                        .Select(x => new ImportedRoadSegmentNumberedRoadAttribute
                        {
                            Origin = new()
                            {
                                Application = x.Origin.Application,
                                Operator = x.Origin.Operator,
                                Organization = x.Origin.Organization,
                                OrganizationId = x.Origin.OrganizationId,
                                Since = x.Origin.Since,
                                TransactionId = x.Origin.TransactionId
                            },
                            AttributeId = x.AttributeId,
                            Number = x.Number,
                            Direction = x.Direction,
                            Ordinal = x.Ordinal
                        })
                        .ToArray(),
                    GeometryVersion = envelope.Message.GeometryVersion,
                    Version = envelope.Message.Version,
                    RecordingDate = envelope.Message.RecordingDate,
                    Lanes = envelope.Message.Lanes.Select(x => new ImportedRoadSegmentLaneAttribute
                    {
                        AsOfGeometryVersion = x.AsOfGeometryVersion,
                        AttributeId = x.AttributeId,
                        Count = x.Count,
                        Direction = x.Direction,
                        FromPosition = Convert.ToDouble(x.FromPosition),
                        ToPosition = Convert.ToDouble(x.ToPosition),
                        Origin = new()
                        {
                            Application = x.Origin.Application,
                            Operator = x.Origin.Operator,
                            Organization = x.Origin.Organization,
                            OrganizationId = x.Origin.OrganizationId,
                            Since = x.Origin.Since,
                            TransactionId = x.Origin.TransactionId
                        }
                    }).ToArray(),
                    Surfaces = envelope.Message.Surfaces.Select(x => new ImportedRoadSegmentSurfaceAttribute
                    {
                        AsOfGeometryVersion = x.AsOfGeometryVersion,
                        AttributeId = x.AttributeId,
                        Type = x.Type,
                        FromPosition = Convert.ToDouble(x.FromPosition),
                        ToPosition = Convert.ToDouble(x.ToPosition),
                        Origin = new()
                        {
                            Application = x.Origin.Application,
                            Operator = x.Origin.Operator,
                            Organization = x.Origin.Organization,
                            OrganizationId = x.Origin.OrganizationId,
                            Since = x.Origin.Since,
                            TransactionId = x.Origin.TransactionId
                        }
                    }).ToArray(),
                    Widths = envelope.Message.Widths.Select(x => new ImportedRoadSegmentWidthAttribute
                    {
                        AsOfGeometryVersion = x.AsOfGeometryVersion,
                        AttributeId = x.AttributeId,
                        Width = x.Width,
                        FromPosition = Convert.ToDouble(x.FromPosition),
                        ToPosition = Convert.ToDouble(x.ToPosition),
                        Origin = new()
                        {
                            Application = x.Origin.Application,
                            Operator = x.Origin.Operator,
                            Organization = x.Origin.Organization,
                            OrganizationId = x.Origin.OrganizationId,
                            Since = x.Origin.Since,
                            TransactionId = x.Origin.TransactionId
                        }
                    }).ToArray(),
                    When = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset(),
                    Origin = new()
                    {
                        Application = envelope.Message.Origin.Application,
                        Operator = envelope.Message.Origin.Operator,
                        Organization = envelope.Message.Origin.Organization,
                        OrganizationId = envelope.Message.Origin.OrganizationId,
                        Since = envelope.Message.Origin.Since,
                        TransactionId = envelope.Message.Origin.TransactionId,
                    },
                    Provenance = new ProvenanceData(provenance)
                };
                session.Events.Append(streamKey, legacyEvent);

                var roadSegment = RoadSegment.CreateForMigration(
                    roadSegmentId: roadSegmentId,
                    geometry: geometry.EnsureLambert08(),
                    status: RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod) == RoadSegmentGeometryDrawMethod.Outlined ? RoadSegmentStatusV2.Gepland : RoadSegmentStatusV2.Gerealiseerd,
                    startNodeId: envelope.Message.StartNodeId > 0 ? new RoadNodeId(envelope.Message.StartNodeId) : null,
                    endNodeId: envelope.Message.EndNodeId > 0 ? new RoadNodeId(envelope.Message.EndNodeId) : null
                );
                session.Store(roadSegment);
            }, token);
        });

        When<Envelope<BackOffice.Messages.ImportedGradeSeparatedJunction>>((_, envelope, token) =>
        {
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, async session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.Origin.Since),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                var junctionId = new GradeSeparatedJunctionId(envelope.Message.Id);
                var lowerRoadSegmentId = new RoadSegmentId(envelope.Message.LowerRoadSegmentId);
                var upperRoadSegmentId = new RoadSegmentId(envelope.Message.UpperRoadSegmentId);
                var geometry = await CalculateJunctionGeometry(session, lowerRoadSegmentId, upperRoadSegmentId, token);

                var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junctionId);
                var legacyEvent = new ImportedGradeSeparatedJunction
                {
                    Id = envelope.Message.Id,
                    LowerRoadSegmentId = envelope.Message.LowerRoadSegmentId,
                    Type = envelope.Message.Type,
                    UpperRoadSegmentId = envelope.Message.UpperRoadSegmentId,
                    Geometry = geometry?.EnsureLambert72(),
                    Origin = new()
                    {
                        Application = envelope.Message.Origin.Application,
                        Operator = envelope.Message.Origin.Operator,
                        Organization = envelope.Message.Origin.Organization,
                        OrganizationId = envelope.Message.Origin.OrganizationId,
                        Since = envelope.Message.Origin.Since,
                        TransactionId = envelope.Message.Origin.TransactionId,
                    },
                    When = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset(),
                    Provenance = new ProvenanceData(provenance)
                };
                session.Events.Append(streamKey, legacyEvent);

                var junction = GradeSeparatedJunction.CreateForMigration(
                    gradeSeparatedJunctionId: junctionId,
                    lowerRoadSegmentId: lowerRoadSegmentId,
                    upperRoadSegmentId: upperRoadSegmentId,
                    geometry: geometry?.EnsureLambert08());
                session.Store(junction);
            }, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (_, envelope, token) =>
        {
            var roadNetworkId = new ScopedRoadNetworkId(envelope.Message.DownloadId ?? Guid.NewGuid());

            foreach (var (message, changeIndex) in envelope.Message.Changes.Select((x, i) => (x.Flatten(), i)))
                switch (message)
                {
                    case RoadNodeAdded change:
                        await AddRoadNode(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadNodeModified change:
                        await ModifyRoadNode(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadNodeRemoved change:
                        await RemoveRoadNode(roadNetworkId, change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAdded change:
                        await AddRoadSegment(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentModified change:
                        await ModifyRoadSegment(roadNetworkId, change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAddedToEuropeanRoad change:
                        await AddRoadSegmentToEuropeanRoad(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad change:
                        await RemoveRoadSegmentFromEuropeanRoad(roadNetworkId, change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAddedToNationalRoad change:
                        await AddRoadSegmentToNationalRoad(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentRemovedFromNationalRoad change:
                        await RemoveRoadSegmentFromNationalRoad(roadNetworkId, change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAddedToNumberedRoad change:
                        await AddRoadSegmentToNumberedRoad(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad change:
                        await RemoveRoadSegmentFromNumberedRoad(roadNetworkId, change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAttributesModified change:
                        await ModifyRoadSegmentAttributes(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentGeometryModified change:
                        await ModifyRoadSegmentGeometry(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentRemoved change:
                        await RemoveRoadSegment(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case OutlinedRoadSegmentRemoved change:
                        await RemoveOutlinedRoadSegment(roadNetworkId, change, changeIndex, envelope, token);
                        break;

                    case GradeSeparatedJunctionAdded change:
                        await AddGradeSeparatedJunction(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case GradeSeparatedJunctionModified change:
                        await ModifyGradeSeparatedJunction(roadNetworkId, change, changeIndex, envelope, token);
                        break;
                    case GradeSeparatedJunctionRemoved change:
                        await RemoveGradeSeparatedJunction(roadNetworkId, change, changeIndex, envelope, token);
                        break;

                    default:
                        throw new NotImplementedException($"Unknown change type {message.GetType()}");
                }

            await AcceptRoadNetworkChange(roadNetworkId, envelope, token);
        });

        When<Envelope<BackOffice.Messages.ImportedOrganization>>((_, envelope, token) =>
        {
            var organizationId = new OrganizationId(envelope.Message.Code);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var legacyEvent = new OrganizationWasImported
                {
                    OrganizationId = organizationId,
                    Name = envelope.Message.Name,
                    Provenance = new ProvenanceData(BuildOrganizationProvenance(envelope.Message.When, Modification.Insert))
                };
                session.Events.Append(OrganizationStreamKey(organizationId), legacyEvent);
            }, token);
        });
        When<Envelope<BackOffice.Messages.CreateOrganizationAccepted>>((_, envelope, token) =>
        {
            var organizationId = new OrganizationId(envelope.Message.Code);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new OrganizationWasCreated
                {
                    OrganizationId = organizationId,
                    Name = envelope.Message.Name,
                    OvoCode = envelope.Message.OvoCode,
                    KboNumber = envelope.Message.KboNumber,
                    Provenance = new ProvenanceData(BuildOrganizationProvenance(envelope.Message.When, Modification.Insert))
                };
                session.Events.Append(OrganizationStreamKey(organizationId), v2Event);
            }, token);
        });
        When<Envelope<BackOffice.Messages.ChangeOrganizationAccepted>>((_, envelope, token) =>
        {
            var organizationId = new OrganizationId(envelope.Message.Code);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new OrganizationWasModified
                {
                    OrganizationId = organizationId,
                    Name = envelope.Message.NameModified ? envelope.Message.Name : null,
                    OvoCode = envelope.Message.OvoCodeModified ? envelope.Message.OvoCode : null,
                    KboNumber = envelope.Message.KboNumberModified ? envelope.Message.KboNumber : null,
                    IsMaintainer = envelope.Message.IsMaintainerModified ? envelope.Message.IsMaintainer : null,
                    Provenance = new ProvenanceData(BuildOrganizationProvenance(envelope.Message.When, Modification.Update))
                };
                session.Events.Append(OrganizationStreamKey(organizationId), v2Event);
            }, token);
        });
        When<Envelope<BackOffice.Messages.RenameOrganizationAccepted>>((_, envelope, token) =>
        {
            var organizationId = new OrganizationId(envelope.Message.Code);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new OrganizationWasModified
                {
                    OrganizationId = organizationId,
                    Name = envelope.Message.Name,
                    Provenance = new ProvenanceData(BuildOrganizationProvenance(envelope.Message.When, Modification.Update))
                };
                session.Events.Append(OrganizationStreamKey(organizationId), v2Event);
            }, token);
        });
        When<Envelope<BackOffice.Messages.DeleteOrganizationAccepted>>((_, envelope, token) =>
        {
            var organizationId = new OrganizationId(envelope.Message.Code);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new OrganizationWasRemoved
                {
                    OrganizationId = organizationId,
                    Provenance = new ProvenanceData(BuildOrganizationProvenance(envelope.Message.When, Modification.Delete))
                };
                session.Events.Append(OrganizationStreamKey(organizationId), v2Event);
            }, token);
        });

        When<Envelope<RoadSegmentsStreetNamesChanged>>(async (_, envelope, token) =>
        {
            var roadNetworkId = new ScopedRoadNetworkId(Guid.NewGuid());

            await RoadSegmentsStreetNamesChanged(roadNetworkId, envelope, token);
        });

        When<Envelope<BackOffice.Messages.StreetNameCreated>>((_, envelope, token) =>
        {
            var streetNameLocalId = new StreetNameLocalId(envelope.Message.Record.PersistentLocalId);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new StreetNameWasCreated
                {
                    StreetNameId = streetNameLocalId,
                    DutchName = envelope.Message.Record.DutchName,
                    Provenance = new ProvenanceData(BuildStreetNameProvenance(envelope.Message.When, Modification.Insert))
                };
                session.Events.Append(StreetNameStreamKey(streetNameLocalId), v2Event);
            }, token);
        });

        When<Envelope<BackOffice.Messages.StreetNameModified>>((_, envelope, token) =>
        {
            if (!envelope.Message.NameModified && !envelope.Message.StatusModified)
            {
                return Task.CompletedTask;
            }

            var streetNameLocalId = new StreetNameLocalId(envelope.Message.Record.PersistentLocalId);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new StreetNameWasModified
                {
                    StreetNameId = streetNameLocalId,
                    DutchName = envelope.Message.Record.DutchName,
                    NisCode = envelope.Message.Record.NisCode,
                    Status = envelope.Message.Record.StreetNameStatus,
                    Provenance = new ProvenanceData(BuildStreetNameProvenance(envelope.Message.When, Modification.Update))
                };
                session.Events.Append(StreetNameStreamKey(streetNameLocalId), v2Event);
            }, token);
        });

        When<Envelope<BackOffice.Messages.StreetNameRemoved>>((_, envelope, token) =>
        {
            var streetNameLocalId = new StreetNameId(envelope.Message.StreetNameId).ToStreetNameLocalId();
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new StreetNameWasRemoved
                {
                    StreetNameId = streetNameLocalId,
                    Provenance = new ProvenanceData(BuildStreetNameProvenance(envelope.Message.When, Modification.Delete))
                };
                session.Events.Append(StreetNameStreamKey(streetNameLocalId), v2Event);
            }, token);
        });

        When<Envelope<BackOffice.Messages.StreetNameRenamed>>((_, envelope, token) =>
        {
            var streetNameLocalId = new StreetNameId(envelope.Message.StreetNameLocalId).ToStreetNameLocalId();
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

            return store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var v2Event = new StreetNameWasRenamed
                {
                    StreetNameId = streetNameLocalId,
                    DestinationStreetNameId = new StreetNameId(envelope.Message.DestinationStreetNameLocalId).ToStreetNameLocalId(),
                    Provenance = new ProvenanceData(BuildStreetNameProvenance(envelope.Message.When, Modification.Unknown))
                };
                session.Events.Append(StreetNameStreamKey(streetNameLocalId), v2Event);
            }, token);
        });

        // When<Envelope<BackOffice.Messages.ImportedMunicipality>>((_, envelope, token) =>
        // {
        //     return Task.CompletedTask;
        // });

/* Not migrating extract events
RoadNetworkChangesArchiveAccepted
RoadNetworkChangesArchiveFeatureCompareCompleted
RoadNetworkChangesArchiveUploaded
RoadNetworkExtractChangesArchiveAccepted
RoadNetworkExtractChangesArchiveFeatureCompareCompleted
RoadNetworkExtractChangesArchiveUploaded
RoadNetworkExtractClosed
RoadNetworkExtractDownloadBecameAvailable
RoadNetworkExtractDownloaded
RoadNetworkExtractGotRequested
RoadNetworkExtractGotRequestedV2
*/
    }

    private Task AcceptRoadNetworkChange(
        ScopedRoadNetworkId roadNetworkId,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope);

        return _store.IdempotentSession(idempotentSessionIdentifier, session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Unknown);

            var streamKey = StreamKeyFactory.Create(typeof(ScopedRoadNetwork), new ScopedRoadNetworkId(envelope.Message.DownloadId ?? Guid.NewGuid()));
            var legacyEvent = new RoadRegistry.ScopedRoadNetwork.Events.V1.RoadNetworkChangesAccepted
            {
                Operator = envelope.Message.Operator,
                Organization = envelope.Message.Organization,
                OrganizationId = envelope.Message.OrganizationId,
                Reason = envelope.Message.Reason,
                RequestId = envelope.Message.RequestId,
                DownloadId = envelope.Message.DownloadId,
                TransactionId = envelope.Message.TransactionId,
                TicketId = envelope.Message.TicketId,
                When = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset(),
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);
        }, token);
    }

    private Task AddRoadNode(
        ScopedRoadNetworkId roadNetworkId,
        RoadNodeAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var point = GeometryTranslator.Translate(change.Geometry).ToRoadNodeGeometry();
            var provenance = BuildProvenance(envelope, Modification.Insert);

            var roadNodeId = new RoadNodeId(change.Id);
            var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
            var legacyEvent = new RoadRegistry.RoadNode.Events.V1.RoadNodeAdded
            {
                Geometry = point,
                RoadNodeId = change.Id,
                Version = change.Version,
                TemporaryId = change.TemporaryId,
                OriginalId = change.OriginalId,
                Type = change.Type,
                Provenance = new ProvenanceData(provenance),
            };
            session.Events.Append(streamKey, legacyEvent);

            var roadNode = RoadNode.CreateForMigration(
                roadNodeId: roadNodeId,
                geometry: point.EnsureLambert08()
            );
            session.Store(roadNode);
        }, token);
    }

    private Task ModifyRoadNode(
        ScopedRoadNetworkId roadNetworkId,
        RoadNodeModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
            {
                session.CorrelationId = roadNetworkId;
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var point = GeometryTranslator.Translate(change.Geometry).ToRoadNodeGeometry();
                var provenance = BuildProvenance(envelope, Modification.Update);

                var roadNodeId = new RoadNodeId(change.Id);
                var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
                var legacyEvent = new RoadRegistry.RoadNode.Events.V1.RoadNodeModified
                {
                    Geometry = point,
                    RoadNodeId = change.Id,
                    Version = change.Version,
                    Type = change.Type,
                    Provenance = new ProvenanceData(provenance)
                };
                session.Events.Append(streamKey, legacyEvent);

                var roadNode = await session.LoadAsync(roadNodeId, cancellationToken: token)
                    ?? throw new InvalidOperationException($"Road node {change.Id} not found");
                roadNode.Apply(new RoadNodeWasModified
                {
                    RoadNodeId = roadNodeId,
                    Geometry = point.EnsureLambert08(),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            token);
    }

    private Task RemoveRoadNode(
        ScopedRoadNetworkId roadNetworkId,
        RoadNodeRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Delete);

            var roadNodeId = new RoadNodeId(change.Id);
            var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
            var legacyEvent = new RoadRegistry.RoadNode.Events.V1.RoadNodeRemoved
            {
                RoadNodeId = change.Id,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);

            var roadNode = await session.LoadAsync(roadNodeId, cancellationToken: token)
                           ?? throw new InvalidOperationException($"Road node {change.Id} not found");
            roadNode.Apply(new RoadNodeWasRemoved
            {
                RoadNodeId = roadNodeId,
                Provenance = new ProvenanceData(provenance)
            });
        }, token);
    }

    private Task AddRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var geometry = GeometryTranslator.Translate(change.Geometry).ToRoadSegmentGeometry();
            var provenance = BuildProvenance(envelope, Modification.Insert);

            var roadSegmentId = new RoadSegmentId(change.Id);
            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = new RoadRegistry.RoadSegment.Events.V1.RoadSegmentAdded
            {
                RoadSegmentId = change.Id,
                TemporaryId = change.TemporaryId,
                OriginalId = change.OriginalId,
                Geometry = geometry,
                StartNodeId = change.StartNodeId,
                EndNodeId = change.EndNodeId,
                GeometryDrawMethod = change.GeometryDrawMethod,
                AccessRestriction = change.AccessRestriction,
                Category = change.Category,
                Morphology = change.Morphology,
                Status = change.Status,
                LeftSide = new()
                {
                    StreetNameId = change.LeftSide.StreetNameId
                },
                RightSide = new()
                {
                    StreetNameId = change.RightSide.StreetNameId
                },
                MaintenanceAuthority = new()
                {
                    Code = change.MaintenanceAuthority.Code,
                    Name = change.MaintenanceAuthority.Name
                },
                GeometryVersion = change.GeometryVersion,
                Version = change.Version,
                Lanes = change.Lanes.Select(x => new RoadSegmentLaneAttributes
                    {
                        AsOfGeometryVersion = x.AsOfGeometryVersion,
                        AttributeId = x.AttributeId,
                        Count = x.Count,
                        Direction = x.Direction,
                        FromPosition = Convert.ToDouble(x.FromPosition),
                        ToPosition = Convert.ToDouble(x.ToPosition)
                    })
                    .ToArray(),
                Surfaces = change.Surfaces.Select(x => new RoadSegmentSurfaceAttributes
                    {
                        AsOfGeometryVersion = x.AsOfGeometryVersion,
                        AttributeId = x.AttributeId,
                        Type = x.Type,
                        FromPosition = Convert.ToDouble(x.FromPosition),
                        ToPosition = Convert.ToDouble(x.ToPosition)
                    })
                    .ToArray(),
                Widths = change.Widths.Select(x => new RoadSegmentWidthAttributes
                    {
                        AsOfGeometryVersion = x.AsOfGeometryVersion,
                        AttributeId = x.AttributeId,
                        Width = x.Width,
                        FromPosition = Convert.ToDouble(x.FromPosition),
                        ToPosition = Convert.ToDouble(x.ToPosition)
                    })
                    .ToArray(),
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);

            var roadSegment = RoadSegment.CreateForMigration(
                roadSegmentId: roadSegmentId,
                geometry: geometry.EnsureLambert08(),
                status: RoadSegmentGeometryDrawMethod.Parse(change.GeometryDrawMethod) == RoadSegmentGeometryDrawMethod.Outlined ? RoadSegmentStatusV2.Gepland : RoadSegmentStatusV2.Gerealiseerd,
                startNodeId: change.StartNodeId > 0 ? new RoadNodeId(change.StartNodeId) : null,
                endNodeId: change.EndNodeId > 0 ? new RoadNodeId(change.EndNodeId) : null
            );
            session.Store(roadSegment);
        }, token);
    }

    private async Task ModifyRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);
        var geometry = GeometryTranslator.Translate(change.Geometry).ToRoadSegmentGeometry();

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentModified
                {
                    RoadSegmentId = change.Id,
                    OriginalId = change.OriginalId,
                    ConvertedFromOutlined = change.ConvertedFromOutlined,
                    Geometry = geometry,
                    StartNodeId = change.StartNodeId,
                    EndNodeId = change.EndNodeId,
                    GeometryDrawMethod = change.GeometryDrawMethod,
                    AccessRestriction = change.AccessRestriction,
                    Category = change.Category,
                    Morphology = change.Morphology,
                    Status = change.Status,
                    LeftSide = new()
                    {
                        StreetNameId = change.LeftSide.StreetNameId
                    },
                    RightSide = new()
                    {
                        StreetNameId = change.RightSide.StreetNameId
                    },
                    MaintenanceAuthority = new()
                    {
                        Code = change.MaintenanceAuthority.Code,
                        Name = change.MaintenanceAuthority.Name
                    },
                    GeometryVersion = change.GeometryVersion,
                    Version = change.Version,
                    Lanes = change.Lanes.Select(x => new RoadSegmentLaneAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Count = x.Count,
                            Direction = x.Direction,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Surfaces = change.Surfaces.Select(x => new RoadSegmentSurfaceAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Type = x.Type,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Widths = change.Widths.Select(x => new RoadSegmentWidthAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Width = x.Width,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
                roadSegment.Apply(new RoadSegmentGeometryWasModified
                {
                    RoadSegmentId = roadSegmentId,
                    StartNodeId = change.StartNodeId > 0 ? new RoadNodeId(change.StartNodeId) : null,
                    EndNodeId = change.EndNodeId > 0 ? new RoadNodeId(change.EndNodeId) : null,
                    Geometry = geometry.EnsureLambert08(),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, changeIndex, token, recalculateJunctions: true);
    }

    private Task ModifyRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentId roadSegmentId,
        Func<Provenance, IMartenEvent> getLegacyEvent,
        Action<RoadSegment, Provenance> applyModification,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int changeIndex,
        CancellationToken token,
        bool recalculateJunctions = false)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Update);

            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = getLegacyEvent(provenance);
            session.Events.Append(streamKey, legacyEvent);

            var roadSegment = await session.LoadAsync(roadSegmentId, cancellationToken: token)
                              ?? throw new InvalidOperationException($"Road segment {roadSegmentId} not found");
            applyModification(roadSegment, provenance);
            session.Store(roadSegment);

            if (recalculateJunctions)
            {
                await RecalculateGradeSeparatedJunctionsForSegment(session, roadSegment, provenance, token);
            }
        }, token);
    }

    private static async Task RecalculateGradeSeparatedJunctionsForSegment(
        IDocumentSession session,
        RoadSegment changedSegment,
        Provenance provenance,
        CancellationToken token)
    {
        var junctionIds = (await session.Connection.QueryAsync<int>(new CommandDefinition(
            $"SELECT id FROM {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} WHERE lower_road_segment_id = @segId OR upper_road_segment_id = @segId",
            new { segId = changedSegment.RoadSegmentId.ToInt32() },
            cancellationToken: token))).ToArray();

        foreach (var junctionIdValue in junctionIds)
        {
            var junctionId = new GradeSeparatedJunctionId(junctionIdValue);
            var junction = await session.LoadAsync(junctionId, cancellationToken: token);
            if (junction is null || junction.IsRemoved)
            {
                continue;
            }

            // Use the changed segment's new in-memory geometry; load only the counterpart from the store.
            var counterpartId = junction.LowerRoadSegmentId == changedSegment.RoadSegmentId
                ? junction.UpperRoadSegmentId
                : junction.LowerRoadSegmentId;
            var counterpart = await session.LoadAsync(counterpartId, cancellationToken: token);
            if (counterpart is null)
            {
                continue;
            }

            var newGeometry = JunctionGeometryCalculator.Calculate(changedSegment.Geometry, counterpart.Geometry)?.EnsureLambert08();
            if (newGeometry is null || junction.Geometry == newGeometry)
            {
                continue;
            }

            // The stream carries the V1 event with the Lambert72 geometry (legacy CRS); the aggregate snapshot keeps
            // Lambert08 via the V2 apply (which is not appended to the stream).
            session.Events.Append(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junctionId),
                new RoadRegistry.GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionGeometryModified
                {
                    Id = junctionIdValue,
                    Geometry = newGeometry.EnsureLambert72(),
                    Provenance = new ProvenanceData(provenance)
                });
            junction.Apply(new GradeSeparatedJunctionGeometryWasChanged
            {
                GradeSeparatedJunctionId = junctionId,
                Geometry = newGeometry,
                Provenance = new ProvenanceData(provenance)
            });
            session.Store(junction);
        }
    }

    private async Task AddRoadSegmentToEuropeanRoad(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentAddedToEuropeanRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToEuropeanRoad
                {
                    AttributeId = change.AttributeId,
                    TemporaryAttributeId = change.TemporaryAttributeId,
                    RoadSegmentId = change.SegmentId,
                    RoadSegmentVersion = change.SegmentVersion,
                    Number = change.Number,
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
            },
            envelope, changeIndex, token);
    }

    private async Task RemoveRoadSegmentFromEuropeanRoad(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentRemovedFromEuropeanRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromEuropeanRoad
                {
                    AttributeId = change.AttributeId,
                    RoadSegmentId = change.SegmentId,
                    RoadSegmentVersion = change.SegmentVersion,
                    Number = change.Number,
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
            },
            envelope, changeIndex, token);
    }

    private async Task AddRoadSegmentToNationalRoad(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentAddedToNationalRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToNationalRoad
                {
                    AttributeId = change.AttributeId,
                    TemporaryAttributeId = change.TemporaryAttributeId,
                    RoadSegmentId = change.SegmentId,
                    RoadSegmentVersion = change.SegmentVersion,
                    Number = change.Number,
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
            },
            envelope, changeIndex, token);
    }

    private async Task RemoveRoadSegmentFromNationalRoad(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentRemovedFromNationalRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromNationalRoad
                {
                    AttributeId = change.AttributeId,
                    RoadSegmentId = change.SegmentId,
                    RoadSegmentVersion = change.SegmentVersion,
                    Number = change.Number,
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
            },
            envelope, changeIndex, token);
    }

    private async Task AddRoadSegmentToNumberedRoad(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentAddedToNumberedRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToNumberedRoad
                {
                    AttributeId = change.AttributeId,
                    TemporaryAttributeId = change.TemporaryAttributeId,
                    RoadSegmentId = change.SegmentId,
                    RoadSegmentVersion = change.SegmentVersion,
                    Number = change.Number,
                    Provenance = new ProvenanceData(provenance),
                    Direction = change.Direction,
                    Ordinal = change.Ordinal
                };
            },
            (roadSegment, provenance) =>
            {
            },
            envelope, changeIndex, token);
    }

    private async Task RemoveRoadSegmentFromNumberedRoad(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentRemovedFromNumberedRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromNumberedRoad
                {
                    AttributeId = change.AttributeId,
                    RoadSegmentId = change.SegmentId,
                    RoadSegmentVersion = change.SegmentVersion,
                    Number = change.Number,
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
            },
            envelope, changeIndex, token);
    }

    private async Task ModifyRoadSegmentAttributes(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentAttributesModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentAttributesModified
                {
                    RoadSegmentId = change.Id,
                    Version = change.Version,
                    AccessRestriction = change.AccessRestriction,
                    Category = change.Category,
                    Morphology = change.Morphology,
                    Status = change.Status,
                    LeftSide = change.LeftSide is not null ? new RoadSegmentSideAttributes
                    {
                        StreetNameId = change.LeftSide.StreetNameId
                    } : null,
                    RightSide = change.RightSide is not null ? new RoadSegmentSideAttributes
                    {
                        StreetNameId = change.RightSide.StreetNameId
                    } : null,
                    MaintenanceAuthority = change.MaintenanceAuthority is not null
                        ? new MaintenanceAuthority
                        {
                            Code = change.MaintenanceAuthority.Code,
                            Name = change.MaintenanceAuthority.Name
                        } : null,
                    Lanes = change.Lanes?.Select(x => new RoadSegmentLaneAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Count = x.Count,
                            Direction = x.Direction,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Surfaces = change.Surfaces?.Select(x => new RoadSegmentSurfaceAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Type = x.Type,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Widths = change.Widths?.Select(x => new RoadSegmentWidthAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Width = x.Width,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
            },
            envelope, changeIndex, token);
    }

    private async Task ModifyRoadSegmentGeometry(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentGeometryModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);
        var geometry = GeometryTranslator.Translate(change.Geometry).ToRoadSegmentGeometry();

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentGeometryModified
                {
                    RoadSegmentId = change.Id,
                    Version = change.Version,
                    GeometryVersion = change.GeometryVersion,
                    Geometry = geometry,
                    Lanes = change.Lanes.Select(x => new RoadSegmentLaneAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Count = x.Count,
                            Direction = x.Direction,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Surfaces = change.Surfaces.Select(x => new RoadSegmentSurfaceAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Type = x.Type,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Widths = change.Widths.Select(x => new RoadSegmentWidthAttributes
                        {
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            AttributeId = x.AttributeId,
                            Width = x.Width,
                            FromPosition = Convert.ToDouble(x.FromPosition),
                            ToPosition = Convert.ToDouble(x.ToPosition)
                        })
                        .ToArray(),
                    Provenance = new ProvenanceData(provenance)
                };
            },
            (roadSegment, provenance) =>
            {
                roadSegment.Apply(new RoadSegmentGeometryWasModified
                {
                    RoadSegmentId = roadSegmentId,
                    StartNodeId = roadSegment.StartNodeId > 0 ? roadSegment.StartNodeId : null,
                    EndNodeId = roadSegment.EndNodeId > 0 ? roadSegment.EndNodeId : null,
                    Geometry = geometry.EnsureLambert08(),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, changeIndex, token, recalculateJunctions: true);
    }

    private Task RemoveRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Delete);

            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = new RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemoved
            {
                RoadSegmentId = change.Id,
                GeometryDrawMethod = change.GeometryDrawMethod,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);

            var roadSegment = await session.LoadAsync(roadSegmentId, cancellationToken: token)
                              ?? throw new InvalidOperationException($"Road segment {change.Id} not found");
            roadSegment.Apply(new RoadSegmentWasRemoved
            {
                RoadSegmentId = roadSegmentId,
                Provenance = new ProvenanceData(provenance)
            });
            session.Store(roadSegment);
        }, token);
    }

    private Task RemoveOutlinedRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        OutlinedRoadSegmentRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Delete);

            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = new RoadRegistry.RoadSegment.Events.V1.OutlinedRoadSegmentRemoved
            {
                RoadSegmentId = change.Id,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);

            var roadSegment = await session.LoadAsync(roadSegmentId, cancellationToken: token)
                              ?? throw new InvalidOperationException($"Road segment {change.Id} not found");
            roadSegment.Apply(new RoadSegmentWasRemoved
            {
                RoadSegmentId = roadSegmentId,
                Provenance = new ProvenanceData(provenance)
            });
            session.Store(roadSegment);
        }, token);
    }

    // Loads the two linked road segments (already migrated earlier in the stream) and computes the junction point.
    // Null when either segment is missing or the segments do not intersect. The migration carries this on the V1 event
    // so the downstream projections read it without recomputing from segments.
    private static async Task<JunctionGeometry?> CalculateJunctionGeometry(
        IDocumentSession session,
        RoadSegmentId lowerRoadSegmentId,
        RoadSegmentId upperRoadSegmentId,
        CancellationToken token)
    {
        var lowerSegment = await session.LoadAsync(lowerRoadSegmentId, token);
        var upperSegment = await session.LoadAsync(upperRoadSegmentId, token);
        if (lowerSegment is null || upperSegment is null)
        {
            return null;
        }

        return JunctionGeometryCalculator.Calculate(lowerSegment.Geometry, upperSegment.Geometry)?.EnsureLambert08();
    }

    private Task AddGradeSeparatedJunction(
        ScopedRoadNetworkId roadNetworkId,
        GradeSeparatedJunctionAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Insert);

            var gradeSeparatedJunctionId = new GradeSeparatedJunctionId(change.Id);
            var lowerRoadSegmentId = new RoadSegmentId(change.LowerRoadSegmentId);
            var upperRoadSegmentId = new RoadSegmentId(change.UpperRoadSegmentId);
            var geometry = await CalculateJunctionGeometry(session, lowerRoadSegmentId, upperRoadSegmentId, token);

            var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionId);
            var legacyEvent = new RoadRegistry.GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionAdded
            {
                Id = change.Id,
                TemporaryId = change.TemporaryId,
                LowerRoadSegmentId = change.LowerRoadSegmentId,
                UpperRoadSegmentId = change.UpperRoadSegmentId,
                Type = change.Type,
                Geometry = geometry?.EnsureLambert72(),
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);

            var junction = GradeSeparatedJunction.CreateForMigration(
                gradeSeparatedJunctionId: gradeSeparatedJunctionId,
                lowerRoadSegmentId: lowerRoadSegmentId,
                upperRoadSegmentId: upperRoadSegmentId,
                geometry: geometry?.EnsureLambert08());
            session.Store(junction);
        }, token);
    }

    private Task RemoveGradeSeparatedJunction(
        ScopedRoadNetworkId roadNetworkId,
        GradeSeparatedJunctionRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Insert);

            var gradeSeparatedJunctionId = new GradeSeparatedJunctionId(change.Id);
            var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionId);
            var legacyEvent = new RoadRegistry.GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionRemoved
            {
                Id = change.Id,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);

            var gradeSeparatedJunction = await session.LoadAsync(gradeSeparatedJunctionId, cancellationToken: token)
                                           ?? throw new InvalidOperationException($"Grade separated junction {change.Id} not found");
            gradeSeparatedJunction.Apply(new GradeSeparatedJunctionWasRemoved
            {
                GradeSeparatedJunctionId = gradeSeparatedJunctionId,
                Provenance = new ProvenanceData(provenance)
            });
            session.Store(gradeSeparatedJunction);
        }, token);
    }

    private Task ModifyGradeSeparatedJunction(
        ScopedRoadNetworkId roadNetworkId,
        GradeSeparatedJunctionModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, changeIndex);

        return _store.IdempotentSession(idempotentSessionIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Insert);

            var gradeSeparatedJunctionId = new GradeSeparatedJunctionId(change.Id);
            var lowerRoadSegmentId = new RoadSegmentId(change.LowerRoadSegmentId);
            var upperRoadSegmentId = new RoadSegmentId(change.UpperRoadSegmentId);
            var geometry = await CalculateJunctionGeometry(session, lowerRoadSegmentId, upperRoadSegmentId, token);

            var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionId);
            var legacyEvent = new RoadRegistry.GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionModified
            {
                Id = change.Id,
                LowerRoadSegmentId = change.LowerRoadSegmentId,
                UpperRoadSegmentId = change.UpperRoadSegmentId,
                Type = change.Type,
                Geometry = geometry?.EnsureLambert72(),
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);

            var gradeSeparatedJunction = await session.LoadAsync(gradeSeparatedJunctionId, cancellationToken: token)
                                         ?? throw new InvalidOperationException($"Grade separated junction {change.Id} not found");
            gradeSeparatedJunction.Apply(new GradeSeparatedJunctionWasModified
            {
                GradeSeparatedJunctionId = gradeSeparatedJunctionId,
                LowerRoadSegmentId = lowerRoadSegmentId,
                UpperRoadSegmentId = upperRoadSegmentId,
                Provenance = new ProvenanceData(provenance)
            });
            // Keep the aggregate snapshot geometry (Lambert08) in sync with the (possibly changed) segments.
            if (geometry is not null && gradeSeparatedJunction.Geometry != geometry)
            {
                gradeSeparatedJunction.Apply(new GradeSeparatedJunctionGeometryWasChanged
                {
                    GradeSeparatedJunctionId = gradeSeparatedJunctionId,
                    Geometry = geometry,
                    Provenance = new ProvenanceData(provenance)
                });
            }
            session.Store(gradeSeparatedJunction);
        }, token);
    }

    private async Task RoadSegmentsStreetNamesChanged(
        ScopedRoadNetworkId roadNetworkId,
        Envelope<RoadSegmentsStreetNamesChanged> envelope,
        CancellationToken token)
    {
        // Only V1 road segments reach this handler (V2 segments are relinked directly via SQS from the consumer).
        foreach (var (change, index) in envelope.Message.RoadSegments.Select((x, i) => (x, i)))
        {
            var roadSegmentId = new RoadSegmentId(change.Id);
            var idempotentSessionIdentifier = BuildIdemponentSessionIdentifier(envelope, index);

            await _store.IdempotentSession(idempotentSessionIdentifier, session =>
            {
                session.CorrelationId = roadNetworkId;
                session.CausationId = $"migration-{envelope.EventName}-{idempotentSessionIdentifier}";

                var provenance = new Provenance(
                    LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    Application.RoadRegistry,
                    new Reason(envelope.Message.Reason),
                    new Operator(string.Empty),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen);

                var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
                var legacyEvent = new RoadSegmentStreetNamesChanged
                {
                    RoadSegmentId = change.Id,
                    Version = change.Version,
                    GeometryDrawMethod = change.GeometryDrawMethod,
                    LeftSideStreetNameId = change.LeftSideStreetNameId,
                    RightSideStreetNameId = change.RightSideStreetNameId,
                    Provenance = new ProvenanceData(provenance)
                };
                session.Events.Append(streamKey, legacyEvent);

                return Task.CompletedTask;
            }, token);
        }
    }

    private static Provenance BuildProvenance(Envelope<RoadNetworkChangesAccepted> envelope, Modification modification)
    {
        return new Provenance(
            LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            Application.RoadRegistry,
            new Reason(envelope.Message.Reason),
            new Operator(envelope.Message.OrganizationId),
            modification,
            Organisation.DigitaalVlaanderen);
    }

    private static Provenance BuildStreetNameProvenance(string when, Modification modification)
    {
        return new Provenance(
            LocalDateTimeTranslator.TranslateFromWhen(when),
            Application.RoadRegistry,
            new Reason("StreetName"),
            new Operator("StreetNameRegistry"),
            modification,
            Organisation.DigitaalVlaanderen);
    }

    private static Provenance BuildOrganizationProvenance(string when, Modification modification)
    {
        return new Provenance(
            LocalDateTimeTranslator.TranslateFromWhen(when),
            Application.RoadRegistry,
            new Reason("Organization"),
            new Operator("OrganizationRegistry"),
            modification,
            Organisation.DigitaalVlaanderen);
    }

    private static string OrganizationStreamKey(OrganizationId organizationId)
    {
        return $"Organization-{organizationId}";
    }

    private static string StreetNameStreamKey(StreetNameLocalId streetNameId)
    {
        return $"StreetName-{streetNameId}";
    }

    private static string BuildIdemponentSessionIdentifier<TMessage>(Envelope<TMessage> envelope, int? changeIndex = null)
        where TMessage : IMessage
    {
        return "MartenMigrationProjection-" + (
            changeIndex is not null
                ? $"{envelope.Position}-{changeIndex}"
                : envelope.Position.ToString());
    }
}

public static class LocalDateTimeTranslator
{
    private static readonly DateTimeZone LocalTimeZone =
        DateTimeZoneProviders.Tzdb["Europe/Brussels"];

    public static Instant TranslateFromWhen(string value)
    {
        return Instant.FromDateTimeOffset(new ZonedDateTime(InstantPattern.ExtendedIso.Parse(value).Value, LocalTimeZone)
            .ToDateTimeOffset());
    }
}
