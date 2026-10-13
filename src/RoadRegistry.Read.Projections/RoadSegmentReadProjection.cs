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
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.StreetName;
using RoadRegistry.ValueObjects;
using RoadSegment.Events.V1;
using RoadSegment.Events.V2;
using RoadSegment.ValueObjects;
using RoadRegistry.StreetName.Events.V2;
using Weasel.Postgresql.Tables;

public class RoadSegmentReadProjection : MartenRoadNetworkChangesProjection
{
    private readonly IStreetNameClient _streetNameClient;
    private readonly ILogger _logger;

    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_roadsegments")
            .Identity(x => x.Id)
            // Everything this document points at, as indexed columns on its own table. A road node or a street name
            // finds the segments that reference it by querying these columns - see ReadModelQueries - so no document
            // has to keep a list of the documents pointing back at it.
            .Duplicate(x => x.StartNodeId, configure: index => { index.Name = "ix_read_roadsegments_startnodeid"; })
            .Duplicate(x => x.EndNodeId, configure: index => { index.Name = "ix_read_roadsegments_endnodeid"; })
            // Nullable, unlike the others: Marten adds a duplicated column with a plain ALTER TABLE ADD COLUMN
            // followed by an UPDATE that fills it, and Postgres rejects adding a NOT NULL column without a default
            // to a table that already has rows. The members themselves are never null - an empty array at worst.
            .Duplicate(x => x.StreetNameIds, configure: index =>
            {
                index.Name = "ix_read_roadsegments_streetnameids";
                index.Method = IndexMethod.gin;
            })
            ;
    }

    public RoadSegmentReadProjection(IStreetNameClient streetNameClient, ILogger<RoadSegmentReadProjection> logger)
    {
        _streetNameClient = streetNameClient.ThrowIfNull();
        _logger = logger.ThrowIfNull();

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

            var streetNameId = await BuildStreetNameIdAttributesFromV1(session, e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry.Lambert72, ct);
            var maintenanceAuthorityId = await BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry.Lambert72, ct);

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = geometry,
                StartNodeId = e.Data.StartNodeId > 0 ? e.Data.StartNodeId : null,
                EndNodeId = e.Data.EndNodeId > 0 ? e.Data.EndNodeId : null,
                GeometryDrawMethod = geometryDrawMethod,
                Status = status,
                AccessRestriction = ForEntireGeometry(accessRestriction, geometry.Lambert72),
                Category = ForEntireGeometry(category, geometry.Lambert72),
                Morphology = ForEntireGeometry(morphology, geometry.Lambert72),
                StreetNameId = streetNameId,
                MaintenanceAuthorityId = maintenanceAuthorityId,
                SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(x.FromPosition),
                        UseGeometryLengthIfPositionIsLast(x.ToPosition, geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Beide,
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
            await AddOrUpdateRoadSegment(session, roadSegment, ct);
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

            var streetNameId = await BuildStreetNameIdAttributesFromV1(session, e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry.Lambert72, ct);
            var maintenanceAuthorityId = await BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry.Lambert72, ct);

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = geometry,
                StartNodeId = e.Data.StartNodeId > 0 ? e.Data.StartNodeId : null,
                EndNodeId = e.Data.EndNodeId > 0 ? e.Data.EndNodeId : null,
                GeometryDrawMethod = geometryDrawMethod,
                Status = status,
                AccessRestriction = ForEntireGeometry(accessRestriction, geometry.Lambert72),
                Category = ForEntireGeometry(category, geometry.Lambert72),
                Morphology = ForEntireGeometry(morphology, geometry.Lambert72),
                StreetNameId = streetNameId,
                MaintenanceAuthorityId = maintenanceAuthorityId,
                SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(x.FromPosition),
                        UseGeometryLengthIfPositionIsLast(x.ToPosition, geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Beide,
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
            await AddOrUpdateRoadSegment(session, roadSegment, ct);
        });
        When<IEvent<RoadSegmentModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), async segment =>
            {
                var status = e.Data.Status;
                var morphology = e.Data.Morphology;
                var category = e.Data.Category;
                var geometryDrawMethod = e.Data.GeometryDrawMethod;
                var accessRestriction = e.Data.AccessRestriction;
                var geometry = ProjectGeometry(e.Data.Geometry);

                segment.Geometry = geometry;
                segment.StartNodeId = e.Data.StartNodeId > 0 ? e.Data.StartNodeId : null;
                segment.EndNodeId = e.Data.EndNodeId > 0 ? e.Data.EndNodeId : null;
                segment.GeometryDrawMethod = geometryDrawMethod;
                segment.Status = status;
                segment.AccessRestriction = ForEntireGeometry(accessRestriction, geometry.Lambert72);
                segment.Category = ForEntireGeometry(category, geometry.Lambert72);
                segment.Morphology = ForEntireGeometry(morphology, geometry.Lambert72);
                segment.StreetNameId = await BuildStreetNameIdAttributesFromV1(session, e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, segment.Geometry.Lambert72, ct);
                segment.MaintenanceAuthorityId = await BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry.Lambert72, ct);
                segment.SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(x.FromPosition),
                        UseGeometryLengthIfPositionIsLast(x.ToPosition, segment.Geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Beide,
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
        });
        When<IEvent<RoadSegmentAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.EuropeanRoadNumbers.Add(EuropeanRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.NationalRoadNumbers.Add(NationalRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.EuropeanRoadNumbers.Remove(EuropeanRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), segment => { segment.NationalRoadNumbers.Remove(NationalRoadNumber.Parse(e.Data.Number)); }, e.Data, ct); });
        When<IEvent<RoadSegmentAttributesModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), async segment =>
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
                    segment.StreetNameId = await BuildStreetNameIdAttributesFromV1(
                        session,
                        e.Data.LeftSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Links),
                        e.Data.RightSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Rechts),
                        segment.Geometry.Lambert72,
                        ct);
                }

                if (e.Data.MaintenanceAuthority is not null)
                {
                    segment.MaintenanceAuthorityId = await BuildMaintenanceAuthority(session, new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry.Lambert72, ct);
                }

                if (e.Data.Surfaces is not null)
                {
                    segment.SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                        .OrderBy(x => x.FromPosition)
                        .Select((x, i) => (
                            new RoadSegmentPositionV2(x.FromPosition),
                            UseGeometryLengthIfPositionIsLast(x.ToPosition, segment.Geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                            RoadSegmentAttributeSide.Beide,
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
                        new RoadSegmentPositionV2(x.FromPosition),
                        UseGeometryLengthIfPositionIsLast(x.ToPosition, segment.Geometry.Lambert72, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Beide,
                        (string?)x.Type))
                );
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentStreetNamesChanged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, new RoadSegmentId(e.Data.RoadSegmentId), async segment =>
            {
                if (e.Data.LeftSideStreetNameId is not null || e.Data.RightSideStreetNameId is not null)
                {
                    segment.StreetNameId = await BuildStreetNameIdAttributesFromV1(
                        session,
                        e.Data.LeftSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Links),
                        e.Data.RightSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Rechts),
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

            var streetNameId = await BuildStreetNameAttribute(session, e.Data.StreetNameId, ct);
            var maintenanceAuthorityId = await BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = ProjectGeometry(e.Data.Geometry),
                StartNodeId = e.Data.StartNodeId?.ToInt32(),
                EndNodeId = e.Data.EndNodeId?.ToInt32(),
                GeometryDrawMethod = e.Data.GeometryDrawMethod.ToString(),
                Status = e.Data.Status.ToString(),
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString()),
                StreetNameId = streetNameId,
                MaintenanceAuthorityId = maintenanceAuthorityId,
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
            await AddOrUpdateRoadSegment(session, roadSegment, ct);
        });
        When<IEvent<OutlinedRoadSegmentWasAdded>>(async (session, e, ct) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var streetNameId = await BuildStreetNameAttribute(session, e.Data.StreetNameId, ct);
            var maintenanceAuthorityId = await BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);

            var roadSegment = new RoadSegmentReadItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = ProjectGeometry(e.Data.Geometry),
                StartNodeId = null,
                EndNodeId = null,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst.ToString(),
                Status = e.Data.Status.ToString(),
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString()),
                StreetNameId = streetNameId,
                MaintenanceAuthorityId = maintenanceAuthorityId,
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x!.ToString()),
                CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection),
                BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection),
                PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection),
                EuropeanRoadNumbers = [],
                NationalRoadNumbers = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            await AddOrUpdateRoadSegment(session, roadSegment, ct);
        });
        When<IEvent<RoadSegmentWasMerged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, async segment =>
            {
                segment.Geometry = ProjectGeometry(e.Data.Geometry);
                segment.StartNodeId = e.Data.StartNodeId?.ToInt32();
                segment.EndNodeId = e.Data.EndNodeId?.ToInt32();
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod.ToString();
                segment.Status = e.Data.Status.ToString();
                segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString());
                segment.Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString());
                segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString());
                segment.StreetNameId = await BuildStreetNameAttribute(session, e.Data.StreetNameId, ct);
                segment.MaintenanceAuthorityId = await BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);
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
                segment.StartNodeId = e.Data.StartNodeId?.ToInt32();
                segment.EndNodeId = e.Data.EndNodeId?.ToInt32();
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasModified>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, async segment =>
            {
                segment.Geometry = e.Data.Geometry is not null ? ProjectGeometry(e.Data.Geometry) : segment.Geometry;
                if (e.Data.NodeIds is not null)
                {
                    segment.StartNodeId = e.Data.NodeIds.Start?.ToInt32();
                    segment.EndNodeId = e.Data.NodeIds.End?.ToInt32();
                }
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
                    segment.StreetNameId = await BuildStreetNameAttribute(session, e.Data.StreetNameId, ct);
                }

                if (e.Data.MaintenanceAuthorityId is not null)
                {
                    segment.MaintenanceAuthorityId = await BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);
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
        // The status changes. Each transition raises an event of its own - the status a segment takes on is the event
        // itself - but what a projection has to do with one follows entirely from the kind of change it is, so the
        // three shapes below are all there is. RoadSegmentStatusChange.ForEvent supplies the status.
        When<IEvent<RoadSegmentWasRealizedFromPlanned>>((session, e, ct) => ProjectConnected(session, e.Data, ct));
        When<IEvent<RoadSegmentWasRealizedFromOutOfUse>>((session, e, ct) => ProjectConnected(session, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromHistorizedToRealized>>((session, e, ct) => ProjectConnected(session, e.Data, ct));

        When<IEvent<RoadSegmentWasCorrectedFromRealizedToPlanned>>((session, e, ct) => ProjectDisconnected(session, e.Data, ct));
        When<IEvent<RoadSegmentWasTakenOutOfUseFromRealized>>((session, e, ct) => ProjectDisconnected(session, e.Data, ct));
        When<IEvent<RoadSegmentWasHistorizedFromRealized>>((session, e, ct) => ProjectDisconnected(session, e.Data, ct));

        When<IEvent<RoadSegmentWasHistorizedFromOutOfUse>>((session, e, ct) => ProjectUnconnectedStatusChange(session, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromNotRealizedToPlanned>>((session, e, ct) => ProjectUnconnectedStatusChange(session, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromHistorizedToOutOfUse>>((session, e, ct) => ProjectUnconnectedStatusChange(session, e.Data, ct));
        When<IEvent<RoadSegmentWasNotRealizedFromPlanned>>((session, e, ct) => ProjectUnconnectedStatusChange(session, e.Data, ct));
        When<IEvent<RoadSegmentWasMigrated>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, async segment =>
            {
                segment.Geometry = ProjectGeometry(e.Data.Geometry);
                segment.StartNodeId = e.Data.StartNodeId?.ToInt32();
                segment.EndNodeId = e.Data.EndNodeId?.ToInt32();
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod.ToString();
                segment.Status = e.Data.Status.ToString();
                segment.AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x!.ToString());
                segment.Category = e.Data.Category.ToStringAttributeValues(x => x!.ToString());
                segment.Morphology = e.Data.Morphology.ToStringAttributeValues(x => x!.ToString());
                segment.StreetNameId = await BuildStreetNameAttribute(session, e.Data.StreetNameId, ct);
                segment.MaintenanceAuthorityId = await BuildMaintenanceAuthority(session, e.Data.MaintenanceAuthorityId, ct);
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
        When<IEvent<RoadSegmentWasRetiredBecauseOfSplit>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Status = RoadSegmentStatusV2.Gehistoreerd.ToString();
                segment.StartNodeId = null;
                segment.EndNodeId = null;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasSplit>>((session, e, ct) =>
        {
            // Only the segment that keeps its identifier (largest part) carries modifications.
            if (e.Data.Modifications is null)
            {
                return Task.CompletedTask;
            }

            return ModifyRoadSegment(session, e.Data.RoadSegmentId, async segment =>
            {
                segment.Geometry = ProjectGeometry(e.Data.Modifications.Geometry);
                segment.StartNodeId = e.Data.Modifications.StartNodeId?.ToInt32();
                segment.EndNodeId = e.Data.Modifications.EndNodeId?.ToInt32();
                segment.AccessRestriction = e.Data.Modifications.AccessRestriction.ToStringAttributeValues(x => x!.ToString());
                segment.Category = e.Data.Modifications.Category.ToStringAttributeValues(x => x!.ToString());
                segment.Morphology = e.Data.Modifications.Morphology.ToStringAttributeValues(x => x!.ToString());
                segment.StreetNameId = await BuildStreetNameAttribute(session, e.Data.Modifications.StreetNameId, ct);
                segment.MaintenanceAuthorityId = await BuildMaintenanceAuthority(session, e.Data.Modifications.MaintenanceAuthorityId, ct);
                segment.SurfaceType = e.Data.Modifications.SurfaceType.ToStringAttributeValues(x => x!.ToString());
                segment.CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.Modifications.CarTrafficDirection);
                segment.BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.Modifications.BikeTrafficDirection);
                segment.PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.Modifications.PedestrianTrafficDirection);
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentStreetNameIdWasChanged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, async segment =>
            {
                segment.StreetNameId = await BuildStreetNameAttribute(session, e.Data.StreetNameId, ct);
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentGeometryDrawMethodWasChanged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.GeometryDrawMethod = e.Data.GeometryDrawMethod.ToString();
            }, e.Data, ct);
        });

        // A street name's or an organization's own events are not handled here. The names this projection writes
        // onto a segment are what they were when the segment was last written; the read endpoint resolves the
        // current one from the street name / organization document and falls back to what is stored here. Keeping
        // them in step from the other entity's events is what used to need the reverse lookup - and it could not be
        // made reliable with one: a query sees the database, not what the batch it runs in has yet to commit, so a
        // segment written earlier in the same batch was missed.
    }

    private static async Task<ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue>> BuildMaintenanceAuthority(IDocumentOperations session, OrganizationId organizationId, RoadSegmentGeometry geometry, CancellationToken ct)
    {
        return ForEntireGeometry(await ToMaintenanceAuthorityAttributeValue(session, organizationId, ct), geometry);
    }

    private static async Task<ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue>> BuildMaintenanceAuthority(IDocumentOperations session, RoadSegmentDynamicAttributeValues<OrganizationId> attributes, CancellationToken ct)
    {
        var values = new List<(RoadSegmentPositionV2 From, RoadSegmentPositionV2 To, RoadSegmentAttributeSide Side, RoadSegmentMaintenanceAuthorityAttributeValue? Value)>();
        foreach (var x in attributes.Values)
        {
            values.Add((x.Coverage.From, x.Coverage.To, x.Side, await ToMaintenanceAuthorityAttributeValue(session, x.Value, ct)));
        }

        return new ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue>(values);
    }

    private static async Task<RoadSegmentMaintenanceAuthorityAttributeValue> ToMaintenanceAuthorityAttributeValue(IDocumentOperations session, OrganizationId organizationId, CancellationToken ct)
    {
        var organization = await session.LoadAsync<OrganizationReadItem>(organizationId.ToString(), ct);
        return new RoadSegmentMaintenanceAuthorityAttributeValue
        {
            OrganizationId = organizationId,
            Name = organization?.Name
        };
    }

    private static async Task AddOrUpdateRoadSegment(IDocumentOperations session, RoadSegmentReadItem roadSegment, CancellationToken ct)
    {
        // Apply idempotently. If the segment already exists - because the add event is reprocessed, or another
        // projection already loaded it into this shared session during the batch - mutate the existing tracked
        // instance instead of Storing a new one. Storing a different instance for an already-tracked id makes Marten
        // throw "Document ... with same Id already added to the session".
        var existing = await session.LoadAsync<RoadSegmentReadItem>(roadSegment.RoadSegmentId, ct);
        if (existing is not null)
        {
            existing.CopyDataFrom(roadSegment);
            session.Store(existing);
        }
        else
        {
            session.Store(roadSegment);
        }
    }

    // Knotted into the network: the event records the realized state in full, so nothing is left at what it was.
    private Task ProjectConnected(IDocumentOperations session, IRoadSegmentWasConnectedEvent e, CancellationToken ct)
    {
        return ModifyRoadSegment(session, e.RoadSegmentId, async segment =>
        {
            segment.Geometry = ProjectGeometry(e.Geometry);
            segment.StartNodeId = e.StartNodeId.ToInt32();
            segment.EndNodeId = e.EndNodeId.ToInt32();
            segment.Status = RoadSegmentStatusChange.ForEvent(e).To.ToString();
            segment.AccessRestriction = e.AccessRestriction.ToStringAttributeValues(x => x!.ToString());
            segment.Category = e.Category.ToStringAttributeValues(x => x!.ToString());
            segment.Morphology = e.Morphology.ToStringAttributeValues(x => x!.ToString());
            segment.StreetNameId = await BuildStreetNameAttribute(session, e.StreetNameId, ct);
            segment.MaintenanceAuthorityId = await BuildMaintenanceAuthority(session, e.MaintenanceAuthorityId, ct);
            segment.SurfaceType = e.SurfaceType.ToStringAttributeValues(x => x!.ToString());
            segment.CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.CarTrafficDirection);
            segment.BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.BikeTrafficDirection);
            segment.PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.PedestrianTrafficDirection);
        }, e, ct);
    }

    // Unhooked from the network: the geometry and every attribute stay as they were, only the status changes and the
    // road nodes are given up.
    private Task ProjectDisconnected(IDocumentOperations session, IRoadSegmentWasDisconnectedEvent e, CancellationToken ct)
    {
        return ModifyRoadSegment(session, e.RoadSegmentId, segment =>
        {
            segment.Status = RoadSegmentStatusChange.ForEvent(e).To.ToString();
            segment.StartNodeId = null;
            segment.EndNodeId = null;
            return Task.CompletedTask;
        }, e, ct);
    }

    // Outside the network before and after, so nothing but the status moves.
    private Task ProjectUnconnectedStatusChange(IDocumentOperations session, IRoadSegmentUnconnectedStatusChangeEvent e, CancellationToken ct)
    {
        return ModifyRoadSegment(session, e.RoadSegmentId, segment =>
        {
            segment.Status = RoadSegmentStatusChange.ForEvent(e).To.ToString();
            return Task.CompletedTask;
        }, e, ct);
    }

    private async Task ModifyRoadSegment<TEvent>(IDocumentOperations operations, RoadSegmentId roadSegmentId, Func<RoadSegmentReadItem, Task> modify, TEvent evt, CancellationToken ct)
        where TEvent : IMartenEvent
    {
        var roadSegment = await operations.LoadAsync<RoadSegmentReadItem>(roadSegmentId, ct);
        if (roadSegment is null)
        {
            throw new InvalidOperationException($"No road segment found for Id {roadSegmentId}");
        }

        await modify(roadSegment);

        roadSegment.LastModified = evt.Provenance.ToEventTimestamp();
        operations.Store(roadSegment);
    }

    private Task ModifyRoadSegment<TEvent>(IDocumentOperations operations, RoadSegmentId roadSegmentId, Action<RoadSegmentReadItem> modify, TEvent evt, CancellationToken ct)
        where TEvent : IMartenEvent
    {
        return ModifyRoadSegment(operations, roadSegmentId, segment =>
        {
            modify(segment);
            return Task.CompletedTask;
        }, evt, ct);
    }

    private async Task<ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>> BuildStreetNameAttribute(IDocumentOperations session, RoadSegmentDynamicAttributeValues<StreetNameLocalId> attributes, CancellationToken ct)
    {
        var values = new List<(RoadSegmentPositionV2 From, RoadSegmentPositionV2 To, RoadSegmentAttributeSide Side, RoadSegmentStreetNameAttributeValue? Value)>();
        foreach (var x in attributes.Values)
        {
            values.Add((x.Coverage.From, x.Coverage.To, x.Side, await ToStreetNameAttributeValue(session, x.Value, ct)));
        }

        return new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>(values);
    }

    private async Task<RoadSegmentStreetNameAttributeValue> ToStreetNameAttributeValue(IDocumentOperations session, int? streetNameId, CancellationToken ct)
    {
        return new RoadSegmentStreetNameAttributeValue
        {
            StreetNameId = streetNameId is not null ? new StreetNameLocalId(streetNameId.Value) : StreetNameLocalId.NotApplicable,
            DutchName = await GetStreetName(session, streetNameId, ct)
        };
    }
    private async Task<ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>> BuildStreetNameIdAttributesFromV1(IDocumentOperations session, int? leftSideStreetNameId, int? rightSideStreetNameId, RoadSegmentGeometry geometry, CancellationToken ct)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return ForEntireGeometry(await ToStreetNameAttributeValue(session, StreetNameLocalId.NotApplicable, ct), geometry);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return ForEntireGeometry(await ToStreetNameAttributeValue(session, leftSideStreetNameId!.Value, ct), geometry);
        }

        return new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>([
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Links, await ToStreetNameAttributeValue(session, leftSideStreetNameId, ct)),
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Rechts, await ToStreetNameAttributeValue(session, rightSideStreetNameId, ct))
        ]);
    }

    private static ReadRoadSegmentDynamicAttribute<T> ForEntireGeometry<T>(T value, RoadSegmentGeometry geometry)
        where T : notnull
    {
        return new ReadRoadSegmentDynamicAttribute<T>([(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Beide, value)]);
    }

    private static RoadSegmentPositionV2 UseGeometryLengthIfPositionIsLast(double position, RoadSegmentGeometry geometry, bool isLast)
    {
        return isLast
            ? new RoadSegmentPositionV2(geometry.Value.Length)
            : new RoadSegmentPositionV2(position);
    }

    private static StreetNameLocalId GetValue(ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue> attributes, RoadSegmentAttributeSide side)
    {
        if (side == RoadSegmentAttributeSide.Links)
        {
            return attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Beide || x.Side == RoadSegmentAttributeSide.Links).Value!.StreetNameId;
        }
        if (side == RoadSegmentAttributeSide.Rechts)
        {
            return attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Beide || x.Side == RoadSegmentAttributeSide.Rechts).Value!.StreetNameId;
        }
        throw new InvalidOperationException("Only left or right side is allowed.");
    }

    private static RoadSegmentGeometryProjections ProjectGeometry(RoadSegmentGeometry geometry)
    {
        return new RoadSegmentGeometryProjections
        {
            Lambert72 = geometry.EnsureLambert72().RoundToCm(),
            Lambert08 = geometry.EnsureLambert08().RoundToCm(),
        };
    }

    private async Task<string?> GetStreetName(
        IDocumentOperations session,
        int? streetNameId,
        CancellationToken cancellationToken)
    {
        if (streetNameId is null || streetNameId == StreetNameLocalId.NotApplicable || streetNameId == StreetNameLocalId.Unknown)
        {
            return null;
        }

        var streetName = await session.LoadAsync<StreetNameReadItem>(streetNameId.Value, cancellationToken);
        if (streetName is not null)
        {
            return streetName.DutchName;
        }

        if (IsCatchingUp)
        {
            return null;
        }

        try
        {
            var item = await _streetNameClient.GetAsync(streetNameId.Value, cancellationToken);
            return item?.Name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StreetNameApiClient failed for street name id {StreetNameId}; label will be null.", streetNameId.Value);
            return null;
        }
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
    // Plain ints rather than RoadNodeId: Marten writes a duplicated column straight from the .NET member and Npgsql
    // has no mapping for the value object. The stored document is unchanged either way - RoadNodeId serializes as a
    // bare number.
    public required int? StartNodeId { get; set; }
    public required int? EndNodeId { get; set; }
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

    // The street names this segment points at, flattened out of the dynamic attribute above into what Marten can
    // duplicate: a GIN-indexed array column. That is how a street name finds the segments referencing it, now that
    // no link document records it (the street name sync relinks them on a rename or a municipality merger).
    //
    // Derived, so it cannot drift from the attribute it comes from, but deliberately NOT [JsonIgnore]d: Marten fills
    // a duplicated column from the stored document ("update ... set street_name_ids = <from data>"), which is what
    // it runs when it adds the column or reconciles the schema. A column mirroring a member the document does not
    // carry would be emptied by that. Nothing reads it back - it has no setter, and the attribute above remains the
    // source of truth.
    public int[] StreetNameIds => GetStreetNameHashSet().Select(x => x.ToInt32()).Order().ToArray();

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

    // Overwrites the event-owned fields from another read item, deliberately leaving the identity (Id) untouched.
    // Used to apply an "add" event idempotently onto an already-existing document.
    public void CopyDataFrom(RoadSegmentReadItem source)
    {
        Geometry = source.Geometry;
        StartNodeId = source.StartNodeId;
        EndNodeId = source.EndNodeId;
        Status = source.Status;
        GeometryDrawMethod = source.GeometryDrawMethod;
        AccessRestriction = source.AccessRestriction;
        Category = source.Category;
        Morphology = source.Morphology;
        StreetNameId = source.StreetNameId;
        MaintenanceAuthorityId = source.MaintenanceAuthorityId;
        SurfaceType = source.SurfaceType;
        CarTrafficDirection = source.CarTrafficDirection;
        BikeTrafficDirection = source.BikeTrafficDirection;
        PedestrianTrafficDirection = source.PedestrianTrafficDirection;
        EuropeanRoadNumbers = source.EuropeanRoadNumbers;
        NationalRoadNumbers = source.NationalRoadNumbers;
        Origin = source.Origin;
        LastModified = source.LastModified;
        IsV2 = source.IsV2;
        IsRemoved = source.IsRemoved;
    }
}

public sealed class RoadSegmentStreetNameAttributeValue
{
    public required StreetNameLocalId StreetNameId { get; set; }
    public required string? DutchName { get; set; }
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
