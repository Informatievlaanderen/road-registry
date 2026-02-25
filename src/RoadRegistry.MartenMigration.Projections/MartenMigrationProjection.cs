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
using NodaTime;
using NodaTime.Text;
using RoadNode;
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
    private readonly MigrationRoadNetworkRepository _repo;

    public MartenMigrationProjection(MigrationRoadNetworkRepository repo)
    {
        _repo = repo.ThrowIfNull();

        When<Envelope<BackOffice.Messages.ImportedRoadNode>>((_, envelope, token) =>
        {
            var eventIdentifier = BuildEventIdentifier(envelope);

            return _repo.InIdempotentSession(eventIdentifier, session =>
            {
                var point = GeometryTranslator.Translate(envelope.Message.Geometry);
                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.Origin.Since),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

                var roadNodeId = new RoadNodeId(envelope.Message.Id);
                var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
                var legacyEvent = new ImportedRoadNode
                {
                    Geometry = point.ToRoadNodeGeometry(),
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
            }, token);
        });

        When<Envelope<BackOffice.Messages.ImportedRoadSegment>>((_, envelope, token) =>
        {
            var eventIdentifier = BuildEventIdentifier(envelope);

            return _repo.InIdempotentSession(eventIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

                var geometry = GeometryTranslator.Translate(envelope.Message.Geometry);
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
                    Geometry = geometry.ToRoadSegmentGeometry(),
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
            }, token);
        });

        When<Envelope<BackOffice.Messages.ImportedGradeSeparatedJunction>>((_, envelope, token) =>
        {
            var eventIdentifier = BuildEventIdentifier(envelope);

            return _repo.InIdempotentSession(eventIdentifier, session =>
            {
                session.CorrelationId = new ScopedRoadNetworkId(Guid.NewGuid());
                session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.Origin.Since),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                var junctionId = new GradeSeparatedJunctionId(envelope.Message.Id);
                var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junctionId);
                var legacyEvent = new ImportedGradeSeparatedJunction
                {
                    Id = envelope.Message.Id,
                    LowerRoadSegmentId = envelope.Message.LowerRoadSegmentId,
                    Type = envelope.Message.Type,
                    UpperRoadSegmentId = envelope.Message.UpperRoadSegmentId,
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

        When<Envelope<RoadSegmentsStreetNamesChanged>>(async (_, envelope, token) =>
        {
            var roadNetworkId = new ScopedRoadNetworkId(Guid.NewGuid());

            await RoadSegmentsStreetNamesChanged(roadNetworkId, envelope, token);
        });
    }

    private Task AcceptRoadNetworkChange(
        ScopedRoadNetworkId roadNetworkId,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope);

        return _repo.InIdempotentSession(eventIdentifier, session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

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
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var point = GeometryTranslator.Translate(change.Geometry);
            var provenance = BuildProvenance(envelope, Modification.Insert);

            var roadNodeId = new RoadNodeId(change.Id);
            var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
            var legacyEvent = new RoadRegistry.RoadNode.Events.V1.RoadNodeAdded
            {
                Geometry = point.ToRoadNodeGeometry(),
                RoadNodeId = change.Id,
                Version = change.Version,
                TemporaryId = change.TemporaryId,
                OriginalId = change.OriginalId,
                Type = change.Type,
                Provenance = new ProvenanceData(provenance),
            };
            session.Events.Append(streamKey, legacyEvent);
        }, token);
    }

    private Task ModifyRoadNode(
        ScopedRoadNetworkId roadNetworkId,
        RoadNodeModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
            {
                session.CorrelationId = roadNetworkId;
                session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

                var point = GeometryTranslator.Translate(change.Geometry);
                var provenance = BuildProvenance(envelope, Modification.Update);

                var roadNodeId = new RoadNodeId(change.Id);
                var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
                var legacyEvent = new RoadRegistry.RoadNode.Events.V1.RoadNodeModified
                {
                    Geometry = point.ToRoadNodeGeometry(),
                    RoadNodeId = change.Id,
                    Version = change.Version,
                    Type = change.Type,
                    Provenance = new ProvenanceData(provenance)
                };
                session.Events.Append(streamKey, legacyEvent);
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
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Delete);

            var roadNodeId = new RoadNodeId(change.Id);
            var streamKey = StreamKeyFactory.Create(typeof(RoadNode), roadNodeId);
            var legacyEvent = new RoadRegistry.RoadNode.Events.V1.RoadNodeRemoved
            {
                RoadNodeId = change.Id,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);
        }, token);
    }

    private Task AddRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var geometry = GeometryTranslator.Translate(change.Geometry);
            var provenance = BuildProvenance(envelope, Modification.Insert);

            var roadSegmentId = new RoadSegmentId(change.Id);
            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = new RoadRegistry.RoadSegment.Events.V1.RoadSegmentAdded
            {
                RoadSegmentId = change.Id,
                TemporaryId = change.TemporaryId,
                OriginalId = change.OriginalId,
                Geometry = geometry.ToRoadSegmentGeometry(),
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

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                var geometry = GeometryTranslator.Translate(change.Geometry);

                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentModified
                {
                    RoadSegmentId = change.Id,
                    OriginalId = change.OriginalId,
                    ConvertedFromOutlined = change.ConvertedFromOutlined,
                    Geometry = geometry.ToRoadSegmentGeometry(),
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
            envelope, change, changeIndex, token);
    }

    private Task ModifyRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentId roadSegmentId,
        Func<Provenance, IMartenEvent> getLegacyEvent,
        Envelope<RoadNetworkChangesAccepted> envelope,
        object change,
        int changeIndex,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Update);

            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = getLegacyEvent(provenance);
            session.Events.Append(streamKey, legacyEvent);

            return Task.CompletedTask;
        }, token);
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
            envelope, change, changeIndex, token);
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
            envelope, change, changeIndex, token);
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
            envelope, change, changeIndex, token);
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
            envelope, change, changeIndex, token);
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
            envelope, change, changeIndex, token);
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
            envelope, change, changeIndex, token);
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
            envelope, change, changeIndex, token);
    }

    private async Task ModifyRoadSegmentGeometry(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentGeometryModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);

        await ModifyRoadSegment(roadNetworkId, roadSegmentId,
            provenance =>
            {
                return new RoadRegistry.RoadSegment.Events.V1.RoadSegmentGeometryModified
                {
                    RoadSegmentId = change.Id,
                    Version = change.Version,
                    GeometryVersion = change.GeometryVersion,
                    Geometry = GeometryTranslator.Translate(change.Geometry).ToRoadSegmentGeometry(),
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
            envelope, change, changeIndex, token);
    }

    private Task RemoveRoadSegment(
        ScopedRoadNetworkId roadNetworkId,
        RoadSegmentRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Delete);

            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = new RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemoved
            {
                RoadSegmentId = change.Id,
                GeometryDrawMethod = change.GeometryDrawMethod,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);
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
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Delete);

            var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentId);
            var legacyEvent = new RoadRegistry.RoadSegment.Events.V1.OutlinedRoadSegmentRemoved
            {
                RoadSegmentId = change.Id,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);
        }, token);
    }

    private Task AddGradeSeparatedJunction(
        ScopedRoadNetworkId roadNetworkId,
        GradeSeparatedJunctionAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Insert);

            var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), new GradeSeparatedJunctionId(change.Id));
            var legacyEvent = new RoadRegistry.GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionAdded
            {
                Id = change.Id,
                TemporaryId = change.TemporaryId,
                LowerRoadSegmentId = change.LowerRoadSegmentId,
                UpperRoadSegmentId = change.UpperRoadSegmentId,
                Type = change.Type,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);
        }, token);
    }

    private Task RemoveGradeSeparatedJunction(
        ScopedRoadNetworkId roadNetworkId,
        GradeSeparatedJunctionRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Insert);

            var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), new GradeSeparatedJunctionId(change.Id));
            var legacyEvent = new RoadRegistry.GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionRemoved
            {
                Id = change.Id,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);
        }, token);
    }

    private Task ModifyGradeSeparatedJunction(
        ScopedRoadNetworkId roadNetworkId,
        GradeSeparatedJunctionModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            session.CorrelationId = roadNetworkId;
            session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

            var provenance = BuildProvenance(envelope, Modification.Insert);

            var streamKey = StreamKeyFactory.Create(typeof(GradeSeparatedJunction), new GradeSeparatedJunctionId(change.Id));
            var legacyEvent = new RoadRegistry.GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionModified
            {
                Id = change.Id,
                LowerRoadSegmentId = change.LowerRoadSegmentId,
                UpperRoadSegmentId = change.UpperRoadSegmentId,
                Type = change.Type,
                Provenance = new ProvenanceData(provenance)
            };
            session.Events.Append(streamKey, legacyEvent);
        }, token);
    }

    private async Task RoadSegmentsStreetNamesChanged(
        ScopedRoadNetworkId roadNetworkId,
        Envelope<RoadSegmentsStreetNamesChanged> envelope,
        CancellationToken token)
    {
        foreach (var (change, index) in envelope.Message.RoadSegments.Select((x, i) => (x, i)))
        {
            var eventIdentifier = BuildEventIdentifier(envelope, index);

            await _repo.InIdempotentSession(eventIdentifier, async session =>
            {
                session.CorrelationId = roadNetworkId;
                session.CausationId = $"migration-{envelope.EventName}-{eventIdentifier}";

                var provenance = new Provenance(
                    LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    Application.RoadRegistry,
                    new Reason(envelope.Message.Reason),
                    new Operator(string.Empty),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen);

                var streamKey = StreamKeyFactory.Create(typeof(RoadSegment), new RoadSegmentId(change.Id));
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

    private static string BuildEventIdentifier<TMessage>(Envelope<TMessage> envelope, int? changeIndex = null)
        where TMessage : IMessage
    {
        return changeIndex is not null
            ? $"{envelope.Position}-{changeIndex}"
            : envelope.Position.ToString();
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
