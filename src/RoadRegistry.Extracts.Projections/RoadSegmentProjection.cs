namespace RoadRegistry.Extracts.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure;
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
            var geometry = ToLambert08(e.Data.Geometry);
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
                AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(accessRestriction, geometry),
                Category = new ExtractRoadSegmentDynamicAttribute<string>(category, geometry),
                Morphology = new ExtractRoadSegmentDynamicAttribute<string>(morphology, geometry),
                Status = new ExtractRoadSegmentDynamicAttribute<string>(status, geometry),
                StreetNameId = BuildStreetNameIdAttributes(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry),
                MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry),
                SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                ),
                CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(),
                BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(),
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
            var geometry = ToLambert08(e.Data.Geometry);
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
                AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(accessRestriction, geometry),
                Category = new ExtractRoadSegmentDynamicAttribute<string>(category, geometry),
                Morphology = new ExtractRoadSegmentDynamicAttribute<string>(morphology, geometry),
                Status = new ExtractRoadSegmentDynamicAttribute<string>(status, geometry),
                StreetNameId = BuildStreetNameIdAttributes(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry),
                MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry),
                SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                ),
                CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(),
                BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(),
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

                segment.Geometry = ToLambert08(e.Data.Geometry);
                segment.StartNodeId = new RoadNodeId(e.Data.StartNodeId);
                segment.EndNodeId = new RoadNodeId(e.Data.EndNodeId);
                segment.GeometryDrawMethod = geometryDrawMethod;
                segment.AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(accessRestriction, segment.Geometry);
                segment.Category = new ExtractRoadSegmentDynamicAttribute<string>(category, segment.Geometry);
                segment.Morphology = new ExtractRoadSegmentDynamicAttribute<string>(morphology, segment.Geometry);
                segment.Status = new ExtractRoadSegmentDynamicAttribute<string>(status, segment.Geometry);
                segment.StreetNameId = BuildStreetNameIdAttributes(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, segment.Geometry);
                segment.MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry);
                segment.SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                );
            }, e.Data, ct);
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
        When<IEvent<RoadSegmentAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.EuropeanRoadNumbers.Add(EuropeanRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.NationalRoadNumbers.Add(NationalRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.EuropeanRoadNumbers.Remove(EuropeanRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.NationalRoadNumbers.Remove(NationalRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentAttributesModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                if (e.Data.AccessRestriction is not null)
                {
                    segment.AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.AccessRestriction, segment.Geometry);
                }

                if (e.Data.Category is not null)
                {
                    segment.Category = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Category, segment.Geometry);
                }

                if (e.Data.Morphology is not null)
                {
                    segment.Morphology = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Morphology, segment.Geometry);
                }

                if (e.Data.Status is not null)
                {
                    segment.Status = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Status, segment.Geometry);
                }

                if (e.Data.LeftSide is not null || e.Data.RightSide is not null)
                {
                    segment.StreetNameId = BuildStreetNameIdAttributes(
                        e.Data.LeftSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Left),
                        e.Data.RightSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Right),
                        segment.Geometry);
                }

                if (e.Data.MaintenanceAuthority is not null)
                {
                    segment.MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry);
                }

                if (e.Data.Surfaces is not null)
                {
                    segment.SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                        .Select(x => (
                            new RoadSegmentPosition(x.FromPosition),
                            new RoadSegmentPosition(x.ToPosition),
                            RoadSegmentAttributeSide.Both,
                            x.Type))
                    );
                }
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentGeometryModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                segment.Geometry = ToLambert08(e.Data.Geometry);
                segment.SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                );
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentStreetNamesChanged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                if (e.Data.LeftSideStreetNameId is not null || e.Data.RightSideStreetNameId is not null)
                {
                    segment.StreetNameId = BuildStreetNameIdAttributes(
                        e.Data.LeftSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Left),
                        e.Data.RightSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Right),
                        segment.Geometry);
                }
            }, e.Data, ct);
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
                StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId),
                MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString()),
                CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.CarAccess),
                BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.BikeAccess),
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
                StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId),
                MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString()),
                CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.CarAccess),
                BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.BikeAccess),
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

                if (e.Data.CarAccess is not null)
                {
                    segment.CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.CarAccess);
                }

                if (e.Data.BikeAccess is not null)
                {
                    segment.BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.BikeAccess);
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
                segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString());
                segment.Category = e.Data.Category.ToStringAttributeValues(x => x.ToString());
                segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString());
                segment.Status = e.Data.Status.ToStringAttributeValues(x => x.ToString());
                segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId);
                segment.MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId);
                segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString());
                segment.CarAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.CarAccess);
                segment.BikeAccess = new ExtractRoadSegmentDynamicAttribute<VehicleAccess>(e.Data.BikeAccess);
                segment.PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(e.Data.PedestrianAccess);
                segment.EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList();
                segment.NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList();
                segment.IsV2 = true;
            }, e.Data, ct);
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
        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
    }

    private async Task ModifyRoadSegment<TEvent>(IDocumentOperations operations, RoadSegmentId roadSegmentId, Action<RoadSegmentExtractItem> modify, TEvent evt, CancellationToken ct)
        where TEvent : IMartenEvent
    {
        var roadSegment = await operations.LoadAsync<RoadSegmentExtractItem>(roadSegmentId, ct);
        if (roadSegment is null)
        {
            throw new InvalidOperationException($"RoadSegment with id {roadSegmentId} is not found");
        }

        modify(roadSegment);

        roadSegment.LastModified = evt.Provenance.ToEventTimestamp();
        operations.Store(roadSegment);
    }

    private static ExtractRoadSegmentDynamicAttribute<StreetNameLocalId> BuildStreetNameIdAttributes(int? leftSideStreetNameId, int? rightSideStreetNameId, RoadSegmentGeometry geometry)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(StreetNameLocalId.NotApplicable, geometry);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(new StreetNameLocalId(leftSideStreetNameId!.Value), geometry);
        }

        return new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>([
            (RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(geometry.Value.Length), RoadSegmentAttributeSide.Left, StreetNameLocalId.FromValue(leftSideStreetNameId) ?? StreetNameLocalId.NotApplicable),
            (RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(geometry.Value.Length), RoadSegmentAttributeSide.Right, StreetNameLocalId.FromValue(rightSideStreetNameId) ?? StreetNameLocalId.NotApplicable)
        ]);
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
    public required ExtractRoadSegmentDynamicAttribute<string> AccessRestriction { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> Category { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> Morphology { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> Status { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<StreetNameLocalId> StreetNameId { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<OrganizationId> MaintenanceAuthorityId { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<string> SurfaceType { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<VehicleAccess> CarAccess { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<VehicleAccess> BikeAccess { get; set; }
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

    public ExtractRoadSegmentDynamicAttribute(T value, RoadSegmentGeometry geometry)
        : this([(RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(geometry.Value.Length), RoadSegmentAttributeSide.Both, value)])
    {
    }

    public ExtractRoadSegmentDynamicAttribute(RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<T> attributes)
        : this(attributes.Values.Select(x => (x.Coverage.From, x.Coverage.To, x.Side, x.Value)))
    {
    }

    public ExtractRoadSegmentDynamicAttribute(IEnumerable<(RoadSegmentPosition From, RoadSegmentPosition To, RoadSegmentAttributeSide Side, T Value)> values)
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
    public RoadSegmentPosition From { get; }
    public RoadSegmentPosition To { get; }
}

public sealed class ExtractRoadSegmentDynamicAttributeValue<T> : IExtractRoadSegmentDynamicAttributeValueCoverage
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentAttributeSide Side { get; set; }

    public required RoadSegmentPosition From { get; set; }
    public required RoadSegmentPosition To { get; set; }
    public required T Value { get; set; }
}

internal static class RoadSegmentDynamicAttributeValuesExtensions
{
    public static ExtractRoadSegmentDynamicAttribute<string> ToStringAttributeValues<T>(this RoadSegmentDynamicAttributeValues<T> attributes, Func<T, string> converter)
    {
        return new ExtractRoadSegmentDynamicAttribute<string>(attributes.Values.Select(x => (x.Coverage.From, x.Coverage.To, x.Side, converter(x.Value))));
    }
}
