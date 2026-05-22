namespace RoadRegistry.Read.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentReadProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_roadsegments")
            .Identity(x => x.Id);
    }

    public RoadSegmentReadProjection()
    {
        // V2
        When<IEvent<RoadSegmentWasAdded>>((session, e, _) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = e.Data.Geometry,
                StartNodeId = e.Data.StartNodeId,
                EndNodeId = e.Data.EndNodeId,
                GeometryDrawMethod = e.Data.GeometryDrawMethod,
                Status = e.Data.Status,
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString()),
                StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId),
                MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString()),
                CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessForward),
                CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessBackward),
                BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessForward),
                BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessBackward),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.PedestrianAccess),
                EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList(),
                NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<RoadSegmentWasMerged>>((session, e, _) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = e.Data.Geometry,
                StartNodeId = e.Data.StartNodeId,
                EndNodeId = e.Data.EndNodeId,
                GeometryDrawMethod = e.Data.GeometryDrawMethod,
                Status = e.Data.Status,
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString()),
                StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId),
                MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString()),
                CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessForward),
                CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessBackward),
                BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessForward),
                BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessBackward),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.PedestrianAccess),
                EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList(),
                NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<RoadSegmentGeometryWasModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Geometry;
                segment.StartNodeId = e.Data.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Geometry ?? segment.Geometry;
                segment.StartNodeId = e.Data.StartNodeId ?? segment.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId ?? segment.EndNodeId;
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod ?? segment.GeometryDrawMethod;
                segment.Status = e.Data.Status ?? segment.Status;

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

                if (e.Data.StreetNameId is not null)
                {
                    segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId);
                }

                if (e.Data.MaintenanceAuthorityId is not null)
                {
                    segment.MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId);
                }

                if (e.Data.SurfaceType is not null)
                {
                    segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString());
                }

                if (e.Data.CarAccessForward is not null)
                {
                    segment.CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessForward);
                }

                if (e.Data.CarAccessBackward is not null)
                {
                    segment.CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessBackward);
                }

                if (e.Data.BikeAccessForward is not null)
                {
                    segment.BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessForward);
                }

                if (e.Data.BikeAccessBackward is not null)
                {
                    segment.BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessBackward);
                }

                if (e.Data.PedestrianAccess is not null)
                {
                    segment.PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.PedestrianAccess);
                }
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasMigrated>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Geometry;
                segment.StartNodeId = e.Data.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId;
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod;
                segment.Status = e.Data.Status;
                segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString());
                segment.Category = e.Data.Category.ToStringAttributeValues(x => x.ToString());
                segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString());
                segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId);
                segment.MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId);
                segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString());
                segment.CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessForward);
                segment.CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.CarAccessBackward);
                segment.BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessForward);
                segment.BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.BikeAccessBackward);
                segment.PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.PedestrianAccess);
                segment.EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList();
                segment.NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList();
                segment.IsV2 = true;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasRemoved>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRemovedBecauseOfMigration>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRetired>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRetiredBecauseOfMerger>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
    }

    private async Task ModifyRoadSegment<TEvent>(IDocumentOperations operations, RoadSegmentId roadSegmentId, Action<RoadSegmentReadItem> modify, TEvent evt, CancellationToken ct)
        where TEvent : IMartenEvent
    {
        var roadSegment = await operations.LoadAsync<RoadSegmentReadItem>(roadSegmentId, ct);
        if (roadSegment is null)
        {
            throw new InvalidOperationException($"RoadSegment with id {roadSegmentId} is not found");
        }

        modify(roadSegment);

        roadSegment.LastModified = evt.Provenance.ToEventTimestamp();
        operations.Store(roadSegment);
    }

    private static ExtractRoadSegmentDynamicAttribute<StreetNameLocalId> BuildStreetNameIdAttributesFromV1(int? leftSideStreetNameId, int? rightSideStreetNameId, RoadSegmentGeometry geometry)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return ForEntireGeometry(StreetNameLocalId.NotApplicable, geometry);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return ForEntireGeometry(new StreetNameLocalId(leftSideStreetNameId!.Value), geometry);
        }

        return new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>([
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Left, StreetNameLocalId.FromValue(leftSideStreetNameId) ?? StreetNameLocalId.NotApplicable),
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Right, StreetNameLocalId.FromValue(rightSideStreetNameId) ?? StreetNameLocalId.NotApplicable)
        ]);
    }

    private static ExtractRoadSegmentDynamicAttribute<T> ForEntireGeometry<T>(T value, RoadSegmentGeometry geometry)
    {
        return new ExtractRoadSegmentDynamicAttribute<T>([(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Both, value)]);
    }

    private static RoadSegmentPositionV2 UseGeometryLengthIfPositionIsLast(double position, RoadSegmentGeometry geometry, bool isLast)
    {
        return isLast
            ? new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm())
            : new RoadSegmentPositionV2(position.RoundToCm());
    }

    private static StreetNameLocalId GetValue(ExtractRoadSegmentDynamicAttribute<StreetNameLocalId> attributes, RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Left).Value,
            RoadSegmentAttributeSide.Right => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Right).Value,
            _ => throw new InvalidOperationException("Only left or right side is allowed.")
        };
    }

    private static RoadSegmentGeometry ToLambert08(RoadSegmentGeometry geometry)
    {
        return RoadSegmentGeometry.Create(geometry.Value.TransformFromLambert72To08());
    }
}

public sealed class RoadSegmentReadItem
{
    [JsonIgnore] public int Id { get; private set; }

    public required RoadSegmentId RoadSegmentId
    {
        get => new(Id);
        set => Id = value;
    }

    public required RoadSegmentGeometry Geometry { get; set; }
    public required RoadNodeId? StartNodeId { get; set; }
    public required RoadNodeId? EndNodeId { get; set; }
    public required string GeometryDrawMethod { get; set; }
    public required string Status { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> AccessRestriction { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> Category { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> Morphology { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<StreetNameLocalId> StreetNameId { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<OrganizationId> MaintenanceAuthorityId { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> SurfaceType { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<bool> CarAccessForward { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<bool> CarAccessBackward { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<bool> BikeAccessForward { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<bool> BikeAccessBackward { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<bool> PedestrianAccess { get; set; }
    public required List<EuropeanRoadNumber> EuropeanRoadNumbers { get; set; }
    public required List<NationalRoadNumber> NationalRoadNumbers { get; set; }
    public required EventTimestamp Origin { get; set; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
}

public sealed class ExtractRoadSegmentDynamicAttribute<T>
{
    public List<ExtractRoadSegmentDynamicAttributeValue<T>> Values { get; set; } = [];

    public ExtractRoadSegmentDynamicAttribute()
    {
    }

    public ExtractRoadSegmentDynamicAttribute(RoadSegmentDynamicAttributeValues<T> attributes)
        : this(attributes.Values.Select(x => (x.Coverage.From, x.Coverage.To, x.Side, x.Value)))
    {
    }

    public ExtractRoadSegmentDynamicAttribute(IEnumerable<(RoadSegmentPositionV2 From, RoadSegmentPositionV2 To, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = values
            .OrderBy(x => x.From)
            .Select(x => new ExtractRoadSegmentDynamicAttributeValue<T>
            {
                From = x.From,
                To = x.To,
                Side = x.Side,
                Value = x.Value
            })
            .ToList();
    }
}

public interface IExtractRoadSegmentDynamicAttributeValueCoverage
{
    public RoadSegmentPositionV2 From { get; }
    public RoadSegmentPositionV2 To { get; }
}

public sealed class ExtractRoadSegmentDynamicAttributeValue<T> : IExtractRoadSegmentDynamicAttributeValueCoverage
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentAttributeSide Side { get; set; }

    public required RoadSegmentPositionV2 From { get; set; }
    public required RoadSegmentPositionV2 To { get; set; }
    public required T Value { get; set; }
}

internal static class RoadSegmentDynamicAttributeValuesExtensions
{
    public static ExtractRoadSegmentDynamicAttribute<string> ToStringAttributeValues<T>(this RoadSegmentDynamicAttributeValues<T> attributes, Func<T, string> converter)
    {
        return new ExtractRoadSegmentDynamicAttribute<string>(attributes.Values.Select(x => (x.Coverage.From, x.Coverage.To, x.Side, converter(x.Value))));
    }
}
