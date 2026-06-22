namespace RoadRegistry.Read.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using Extensions;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.StreetName;
using RoadRegistry.ValueObjects;
using RoadSegment.Events.V1;
using RoadSegment.Events.V2;
using RoadSegment.ValueObjects;
using RoadRegistry.StreetName.Events.V2;

public class RoadSegmentReadProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_roadsegments")
            .Identity(x => x.Id);

        options.Schema.For<StreetNameRoadSegmentsLink>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_streetname_roadsegments_link")
            .Identity(x => x.Id);

        options.Schema.For<OrganizationRoadSegmentsLink>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_organization_roadsegments_link")
            .Identity(x => x.Id);

        options.Schema.For<StreetNameReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_streetnames")
            .Identity(x => x.Id);
    }

    public RoadSegmentReadProjection()
    {
        // V1
        When<IEvent<ImportedRoadSegment>>(async (session, e, ct) =>
        {
            var roadSegmentId = new RoadSegmentId(e.Data.RoadSegmentId);
            var geometry = ProjectGeometry(e.Data.Geometry);
            var status = e.Data.Status;
            var morphology = e.Data.Morphology;
            var category = e.Data.Category;
            var geometryDrawMethod = e.Data.GeometryDrawMethod;
            var accessRestriction = e.Data.AccessRestriction;

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = geometry,
                StartNodeId = new RoadNodeId(e.Data.StartNodeId),
                EndNodeId = new RoadNodeId(e.Data.EndNodeId),
                GeometryDrawMethod = geometryDrawMethod,
                Status = status,
                AccessRestriction = ForEntireGeometry(accessRestriction, geometry.Lambert72),
                Category = ForEntireGeometry(category, geometry.Lambert72),
                Morphology = ForEntireGeometry(morphology, geometry.Lambert72),
                StreetNameId = BuildStreetNameIdAttributesFromV1(session, e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry.Lambert72, ct),
                MaintenanceAuthorityId = BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry.Lambert72, ct),
                SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition).RoundToCm()),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Both,
                        (string?)x.Type))
                ),
                CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(),
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

            await UpdateRoadNodeRoadSegmentIds(session, roadSegment.RoadSegmentId, (null, null), (roadSegment.StartNodeId, roadSegment.EndNodeId), ct);
            await SyncStreetNameLinks(session, roadSegment.RoadSegmentId, [], roadSegment.GetStreetNameHashSet(), ct);
            await SyncOrganizationLinks(session, roadSegment.RoadSegmentId, [], roadSegment.GetMaintenanceAuthorityHashSet(), ct);
        });
        When<IEvent<RoadSegmentAdded>>(async (session, e, ct) =>
        {
            var roadSegmentId = new RoadSegmentId(e.Data.RoadSegmentId);
            var geometry = ProjectGeometry(e.Data.Geometry);
            var status = e.Data.Status;
            var morphology = e.Data.Morphology;
            var category = e.Data.Category;
            var geometryDrawMethod = e.Data.GeometryDrawMethod;
            var accessRestriction = e.Data.AccessRestriction;

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = geometry,
                StartNodeId = new RoadNodeId(e.Data.StartNodeId),
                EndNodeId = new RoadNodeId(e.Data.EndNodeId),
                GeometryDrawMethod = geometryDrawMethod,
                Status = status,
                AccessRestriction = ForEntireGeometry(accessRestriction, geometry.Lambert72),
                Category = ForEntireGeometry(category, geometry.Lambert72),
                Morphology = ForEntireGeometry(morphology, geometry.Lambert72),
                StreetNameId = BuildStreetNameIdAttributesFromV1(session, e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry.Lambert72, ct),
                MaintenanceAuthorityId = BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry.Lambert72, ct),
                SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition).RoundToCm()),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Both,
                        (string?)x.Type))
                ),
                CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(),
                EuropeanRoadNumbers = [],
                NationalRoadNumbers = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = false
            };
            session.Store(roadSegment);

            await UpdateRoadNodeRoadSegmentIds(session, roadSegment.RoadSegmentId, (null, null), (roadSegment.StartNodeId, roadSegment.EndNodeId), ct);
            await SyncStreetNameLinks(session, roadSegment.RoadSegmentId, [], roadSegment.GetStreetNameHashSet(), ct);
            await SyncOrganizationLinks(session, roadSegment.RoadSegmentId, [], roadSegment.GetMaintenanceAuthorityHashSet(), ct);
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
                var geometry = ProjectGeometry(e.Data.Geometry);

                segment.Geometry = geometry;
                segment.StartNodeId = new RoadNodeId(e.Data.StartNodeId);
                segment.EndNodeId = new RoadNodeId(e.Data.EndNodeId);
                segment.GeometryDrawMethod = geometryDrawMethod;
                segment.Status = status;
                segment.AccessRestriction = ForEntireGeometry(accessRestriction, geometry.Lambert72);
                segment.Category = ForEntireGeometry(category, geometry.Lambert72);
                segment.Morphology = ForEntireGeometry(morphology, geometry.Lambert72);
                segment.StreetNameId = BuildStreetNameIdAttributesFromV1(session, e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, segment.Geometry.Lambert72, ct);
                segment.MaintenanceAuthorityId = BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry.Lambert72, ct);
                segment.SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition).RoundToCm()),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), segment.Geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Both,
                        (string?)x.Type))
                );
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentRemoved>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(e.Data.RoadSegmentId, ct);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"RoadSegment with id {e.Data.RoadSegmentId} is not found");
            }

            roadSegment.IsRemoved = true;
            session.Store(roadSegment);

            await UpdateRoadNodeRoadSegmentIds(session, roadSegment.RoadSegmentId, (roadSegment.StartNodeId, roadSegment.EndNodeId), (null, null), ct);
            await SyncStreetNameLinks(session, roadSegment.RoadSegmentId, roadSegment.GetStreetNameHashSet(), [], ct);
            await SyncOrganizationLinks(session, roadSegment.RoadSegmentId, roadSegment.GetMaintenanceAuthorityHashSet(), [], ct);
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
                    segment.AccessRestriction = ForEntireGeometry(e.Data.AccessRestriction, segment.Geometry.Lambert72);
                }

                if (e.Data.Category is not null)
                {
                    segment.Category = ForEntireGeometry(e.Data.Category, segment.Geometry.Lambert72);
                }

                if (e.Data.Morphology is not null)
                {
                    segment.Morphology = ForEntireGeometry(e.Data.Morphology, segment.Geometry.Lambert72);
                }

                if (e.Data.Status is not null)
                {
                    segment.Status = e.Data.Status;
                }

                if (e.Data.LeftSide is not null || e.Data.RightSide is not null)
                {
                    segment.StreetNameId = BuildStreetNameIdAttributesFromV1(
                        session,
                        e.Data.LeftSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Left),
                        e.Data.RightSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Right),
                        segment.Geometry.Lambert72,
                        ct);
                }

                if (e.Data.MaintenanceAuthority is not null)
                {
                    segment.MaintenanceAuthorityId = BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry.Lambert72, ct);
                }

                if (e.Data.Surfaces is not null)
                {
                    segment.SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                        .OrderBy(x => x.FromPosition)
                        .Select((x, i) => (
                            new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition).RoundToCm()),
                            UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), segment.Geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                            RoadSegmentAttributeSide.Both,
                            (string?)x.Type))
                    );
                }
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentGeometryModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                segment.Geometry = ProjectGeometry(e.Data.Geometry);
                segment.SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition).RoundToCm()),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), segment.Geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Both,
                        (string?)x.Type))
                );
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentStreetNamesChanged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment =>
            {
                if (e.Data.LeftSideStreetNameId is not null || e.Data.RightSideStreetNameId is not null)
                {
                    segment.StreetNameId = BuildStreetNameIdAttributesFromV1(
                        session,
                        e.Data.LeftSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Left),
                        e.Data.RightSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Right),
                        segment.Geometry.Lambert72,
                        ct);
                }
            }, e.Data, ct);
        });
        When<IEvent<OutlinedRoadSegmentRemoved>>((_, _, _) => Task.CompletedTask); // Do nothing
        When<IEvent<RoadSegmentAddedToNumberedRoad>>((_, _, _) => Task.CompletedTask); // Do nothing
        When<IEvent<RoadSegmentRemovedFromNumberedRoad>>((_, _, _) => Task.CompletedTask); // Do nothing

        // V2
        When<IEvent<RoadSegmentWasAdded>>(async (session, e, ct) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = ProjectGeometry(e.Data.Geometry),
                StartNodeId = e.Data.StartNodeId,
                EndNodeId = e.Data.EndNodeId,
                GeometryDrawMethod = e.Data.GeometryDrawMethod.ToString(),
                Status = e.Data.Status.ToString(),
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString()),
                StreetNameId = new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>(e.Data.StreetNameId.Values
                    .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (RoadSegmentStreetNameAttributeValue?)ToStreetNameAttributeValue(session, x.Value, ct)))),
                MaintenanceAuthorityId = BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x!.ToString()),
                CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection),
                BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection),
                PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection),
                EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList(),
                NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(roadSegment);

            await UpdateRoadNodeRoadSegmentIds(session, roadSegmentId, (null, null), (roadSegment.StartNodeId, roadSegment.EndNodeId), ct);
            await SyncStreetNameLinks(session, roadSegment.RoadSegmentId, [], roadSegment.GetStreetNameHashSet(), ct);
            await SyncOrganizationLinks(session, roadSegment.RoadSegmentId, [], roadSegment.GetMaintenanceAuthorityHashSet(), ct);
        });
        When<IEvent<RoadSegmentWasMerged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = ProjectGeometry(e.Data.Geometry);
                segment.StartNodeId = e.Data.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId;
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod.ToString();
                segment.Status = e.Data.Status.ToString();
                segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString());
                segment.Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString());
                segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString());
                segment.StreetNameId = new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>(e.Data.StreetNameId.Values
                    .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (RoadSegmentStreetNameAttributeValue?)ToStreetNameAttributeValue(session, x.Value, ct))));
                segment.MaintenanceAuthorityId = BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);
                segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x!.ToString());
                segment.CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection);
                segment.BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection);
                segment.PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection);
                segment.EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList();
                segment.NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList();
                segment.IsV2 = true;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentGeometryWasModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = ProjectGeometry(e.Data.Geometry);
                segment.StartNodeId = e.Data.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Geometry is not null ? ProjectGeometry(e.Data.Geometry) : segment.Geometry;
                segment.StartNodeId = e.Data.StartNodeId ?? segment.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId ?? segment.EndNodeId;
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod?.ToString() ?? segment.GeometryDrawMethod;
                segment.Status = e.Data.Status?.ToString() ?? segment.Status;

                if (e.Data.AccessRestriction is not null)
                {
                    segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString());
                }

                if (e.Data.Category is not null)
                {
                    segment.Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString());
                }

                if (e.Data.Morphology is not null)
                {
                    segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString());
                }

                if (e.Data.StreetNameId is not null)
                {
                    segment.StreetNameId = new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>(e.Data.StreetNameId.Values
                        .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (RoadSegmentStreetNameAttributeValue?)ToStreetNameAttributeValue(session, x.Value, ct))));
                }

                if (e.Data.MaintenanceAuthorityId is not null)
                {
                    segment.MaintenanceAuthorityId = BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);
                }

                if (e.Data.SurfaceType is not null)
                {
                    segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x!.ToString());
                }

                if (e.Data.CarTrafficDirection is not null)
                {
                    segment.CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection);
                }

                if (e.Data.BikeTrafficDirection is not null)
                {
                    segment.BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection);
                }

                if (e.Data.PedestrianTrafficDirection is not null)
                {
                    segment.PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection);
                }
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasMigrated>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = ProjectGeometry(e.Data.Geometry);
                segment.StartNodeId = e.Data.StartNodeId;
                segment.EndNodeId = e.Data.EndNodeId;
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod.ToString();
                segment.Status = e.Data.Status.ToString();
                segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString());
                segment.Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString());
                segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString());
                segment.StreetNameId = new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>(e.Data.StreetNameId.Values
                    .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (RoadSegmentStreetNameAttributeValue?)ToStreetNameAttributeValue(session, x.Value, ct))));
                segment.MaintenanceAuthorityId = BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);
                segment.SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x!.ToString());
                segment.CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection);
                segment.BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection);
                segment.PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection);
                segment.EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList();
                segment.NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList();
                segment.IsV2 = true;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasRemoved>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(e.Data.RoadSegmentId, ct);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No road segment found for Id {e.Data.RoadSegmentId}");
            }

            roadSegment.IsRemoved = true;
            session.Store(roadSegment);

            await UpdateRoadNodeRoadSegmentIds(session, roadSegment.RoadSegmentId, (roadSegment.StartNodeId, roadSegment.EndNodeId), (null, null), ct);
            await SyncStreetNameLinks(session, roadSegment.RoadSegmentId, roadSegment.GetStreetNameHashSet(), [], ct);
            await SyncOrganizationLinks(session, roadSegment.RoadSegmentId, roadSegment.GetMaintenanceAuthorityHashSet(), [], ct);
        });
        When<IEvent<RoadSegmentWasRemovedBecauseOfMigration>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(e.Data.RoadSegmentId, ct);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No road segment found for Id {e.Data.RoadSegmentId}");
            }

            roadSegment.IsRemoved = true;
            session.Store(roadSegment);

            await UpdateRoadNodeRoadSegmentIds(session, roadSegment.RoadSegmentId, (roadSegment.StartNodeId, roadSegment.EndNodeId), (null, null), ct);
            await SyncStreetNameLinks(session, roadSegment.RoadSegmentId, roadSegment.GetStreetNameHashSet(), [], ct);
            await SyncOrganizationLinks(session, roadSegment.RoadSegmentId, roadSegment.GetMaintenanceAuthorityHashSet(), [], ct);
        });
        When<IEvent<RoadSegmentWasRetired>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Status = RoadSegmentStatusV2.Gehistoreerd.ToString();
                segment.StartNodeId = null;
                segment.EndNodeId = null;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasRetiredBecauseOfMerger>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Status = RoadSegmentStatusV2.Gehistoreerd.ToString();
                segment.StartNodeId = null;
                segment.EndNodeId = null;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });

        // StreetName
        When<IEvent<StreetNameWasCreated>>(async (session, e, ct) =>
        {
            session.Store(new StreetNameReadItem
            {
                StreetNameId = e.Data.StreetNameId,
                DutchName = e.Data.DutchName,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });

            await UpdateStreetNameLabels(session, e.Data.StreetNameId, e.Data.DutchName, ct);
        });
        When<IEvent<StreetNameWasModified>>(async (session, e, ct) =>
        {
            var streetName = await session.LoadAsync<StreetNameReadItem>(e.Data.StreetNameId, ct)
                             ?? new StreetNameReadItem
                             {
                                 StreetNameId = e.Data.StreetNameId,
                                 DutchName = e.Data.DutchName,
                                 Origin = e.Data.Provenance.ToEventTimestamp(),
                                 LastModified = e.Data.Provenance.ToEventTimestamp()
                             };
            streetName.DutchName = e.Data.DutchName;
            streetName.NisCode = e.Data.NisCode;
            streetName.Status = e.Data.Status;
            streetName.LastModified = e.Data.Provenance.ToEventTimestamp();
            session.Store(streetName);

            await UpdateStreetNameLabels(session, e.Data.StreetNameId, e.Data.DutchName, ct);
        });
        When<IEvent<StreetNameWasRemoved>>(async (session, e, ct) =>
        {
            var streetName = await session.LoadAsync<StreetNameReadItem>(e.Data.StreetNameId, ct);
            if (streetName is not null)
            {
                streetName.IsRemoved = true;
                streetName.LastModified = e.Data.Provenance.ToEventTimestamp();
                session.Store(streetName);
            }

            await UpdateStreetNameLabels(session, e.Data.StreetNameId, null, ct);
        });

        // Organization
        When<IEvent<OrganizationWasCreated>>((session, e, ct) => UpdateMaintenanceAuthorityNames(session, e.Data.OrganizationId, e.Data.Name, ct));
        When<IEvent<OrganizationWasModified>>((session, e, ct) =>
            e.Data.Name is not null
                ? UpdateMaintenanceAuthorityNames(session, e.Data.OrganizationId, e.Data.Name, ct)
                : Task.CompletedTask);
        When<IEvent<OrganizationWasRemoved>>((_, _, _) => Task.CompletedTask);
    }

    private async Task UpdateStreetNameLabels(IDocumentOperations session, StreetNameLocalId streetNameId, string? dutchName, CancellationToken ct)
    {
        var link = await session.LoadAsync<StreetNameRoadSegmentsLink>(streetNameId, ct);
        if (link is null || link.RoadSegmentIds.Count == 0)
        {
            return;
        }

        var segments = await session.LoadManyAsync<RoadSegmentReadItem>(ct, link.RoadSegmentIds.Select(x => x.ToInt32()).ToArray());
        foreach (var segment in segments)
        {
            var changed = false;

            foreach (var value in segment.StreetNameId.Values)
            {
                if (value.Value is not null && value.Value.StreetNameId == streetNameId)
                {
                    value.Value.DutchName = dutchName;
                    changed = true;
                }
            }

            if (changed)
            {
                // Do not update LastModified of the segment
                session.Store(segment);
            }
        }
    }

    private async Task UpdateRoadNodeRoadSegmentIds(
        IDocumentOperations session,
        RoadSegmentId roadSegmentId,
        (RoadNodeId? Start, RoadNodeId? End) originalStartEndNodeIds,
        (RoadNodeId? Start, RoadNodeId? End) updatedStartEndNodeIds,
        CancellationToken ct)
    {
        var nodeIds = new[]
            {
                originalStartEndNodeIds.Start,
                originalStartEndNodeIds.End,
                updatedStartEndNodeIds.Start,
                updatedStartEndNodeIds.End
            }
            .Where(x => x is not null)
            .Select(x => x!.Value.ToInt32())
            .Distinct()
            .ToArray();

        var nodes = await session.LoadManyAsync<RoadNodeReadItem>(ct, nodeIds);

        if (originalStartEndNodeIds.Start is not null)
        {
            var node = nodes.SingleOrDefault(x => x.RoadNodeId == originalStartEndNodeIds.Start.Value)
                ?? throw new InvalidOperationException($"No road node found for Id {originalStartEndNodeIds.Start.Value}");
            node.RoadSegmentIds = node.RoadSegmentIds.Except([roadSegmentId]).ToArray();
            session.Store(node);
        }
        if (originalStartEndNodeIds.End is not null)
        {
            var node = nodes.SingleOrDefault(x => x.RoadNodeId == originalStartEndNodeIds.End.Value)
                       ?? throw new InvalidOperationException($"No road node found for Id {originalStartEndNodeIds.End.Value}");
            node.RoadSegmentIds = node.RoadSegmentIds.Except([roadSegmentId]).ToArray();
            session.Store(node);
        }

        if (updatedStartEndNodeIds.Start is not null)
        {
            var node = nodes.SingleOrDefault(x => x.RoadNodeId == updatedStartEndNodeIds.Start.Value)
                       ?? throw new InvalidOperationException($"No road node found for Id {updatedStartEndNodeIds.Start.Value}");
            node.RoadSegmentIds = node.RoadSegmentIds.Union([roadSegmentId]).OrderBy(x => x).ToArray();
            session.Store(node);
        }
        if (updatedStartEndNodeIds.End is not null)
        {
            var node = nodes.SingleOrDefault(x => x.RoadNodeId == updatedStartEndNodeIds.End.Value)
                       ?? throw new InvalidOperationException($"No road node found for Id {updatedStartEndNodeIds.End.Value}");
            node.RoadSegmentIds = node.RoadSegmentIds.Union([roadSegmentId]).OrderBy(x => x).ToArray();
            session.Store(node);
        }
    }

    private static async Task SyncStreetNameLinks(
        IDocumentOperations session,
        RoadSegmentId roadSegmentId,
        HashSet<StreetNameLocalId> originalStreetNameIds,
        HashSet<StreetNameLocalId> updatedStreetNameIds,
        CancellationToken ct)
    {
        var oldStreetNameIds = originalStreetNameIds.ToHashSet();

        foreach (var streetNameId in oldStreetNameIds.Except(updatedStreetNameIds))
        {
            var streetNameRoadSegments = await session.LoadAsync<StreetNameRoadSegmentsLink>(streetNameId, ct);
            if (streetNameRoadSegments is null)
            {
                continue;
            }

            streetNameRoadSegments.RoadSegmentIds = streetNameRoadSegments.RoadSegmentIds.Except([roadSegmentId]).ToList();
            if (streetNameRoadSegments.RoadSegmentIds.Count == 0)
            {
                session.Delete(streetNameRoadSegments);
            }
            else
            {
                session.Store(streetNameRoadSegments);
            }
        }

        foreach (var streetNameId in updatedStreetNameIds.Except(oldStreetNameIds))
        {
            var streetNameRoadSegmentsLink = await session.LoadAsync<StreetNameRoadSegmentsLink>(streetNameId, ct)
                                         ?? new StreetNameRoadSegmentsLink { StreetNameId = streetNameId, RoadSegmentIds = [] };

            streetNameRoadSegmentsLink.RoadSegmentIds = streetNameRoadSegmentsLink.RoadSegmentIds.Union([roadSegmentId]).ToList();
            session.Store(streetNameRoadSegmentsLink);
        }
    }

    private static async Task SyncOrganizationLinks(
        IDocumentOperations session,
        RoadSegmentId roadSegmentId,
        HashSet<OrganizationId> originalOrganizationIds,
        HashSet<OrganizationId> updatedOrganizationIds,
        CancellationToken ct)
    {
        foreach (var organizationId in originalOrganizationIds.Except(updatedOrganizationIds))
        {
            var link = await session.LoadAsync<OrganizationRoadSegmentsLink>(organizationId.ToString(), ct);
            if (link is null)
            {
                continue;
            }

            link.RoadSegmentIds = link.RoadSegmentIds.Except([roadSegmentId]).ToList();
            if (link.RoadSegmentIds.Count == 0)
            {
                session.Delete(link);
            }
            else
            {
                session.Store(link);
            }
        }

        foreach (var organizationId in updatedOrganizationIds.Except(originalOrganizationIds))
        {
            var link = await session.LoadAsync<OrganizationRoadSegmentsLink>(organizationId.ToString(), ct)
                       ?? new OrganizationRoadSegmentsLink { OrganizationId = organizationId, RoadSegmentIds = [] };

            link.RoadSegmentIds = link.RoadSegmentIds.Union([roadSegmentId]).ToList();
            session.Store(link);
        }
    }

    private async Task UpdateMaintenanceAuthorityNames(IDocumentOperations session, OrganizationId organizationId, string? name, CancellationToken ct)
    {
        var link = await session.LoadAsync<OrganizationRoadSegmentsLink>(organizationId.ToString(), ct);
        if (link is null || link.RoadSegmentIds.Count == 0)
        {
            return;
        }

        var segments = await session.LoadManyAsync<RoadSegmentReadItem>(ct, link.RoadSegmentIds.Select(x => x.ToInt32()).ToArray());
        foreach (var segment in segments)
        {
            var changed = false;

            foreach (var value in segment.MaintenanceAuthorityId.Values)
            {
                if (value.Value is not null && value.Value.OrganizationId == organizationId)
                {
                    value.Value.Name = name;
                    changed = true;
                }
            }

            if (changed)
            {
                // Do not update LastModified of the segment
                session.Store(segment);
            }
        }
    }

    private ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue> BuildMaintenanceAuthority(IDocumentOperations session, OrganizationId organizationId, RoadSegmentGeometry geometry, CancellationToken ct)
    {
        return ForEntireGeometry(ToMaintenanceAuthorityAttributeValue(session, organizationId, ct), geometry);
    }

    private ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue> BuildMaintenanceAuthority(IDocumentOperations session, RoadSegmentDynamicAttributeValues<OrganizationId> attributes, CancellationToken ct)
    {
        return new ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue>(attributes.Values
            .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (RoadSegmentMaintenanceAuthorityAttributeValue?)ToMaintenanceAuthorityAttributeValue(session, x.Value, ct))));
    }

    private static RoadSegmentMaintenanceAuthorityAttributeValue ToMaintenanceAuthorityAttributeValue(IDocumentOperations session, OrganizationId organizationId, CancellationToken ct)
    {
        var organization = session.LoadAsync<OrganizationReadItem>(organizationId.ToString(), ct).ConfigureAwait(false).GetAwaiter().GetResult();
        return new RoadSegmentMaintenanceAuthorityAttributeValue
        {
            OrganizationId = organizationId,
            Name = organization?.Name
        };
    }

    private async Task ModifyRoadSegment<TEvent>(IDocumentOperations operations, RoadSegmentId roadSegmentId, Action<RoadSegmentReadItem> modify, TEvent evt, CancellationToken ct)
        where TEvent : IMartenEvent
    {
        var roadSegment = await operations.LoadAsync<RoadSegmentReadItem>(roadSegmentId, ct);
        if (roadSegment is null)
        {
            throw new InvalidOperationException($"No road segment found for Id {roadSegmentId}");
        }

        var originalStartEndNodeIds = (roadSegment.StartNodeId, roadSegment.EndNodeId);
        var originalStreetNameIds = roadSegment.GetStreetNameHashSet();
        var originalMaintenanceAuthorityIds = roadSegment.GetMaintenanceAuthorityHashSet();

        modify(roadSegment);

        var updatedStartEndNodeIds = (roadSegment.StartNodeId, roadSegment.EndNodeId);
        if (originalStartEndNodeIds != updatedStartEndNodeIds)
        {
            await UpdateRoadNodeRoadSegmentIds(operations, roadSegment.RoadSegmentId, originalStartEndNodeIds, updatedStartEndNodeIds, ct);
        }

        var updatedStreetNameIds = roadSegment.GetStreetNameHashSet();
        if (!originalStreetNameIds.SetEquals(updatedStreetNameIds))
        {
            await SyncStreetNameLinks(operations, roadSegment.RoadSegmentId, originalStreetNameIds, updatedStreetNameIds, ct);
        }

        var updatedMaintenanceAuthorityIds = roadSegment.GetMaintenanceAuthorityHashSet();
        if (!originalMaintenanceAuthorityIds.SetEquals(updatedMaintenanceAuthorityIds))
        {
            await SyncOrganizationLinks(operations, roadSegment.RoadSegmentId, originalMaintenanceAuthorityIds, updatedMaintenanceAuthorityIds, ct);
        }

        roadSegment.LastModified = evt.Provenance.ToEventTimestamp();
        operations.Store(roadSegment);
    }

    private static RoadSegmentStreetNameAttributeValue ToStreetNameAttributeValue(IDocumentOperations session, int? streetNameId, CancellationToken ct)
    {
        return new RoadSegmentStreetNameAttributeValue
        {
            StreetNameId = streetNameId is not null ? new StreetNameLocalId(streetNameId.Value) : StreetNameLocalId.NotApplicable,
            DutchName = GetStreetName(session, streetNameId, ct)
        };
    }
    private static ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue> BuildStreetNameIdAttributesFromV1(IDocumentOperations session, int? leftSideStreetNameId, int? rightSideStreetNameId, RoadSegmentGeometry geometry, CancellationToken ct)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return ForEntireGeometry(ToStreetNameAttributeValue(session, StreetNameLocalId.NotApplicable, ct), geometry);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return ForEntireGeometry(ToStreetNameAttributeValue(session, leftSideStreetNameId!.Value, ct), geometry);
        }

        return new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>([
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Left, ToStreetNameAttributeValue(session, leftSideStreetNameId, ct)),
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Right, ToStreetNameAttributeValue(session, rightSideStreetNameId, ct))
        ]);
    }

    private static ReadRoadSegmentDynamicAttribute<T> ForEntireGeometry<T>(T value, RoadSegmentGeometry geometry)
        where T : notnull
    {
        return new ReadRoadSegmentDynamicAttribute<T>([(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Both, value)]);
    }

    private static RoadSegmentPositionV2 UseGeometryLengthIfPositionIsLast(double position, RoadSegmentGeometry geometry, bool isLast)
    {
        return isLast
            ? new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm())
            : new RoadSegmentPositionV2(position.RoundToCm());
    }

    private static StreetNameLocalId GetValue(ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue> attributes, RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => attributes.Values.Single(x => x.Side is RoadSegmentAttributeSide.Both or RoadSegmentAttributeSide.Left).Value!.StreetNameId,
            RoadSegmentAttributeSide.Right => attributes.Values.Single(x => x.Side is RoadSegmentAttributeSide.Both or RoadSegmentAttributeSide.Right).Value!.StreetNameId,
            _ => throw new InvalidOperationException("Only left or right side is allowed.")
        };
    }

    private static RoadSegmentGeometryProjections ProjectGeometry(RoadSegmentGeometry geometry)
    {
        return new RoadSegmentGeometryProjections
        {
            Lambert72 = geometry.EnsureLambert72(),
            Lambert08 = geometry.EnsureLambert08(),
        };
    }

    private static string? GetStreetName(
        IDocumentOperations session,
        int? streetNameId,
        CancellationToken cancellationToken)
    {
        if (streetNameId is null || streetNameId == StreetNameLocalId.NotApplicable || streetNameId == StreetNameLocalId.Unknown)
        {
            return null;
        }

        var streetName = session.LoadAsync<StreetNameReadItem>(streetNameId.Value, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        return streetName?.DutchName;
    }
}

public sealed class RoadSegmentGeometryProjections
{
    public required RoadSegmentGeometry Lambert72 { get; init; }
    public required RoadSegmentGeometry Lambert08 { get; init; }
}

public sealed class RoadSegmentReadItem
{
    [JsonIgnore] public int Id { get; private set; }

    public required RoadSegmentId RoadSegmentId
    {
        get => new(Id);
        set => Id = value;
    }

    public required RoadSegmentGeometryProjections Geometry { get; set; }
    public required RoadNodeId? StartNodeId { get; set; }
    public required RoadNodeId? EndNodeId { get; set; }
    public required string Status { get; set; }
    public required string GeometryDrawMethod { get; set; }
    public required ReadRoadSegmentDynamicAttribute<string> AccessRestriction { get; set; }
    public required ReadRoadSegmentDynamicAttribute<string> Category { get; set; }
    public required ReadRoadSegmentDynamicAttribute<string> Morphology { get; set; }
    public required ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue> StreetNameId { get; set; }
    public required ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue> MaintenanceAuthorityId { get; set; }
    public required ReadRoadSegmentDynamicAttribute<string> SurfaceType { get; set; }
    public required ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection> CarTrafficDirection { get; set; }
    public required ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection> BikeTrafficDirection { get; set; }
    public required ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection> PedestrianTrafficDirection { get; set; }
    public required List<EuropeanRoadNumber> EuropeanRoadNumbers { get; set; }
    public required List<NationalRoadNumber> NationalRoadNumbers { get; set; }

    public IReadOnlyCollection<GradeJunctionId> GradeJunctionIds { get; set; } = [];
    public IReadOnlyCollection<GradeSeparatedJunctionId> GradeSeparatedJunctionIds { get; set; } = [];

    public required EventTimestamp Origin { get; set; }
    public required EventTimestamp LastModified { get; set; }
    public required bool IsV2 { get; set; }
    public bool IsRemoved { get; set; }

    public HashSet<StreetNameLocalId> GetStreetNameHashSet()
    {
        return StreetNameId.Values
            .Select(x => x.Value?.StreetNameId)
            .Where(x => !StreetNameLocalId.IsEmpty(x))
            .Select(x => x!.Value)
            .ToHashSet();
    }

    public HashSet<OrganizationId> GetMaintenanceAuthorityHashSet()
    {
        return MaintenanceAuthorityId.Values
            .Where(x => x.Value is not null)
            .Select(x => x.Value!.OrganizationId)
            .ToHashSet();
    }
}

public sealed class RoadSegmentStreetNameAttributeValue
{
    public required StreetNameLocalId StreetNameId { get; set; }
    public required string? DutchName { get; set; }
}

public sealed class StreetNameReadItem
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required StreetNameLocalId StreetNameId
    {
        get => new(Id);
        set => Id = value;
    }

    public required string? DutchName { get; set; }
    public string? NisCode { get; set; }
    public string? Status { get; set; }

    public required EventTimestamp Origin { get; set; }
    public required EventTimestamp LastModified { get; set; }
    public bool IsRemoved { get; set; }
}

public sealed class RoadSegmentMaintenanceAuthorityAttributeValue
{
    public required OrganizationId OrganizationId { get; set; }
    public required string? Name { get; set; }
}

public sealed class ReadRoadSegmentDynamicAttribute<T>
    where T : notnull
{
    public List<ReadRoadSegmentDynamicAttributeValue<T>> Values { get; set; } = [];

    public ReadRoadSegmentDynamicAttribute()
    {
    }

    public ReadRoadSegmentDynamicAttribute(RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<T> attributes)
        : this(attributes.Values.Select(x => (x.Coverage.From, x.Coverage.To, x.Side, x.Value)))
    {
    }

    public ReadRoadSegmentDynamicAttribute(IEnumerable<(RoadSegmentPositionV2 From, RoadSegmentPositionV2 To, RoadSegmentAttributeSide Side, T? Value)> values)
    {
        Values = values
            .OrderBy(x => x.From)
            .Select(x => new ReadRoadSegmentDynamicAttributeValue<T>
            {
                From = x.From,
                To = x.To,
                Side = x.Side,
                Value = x.Value
            })
            .ToList();
    }
}

public sealed class ReadRoadSegmentDynamicAttributeValue<T>
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentAttributeSide Side { get; set; }

    public required RoadSegmentPositionV2 From { get; init; }
    public required RoadSegmentPositionV2 To { get; init; }
    public required T? Value { get; init; }
}

internal static class RoadSegmentDynamicAttributeValuesExtensions
{
    public static ReadRoadSegmentDynamicAttribute<string> ToStringAttributeValues<T>(this RoadSegmentDynamicAttributeValues<T> attributes, Func<T?, string?> converter)
        where T : notnull
    {
        return new ReadRoadSegmentDynamicAttribute<string>(attributes.Values.Select(x => (x.Coverage.From, x.Coverage.To, x.Side, converter(x.Value))));
    }
}

public sealed class StreetNameRoadSegmentsLink
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required StreetNameLocalId StreetNameId
    {
        get => new(Id);
        set => Id = value;
    }

    public required List<RoadSegmentId> RoadSegmentIds { get; set; }
}

public sealed class OrganizationRoadSegmentsLink
{
    [JsonIgnore]
    public string Id { get; private set; }

    public required OrganizationId OrganizationId
    {
        get => new(Id);
        set => Id = value.ToString();
    }

    public required List<RoadSegmentId> RoadSegmentIds { get; set; }
}
