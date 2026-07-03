namespace RoadRegistry.Extracts.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using Extensions;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.RoadSegment;
using RoadSegment.Events.V1;
using RoadSegment.Events.V2;
using RoadSegment.ValueObjects;

public class RoadSegmentExtractProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentExtractItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("extract_roadsegments")
            .Identity(x => x.Id);
    }

    public RoadSegmentExtractProjection()
    {
        // V1
        When<IEvent<ImportedRoadSegment>>((session, e, ct) =>
        {
            var roadSegmentId = new RoadSegmentId(e.Data.RoadSegmentId);
            var geometry = e.Data.Geometry.EnsureLambert08().RoundToCm();
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
                Status = status,
                AccessRestriction = ForEntireGeometry(accessRestriction, geometry),
                Category = ForEntireGeometry(category, geometry),
                Morphology = ForEntireGeometry(morphology, geometry),
                StreetNameId = BuildStreetNameIdAttributesFromV1(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry),
                MaintenanceAuthorityId = ForEntireGeometry(new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry),
                SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition)),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), geometry, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                ),
                CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(),
                CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(),
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
        When<IEvent<RoadSegmentAdded>>((session, e, _) =>
        {
            var roadSegmentId = new RoadSegmentId(e.Data.RoadSegmentId);
            var geometry = e.Data.Geometry.EnsureLambert08().RoundToCm();
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
                Status = status,
                AccessRestriction = ForEntireGeometry(accessRestriction, geometry),
                Category = ForEntireGeometry(category, geometry),
                Morphology = ForEntireGeometry(morphology, geometry),
                StreetNameId = BuildStreetNameIdAttributesFromV1(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, geometry),
                MaintenanceAuthorityId = ForEntireGeometry(new OrganizationId(e.Data.MaintenanceAuthority.Code), geometry),
                SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition)),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), geometry, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                ),
                CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(),
                CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
                PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(),
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

                segment.Geometry = e.Data.Geometry.EnsureLambert08().RoundToCm();
                segment.StartNodeId = new RoadNodeId(e.Data.StartNodeId);
                segment.EndNodeId = new RoadNodeId(e.Data.EndNodeId);
                segment.GeometryDrawMethod = geometryDrawMethod;
                segment.Status = status;
                segment.AccessRestriction = ForEntireGeometry(accessRestriction, segment.Geometry);
                segment.Category = ForEntireGeometry(category, segment.Geometry);
                segment.Morphology = ForEntireGeometry(morphology, segment.Geometry);
                segment.StreetNameId = BuildStreetNameIdAttributesFromV1(e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, segment.Geometry);
                segment.MaintenanceAuthorityId = ForEntireGeometry(new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry);
                segment.SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition)),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), segment.Geometry, isLast: i == e.Data.Surfaces.Length - 1),
                        RoadSegmentAttributeSide.Both,
                        x.Type))
                );
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentRemoved>>(async (session, e, _) =>
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
                    segment.AccessRestriction = ForEntireGeometry(e.Data.AccessRestriction, segment.Geometry);
                }

                if (e.Data.Category is not null)
                {
                    segment.Category = ForEntireGeometry(e.Data.Category, segment.Geometry);
                }

                if (e.Data.Morphology is not null)
                {
                    segment.Morphology = ForEntireGeometry(e.Data.Morphology, segment.Geometry);
                }

                if (e.Data.Status is not null)
                {
                    segment.Status = e.Data.Status;
                }

                if (e.Data.LeftSide is not null || e.Data.RightSide is not null)
                {
                    segment.StreetNameId = BuildStreetNameIdAttributesFromV1(
                        e.Data.LeftSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Left),
                        e.Data.RightSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Right),
                        segment.Geometry);
                }

                if (e.Data.MaintenanceAuthority is not null)
                {
                    segment.MaintenanceAuthorityId = ForEntireGeometry(new OrganizationId(e.Data.MaintenanceAuthority.Code), segment.Geometry);
                }

                if (e.Data.Surfaces is not null)
                {
                    segment.SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                        .OrderBy(x => x.FromPosition)
                        .Select((x, i) => (
                            new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition)),
                            UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), segment.Geometry, isLast: i == e.Data.Surfaces.Length - 1),
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
                segment.Geometry = e.Data.Geometry.EnsureLambert08().RoundToCm();
                segment.SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(e.Data.Surfaces
                    .OrderBy(x => x.FromPosition)
                    .Select((x, i) => (
                        new RoadSegmentPositionV2(Convert.ToDouble(x.FromPosition)),
                        UseGeometryLengthIfPositionIsLast(Convert.ToDouble(x.ToPosition), segment.Geometry, isLast: i == e.Data.Surfaces.Length - 1),
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
                    segment.StreetNameId = BuildStreetNameIdAttributesFromV1(
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
        When<IEvent<RoadSegmentWasAdded>>((session, e, _) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var roadSegment = new RoadSegmentExtractItem
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
                CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.CarTrafficDirection)),
                CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.CarTrafficDirection)),
                BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.BikeTrafficDirection)),
                BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.BikeTrafficDirection)),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(e.Data.PedestrianTrafficDirection)),
                CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection),
                BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection),
                PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection),
                EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList(),
                NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList(),
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<OutlinedRoadSegmentWasAdded>>((session, e, _) =>
        {
            var roadSegmentId = e.Data.RoadSegmentId;

            var roadSegment = new RoadSegmentExtractItem
            {
                RoadSegmentId = roadSegmentId,
                Geometry = e.Data.Geometry,
                StartNodeId = null,
                EndNodeId = null,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
                Status = e.Data.Status,
                AccessRestriction = e.Data.AccessRestriction.ToStringAttributeValues(x => x.ToString()),
                Category = e.Data.Category.ToStringAttributeValues(x => x.ToString()),
                Morphology = e.Data.Morphology.ToStringAttributeValues(x => x.ToString()),
                StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId),
                MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.MaintenanceAuthorityId),
                SurfaceType = e.Data.SurfaceType.ToStringAttributeValues(x => x.ToString()),
                CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.CarTrafficDirection)),
                CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.CarTrafficDirection)),
                BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.BikeTrafficDirection)),
                BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.BikeTrafficDirection)),
                PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(e.Data.PedestrianTrafficDirection)),
                CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection),
                BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection),
                PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection),
                EuropeanRoadNumbers = [],
                NationalRoadNumbers = [],
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp(),
                IsV2 = true
            };
            session.Store(roadSegment);

            return Task.CompletedTask;
        });
        When<IEvent<RoadSegmentWasMerged>>((session, e, ct) =>
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
                segment.CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.CarTrafficDirection));
                segment.CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.CarTrafficDirection));
                segment.BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.BikeTrafficDirection));
                segment.BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.BikeTrafficDirection));
                segment.PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(e.Data.PedestrianTrafficDirection));
                segment.CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection);
                segment.BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection);
                segment.PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection);
                segment.EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList();
                segment.NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList();
                segment.IsV2 = true;
            }, e.Data, ct);
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

                if (e.Data.CarTrafficDirection is not null)
                {
                    segment.CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.CarTrafficDirection));
                    segment.CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.CarTrafficDirection));
                    segment.CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection);
                }

                if (e.Data.BikeTrafficDirection is not null)
                {
                    segment.BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.BikeTrafficDirection));
                    segment.BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.BikeTrafficDirection));
                    segment.BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection);
                }

                if (e.Data.PedestrianTrafficDirection is not null)
                {
                    segment.PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(e.Data.PedestrianTrafficDirection));
                    segment.PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection);
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
                segment.CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.CarTrafficDirection));
                segment.CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.CarTrafficDirection));
                segment.BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.BikeTrafficDirection));
                segment.BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.BikeTrafficDirection));
                segment.PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(e.Data.PedestrianTrafficDirection));
                segment.CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.CarTrafficDirection);
                segment.BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.BikeTrafficDirection);
                segment.PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.PedestrianTrafficDirection);
                segment.EuropeanRoadNumbers = e.Data.EuropeanRoadNumbers.ToList();
                segment.NationalRoadNumbers = e.Data.NationalRoadNumbers.ToList();
                segment.IsV2 = true;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasRemoved>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No road segment found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRemovedBecauseOfMigration>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No road segment found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRetired>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No road segment found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasRetiredBecauseOfMerger>>(async (session, e, _) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No road segment found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
        });
        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentStreetNameIdWasChanged>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameId); }, e.Data, ct); });
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
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Left, StreetNameLocalId.FromValue(leftSideStreetNameId) ?? StreetNameLocalId.NotApplicable),
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Right, StreetNameLocalId.FromValue(rightSideStreetNameId) ?? StreetNameLocalId.NotApplicable)
        ]);
    }

    private static ExtractRoadSegmentDynamicAttribute<T> ForEntireGeometry<T>(T value, RoadSegmentGeometry geometry)
    {
        return new ExtractRoadSegmentDynamicAttribute<T>([(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Both, value)]);
    }

    private static RoadSegmentPositionV2 UseGeometryLengthIfPositionIsLast(double position, RoadSegmentGeometry geometry, bool isLast)
    {
        return isLast
            ? new RoadSegmentPositionV2(geometry.Value.Length)
            : new RoadSegmentPositionV2(position);
    }

    private static StreetNameLocalId GetValue(ExtractRoadSegmentDynamicAttribute<StreetNameLocalId> attributes, RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => attributes.Values.Single(x => x.Side is RoadSegmentAttributeSide.Both or RoadSegmentAttributeSide.Left).Value,
            RoadSegmentAttributeSide.Right => attributes.Values.Single(x => x.Side is RoadSegmentAttributeSide.Both or RoadSegmentAttributeSide.Right).Value,
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
    public required ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection> CarTrafficDirection { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection> BikeTrafficDirection { get; set; }
    public required ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection> PedestrianTrafficDirection { get; set; }
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

    public ExtractRoadSegmentDynamicAttribute(RoadRegistry.RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<T> attributes)
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
