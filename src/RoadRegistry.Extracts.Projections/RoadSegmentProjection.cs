namespace RoadRegistry.Extracts.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackOffice;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadSegment.Events.V1;
using RoadSegment.Events.V2;
using RoadSegment.ValueObjects;

public class RoadSegmentProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentExtractItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("extract_roadsegments")
            .Identity(x => x.Id);
    }

    public RoadSegmentProjection()
    {
        // V1
        When<IEvent<ImportedRoadSegment>>((session, e, ct) =>
        {
            var roadSegmentId = new RoadSegmentId(e.Data.RoadSegmentId);
            var geometry = e.Data.Geometry;
            var status = e.Data.Status;
            var morphology = e.Data.Morphology;
            var category = e.Data.Category;
            var geometryDrawMethod = e.Data.GeometryDrawMethod;
            var accessRestriction = e.Data.AccessRestriction;

            var roadSegment = new RoadSegmentExtractItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = geometry,
                StartNodeId = new RoadNodeId(e.Data.StartNodeId),
                EndNodeId = new RoadNodeId(e.Data.EndNodeId),
                GeometryDrawMethod = geometryDrawMethod,
                AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(accessRestriction),
                Category = new RoadSegmentDynamicAttributeValues<string>(category),
                Morphology = new RoadSegmentDynamicAttributeValues<string>(morphology),
                Status = new RoadSegmentDynamicAttributeValues<string>(status),
                StreetNameId = BuildStreetNameIdAttributes(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code)),
                SurfaceType = new RoadSegmentDynamicAttributeValues<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                ),
                EuropeanRoadNumbers = e.Data.PartOfEuropeanRoads
                    .Select(x => EuropeanRoadNumber.Parse(x.Number))
                    .ToList(),
                NationalRoadNumbers = e.Data.PartOfNationalRoads
                    .Select(x => NationalRoadNumber.Parse(x.Number))
                    .ToList(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<RoadSegmentAdded>>((session, e, ct) =>
        {
            var roadSegmentId = new RoadSegmentId(e.Data.RoadSegmentId);
            var status = e.Data.Status;
            var morphology = e.Data.Morphology;
            var category = e.Data.Category;
            var geometryDrawMethod = e.Data.GeometryDrawMethod;
            var accessRestriction = e.Data.AccessRestriction;

            var roadSegment = new RoadSegmentExtractItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = e.Data.Geometry,
                StartNodeId = new RoadNodeId(e.Data.StartNodeId),
                EndNodeId = new RoadNodeId(e.Data.EndNodeId),
                GeometryDrawMethod = geometryDrawMethod,
                AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(accessRestriction),
                Category = new RoadSegmentDynamicAttributeValues<string>(category),
                Morphology = new RoadSegmentDynamicAttributeValues<string>(morphology),
                Status = new RoadSegmentDynamicAttributeValues<string>(status),
                StreetNameId = BuildStreetNameIdAttributes(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code)),
                SurfaceType = new RoadSegmentDynamicAttributeValues<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                ),
                EuropeanRoadNumbers = [],
                NationalRoadNumbers = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<RoadSegmentModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                var status = e.Data.Status;
                var morphology = e.Data.Morphology;
                var category = e.Data.Category;
                var geometryDrawMethod = e.Data.GeometryDrawMethod;
                var accessRestriction = e.Data.AccessRestriction;

                segment.Geometry = e.Data.Geometry;
                segment.StartNodeId = new RoadNodeId(e.Data.StartNodeId);
                segment.EndNodeId = new RoadNodeId(e.Data.EndNodeId);
                segment.GeometryDrawMethod = geometryDrawMethod;
                segment.AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(accessRestriction);
                segment.Category = new RoadSegmentDynamicAttributeValues<string>(category);
                segment.Morphology = new RoadSegmentDynamicAttributeValues<string>(morphology);
                segment.Status = new RoadSegmentDynamicAttributeValues<string>(status);
                segment.StreetNameId = BuildStreetNameIdAttributes(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId);
                segment.MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code));
                segment.SurfaceType = new RoadSegmentDynamicAttributeValues<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                );
            }, e.Data);
        });
        When<IEvent<RoadSegmentRemoved>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"RoadSegment with id {e.Data.RoadSegmentId} is not found");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.EuropeanRoadNumbers.Add(EuropeanRoadNumber.Parse(e.Data.Number)); }, e.Data); });
        When<IEvent<RoadSegmentAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.NationalRoadNumbers.Add(NationalRoadNumber.Parse(e.Data.Number)); }, e.Data); });
        When<IEvent<RoadSegmentRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.EuropeanRoadNumbers.Remove(EuropeanRoadNumber.Parse(e.Data.Number)); }, e.Data); });
        When<IEvent<RoadSegmentRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.NationalRoadNumbers.Remove(NationalRoadNumber.Parse(e.Data.Number)); }, e.Data); });
        When<IEvent<RoadSegmentAttributesModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                if (e.Data.AccessRestriction is not null)
                {
                    segment.AccessRestriction = new RoadSegmentDynamicAttributeValues<string>(e.Data.AccessRestriction);
                }

                if (e.Data.Category is not null)
                {
                    segment.Category = new RoadSegmentDynamicAttributeValues<string>(e.Data.Category);
                }

                if (e.Data.Morphology is not null)
                {
                    segment.Morphology = new RoadSegmentDynamicAttributeValues<string>(e.Data.Morphology);
                }

                if (e.Data.Status is not null)
                {
                    segment.Status = new RoadSegmentDynamicAttributeValues<string>(e.Data.Status);
                }

                if (e.Data.LeftSide is not null || e.Data.RightSide is not null)
                {
                    segment.StreetNameId = BuildStreetNameIdAttributes(
                        e.Data.LeftSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Left),
                        e.Data.RightSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Right));
                }

                if (e.Data.MaintenanceAuthority is not null)
                {
                    segment.MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code));
                }

                if (e.Data.Surfaces is not null)
                {
                    segment.SurfaceType = new RoadSegmentDynamicAttributeValues<string>(e.Data.Surfaces
                        .Select(x => (
                            new RoadSegmentPosition(x.FromPosition),
                            new RoadSegmentPosition(x.ToPosition),
                            RoadSegmentAttributeSide.Both,
                            x.Type))
                    );
                }
            }, e.Data);
        });
        When<IEvent<RoadSegmentGeometryModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                segment.Geometry = e.Data.Geometry;
                segment.SurfaceType = new RoadSegmentDynamicAttributeValues<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                );
            }, e.Data);
        });
        When<IEvent<RoadSegmentStreetNamesChanged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                if (e.Data.LeftSideStreetNameId is not null || e.Data.RightSideStreetNameId is not null)
                {
                    segment.StreetNameId = BuildStreetNameIdAttributes(
                        e.Data.LeftSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Left),
                        e.Data.RightSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Right));
                }
            }, e.Data);
        });
        When<IEvent<OutlinedRoadSegmentRemoved>>((_, _, _) => Task.CompletedTask); // Do nothing
        When<IEvent<RoadSegmentAddedToNumberedRoad>>((_, _, _) => Task.CompletedTask); // Do nothing
        When<IEvent<RoadSegmentRemovedFromNumberedRoad>>((_, _, _) => Task.CompletedTask); // Do nothing

        // V2
        When<IEvent<RoadSegmentWasAdded>>((session, e, ct) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var roadSegment = new RoadSegmentExtractItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = e.Data.Geometry,
                StartNodeId = new RoadNodeId(e.Data.StartNodeId),
                EndNodeId = new RoadNodeId(e.Data.EndNodeId),
                GeometryDrawMethod = e.Data.GeometryDrawMethod,
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString()),
                Status = e.Data.Status.ToStringAttributeValues(x => x.ToString()),
                StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(e.Data.StreetNameId),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(e.Data.MaintenanceAuthorityId),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString()),
                EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList(),
                NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<RoadSegmentWasMerged>>((session, e, ct) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var roadSegment = new RoadSegmentExtractItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = e.Data.Geometry,
                StartNodeId = new RoadNodeId(e.Data.StartNodeId),
                EndNodeId = new RoadNodeId(e.Data.EndNodeId),
                GeometryDrawMethod = e.Data.GeometryDrawMethod,
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString()),
                Status = e.Data.Status.ToStringAttributeValues(x => x.ToString()),
                StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(e.Data.StreetNameId),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(e.Data.MaintenanceAuthorityId),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString()),
                EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList(),
                NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<RoadSegmentWasModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Geometry ?? segment.Geometry;
                segment.StartNodeId = e.Data.StartNodeId ?? segment.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId ?? segment.EndNodeId;
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod ?? segment.GeometryDrawMethod;

                if (e.Data.AccessRestriction is not null)
                {
                    segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString());
                }

                if (e.Data.Category is not null)
                {
                    segment.Category = e.Data.Category.ToStringAttributeValues(x => x.ToString());
                }

                if (e.Data.Morphology is not null)
                {
                    segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString());
                }

                if (e.Data.Status is not null)
                {
                    segment.Status = e.Data.Status.ToStringAttributeValues(x => x.ToString());
                }

                if (e.Data.StreetNameId is not null)
                {
                    segment.StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(e.Data.StreetNameId);
                }

                if (e.Data.MaintenanceAuthorityId is not null)
                {
                    segment.MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(e.Data.MaintenanceAuthorityId);
                }

                if (e.Data.SurfaceType is not null)
                {
                    segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString());
                }
            }, e.Data);
        });
        When<IEvent<RoadSegmentWasMigrated>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Geometry;
                segment.StartNodeId = e.Data.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId;
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod;
                segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString());
                segment.Category = e.Data.Category.ToStringAttributeValues(x => x.ToString());
                segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString());
                segment.Status = e.Data.Status.ToStringAttributeValues(x => x.ToString());
                segment.StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(e.Data.StreetNameId);
                segment.MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(e.Data.MaintenanceAuthorityId);
                segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString());
                segment.EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList();
                segment.NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList();
                segment.IsV2 = true;
            }, e.Data);
        });
        When<IEvent<RoadSegmentWasRemoved>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRetiredBecauseOfMerger>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRetiredBecauseOfMigration>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Add(e.Data.Number); }, e.Data); });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Add(e.Data.Number); }, e.Data); });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Remove(e.Data.Number); }, e.Data); });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Remove(e.Data.Number); }, e.Data); });
    }

    private async Task ModifyRoadSegment<TEvent>(IDocumentOperations operations, RoadSegmentId roadSegmentId, Action<RoadSegmentExtractItem> modify, TEvent evt)
        where TEvent : IMartenEvent
    {
        var roadSegment = await operations.LoadAsync<RoadSegmentExtractItem>(roadSegmentId);
        if (roadSegment is null)
        {
            throw new InvalidOperationException($"RoadSegment with id {roadSegmentId} is not found");
        }

        modify(roadSegment);

        roadSegment.LastModified = evt.Provenance.ToEventTimestamp();
        operations.Store(roadSegment);
    }

    private static RoadSegmentDynamicAttributeValues<StreetNameLocalId> BuildStreetNameIdAttributes(int? leftSideStreetNameId, int? rightSideStreetNameId)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(StreetNameLocalId.NotApplicable);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(new StreetNameLocalId(leftSideStreetNameId!.Value));
        }

        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>([
            (null, null, RoadSegmentAttributeSide.Left, StreetNameLocalId.FromValue(leftSideStreetNameId) ?? StreetNameLocalId.NotApplicable),
            (null, null, RoadSegmentAttributeSide.Right, StreetNameLocalId.FromValue(rightSideStreetNameId) ?? StreetNameLocalId.NotApplicable)
        ]);
    }

    private static StreetNameLocalId GetValue(RoadSegmentDynamicAttributeValues<StreetNameLocalId> attributes, RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Left).Value,
            RoadSegmentAttributeSide.Right => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Right).Value,
            _ => throw new InvalidOperationException("Only left or right side is allowed.")
        };
    }
}

public sealed class RoadSegmentExtractItem
{
    [JsonIgnore] public int Id { get; private set; }

    public required RoadSegmentId RoadSegmentId
    {
        get => new(Id);
        set => Id = value;
    }

    public required RoadSegmentGeometry Geometry { get; set; }
    public required RoadNodeId StartNodeId { get; set; }
    public required RoadNodeId EndNodeId { get; set; }
    public required string GeometryDrawMethod { get; set; }
    public required RoadSegmentDynamicAttributeValues<string> AccessRestriction { get; set; }
    public required RoadSegmentDynamicAttributeValues<string> Category { get; set; }
    public required RoadSegmentDynamicAttributeValues<string> Morphology { get; set; }
    public required RoadSegmentDynamicAttributeValues<string> Status { get; set; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; set; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; set; }
    public required RoadSegmentDynamicAttributeValues<string> SurfaceType { get; set; }
    public required List<EuropeanRoadNumber> EuropeanRoadNumbers { get; set; }
    public required List<NationalRoadNumber> NationalRoadNumbers { get; set; }

    public required EventTimestamp Origin { get; set; }
    public required EventTimestamp LastModified { get; set; }

    public required bool IsV2 { get; set; }
}

internal static class RoadSegmentDynamicAttributeValuesExtensions
{
    public static RoadSegmentDynamicAttributeValues<string> ToStringAttributeValues<T>(this RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<T> attributes, Func<T, string> converter)
    {
        return new RoadSegmentDynamicAttributeValues<string>(attributes.Values.Select(x => (x.Coverage?.From, x.Coverage?.To, x.Side, converter(x.Value))));
    }
}

public sealed class RoadSegmentDynamicAttributeValues<T>
{
    public List<RoadSegmentDynamicAttributeValue<T>> Values { get; set; } = [];

    public RoadSegmentDynamicAttributeValues()
    {
    }

    public RoadSegmentDynamicAttributeValues(T value)
        : this([(null, null, RoadSegmentAttributeSide.Both, value)])
    {
    }

    public RoadSegmentDynamicAttributeValues(RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<T> attributes)
        : this(attributes.Values.Select(x => (x.Coverage?.From, x.Coverage?.To, x.Side, x.Value)))
    {
    }

    public RoadSegmentDynamicAttributeValues(IEnumerable<(RoadSegmentPosition From, RoadSegmentPosition To, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = values
            .OrderBy(x => x.From)
            .Select(x => new RoadSegmentDynamicAttributeValue<T>
            {
                From = x.From,
                To = x.To,
                Side = x.Side,
                Value = x.Value
            })
            .ToList();
    }

    public RoadSegmentDynamicAttributeValues(IEnumerable<(RoadSegmentPosition? From, RoadSegmentPosition? To, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = values
            .OrderBy(x => x.From)
            .Select(x => new RoadSegmentDynamicAttributeValue<T>
            {
                From = x.From,
                To = x.To,
                Side = x.Side,
                Value = x.Value
            })
            .ToList();
    }
}

public sealed class RoadSegmentDynamicAttributeValue<T>
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentAttributeSide Side { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentPosition? From { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentPosition? To { get; set; }

    public required T Value { get; set; }
}
