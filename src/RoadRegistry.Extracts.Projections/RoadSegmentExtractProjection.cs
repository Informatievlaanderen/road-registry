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

public class RoadSegmentExtractProjection : MartenRoadNetworkChangesProjection
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
                StartNodeId = e.Data.StartNodeId > 0 ? new RoadNodeId(e.Data.StartNodeId) : null,
                EndNodeId = e.Data.EndNodeId > 0 ? new RoadNodeId(e.Data.EndNodeId) : null,
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
                        RoadSegmentAttributeSide.Beide,
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
                StartNodeId = e.Data.StartNodeId > 0 ? new RoadNodeId(e.Data.StartNodeId) : null,
                EndNodeId = e.Data.EndNodeId > 0 ? new RoadNodeId(e.Data.EndNodeId) : null,
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
                        RoadSegmentAttributeSide.Beide,
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
                segment.StartNodeId = e.Data.StartNodeId > 0 ? new RoadNodeId(e.Data.StartNodeId) : null;
                segment.EndNodeId = e.Data.EndNodeId > 0 ? new RoadNodeId(e.Data.EndNodeId) : null;
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
                        RoadSegmentAttributeSide.Beide,
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
                        e.Data.LeftSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Links),
                        e.Data.RightSide?.StreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Rechts),
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
                            RoadSegmentAttributeSide.Beide,
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
                        RoadSegmentAttributeSide.Beide,
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
                        e.Data.LeftSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Links),
                        e.Data.RightSideStreetNameId ?? GetValue(segment.StreetNameId, RoadSegmentAttributeSide.Rechts),
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
        When<IEvent<RoadSegmentWasHistorizedFromPlanned>>((session, e, ct) => ProjectUnconnectedStatusChange(session, e.Data, ct));
        When<IEvent<RoadSegmentWasHistorizedFromNotRealized>>((session, e, ct) => ProjectUnconnectedStatusChange(session, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromHistorizedToPlanned>>((session, e, ct) => ProjectUnconnectedStatusChange(session, e.Data, ct));
        When<IEvent<RoadSegmentWasMeasured>>((session, e, ct) =>
            ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten; }, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedToOutlined>>((session, e, ct) =>
            ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst; }, e.Data, ct));
        When<IEvent<RoadSegmentWasChanged>>((session, e, ct) =>
        {
            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Geometry ?? segment.Geometry;
                if (e.Data.NodeIds is not null)
                {
                    segment.StartNodeId = e.Data.NodeIds.Start;
                    segment.EndNodeId = e.Data.NodeIds.End;
                }

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
        When<IEvent<RoadSegmentWasMigrated>>(async (session, e, ct) =>
        {
            // RoadSegmentWasMigrated carries the road segment whole - geometry, nodes and every attribute - so the row
            // is written from the event rather than patched into the row that happens to be there. That is what makes
            // this the event that brings a road segment back after RoadSegmentRemovalWasCorrectedBecauseOfMigration:
            // the correction carries nothing to build a row from, and the row itself was deleted when the road segment
            // was removed, so there is nothing left to patch. Patching is what stopped the whole extract projection on
            // road segment 427083 - "RoadSegment with id 427083 is not found" - the same hole RoadNodeWasMigrated
            // already closes for road nodes.
            var migrated = BuildMigratedRoadSegment(e.Data);
            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId, ct);

            if (roadSegment is null)
            {
                session.Store(migrated);
                return;
            }

            // A road segment that is still here is written over the row object the session already holds, rather than
            // handed a second one: Marten's identity map refuses to take two objects for the same id - "Document
            // 'RoadSegmentExtractItem' with same Id already added to the session" - which is how the projection came
            // to a stop again on the first migrated road segment that still had its row.
            OverwriteWithMigrated(roadSegment, migrated);
            session.Store(roadSegment);
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
        When<IEvent<RoadSegmentRemovalWasCorrectedBecauseOfMigration>>((_, _, _) =>
        {
            // Nothing here: the row was deleted when the road segment was removed, and the correction says no more
            // than that the removal is undone. The road segment is collected again right after it, and the
            // RoadSegmentWasMigrated above builds the row back with the real geometry and attributes rather than a
            // placeholder invented here.
            return Task.CompletedTask;
        });
        // A road segment retired by a merger or a split is historized, not removed: the aggregate keeps it - status
        // gehistoreerd, no road nodes - so this projection keeps it as well, the same as a road segment historized
        // from realized. Deleting on a status change was what made a later event on the same road segment explode:
        // the status it leaves behind can be corrected back, and there would be nothing left to correct. Whether a
        // gehistoreerd road segment belongs in an extract is not this projection's call: the aggregate records the
        // status, and RoadNetworkRepository.LoadForExtract asks it for the road segments an extract is made of.
        When<IEvent<RoadSegmentWasRetiredBecauseOfMerger>>((session, e, ct) => Historize(session, e.Data.RoadSegmentId, e.Data, ct));
        When<IEvent<RoadSegmentWasRetiredBecauseOfSplit>>((session, e, ct) => Historize(session, e.Data.RoadSegmentId, e.Data, ct));
        When<IEvent<RoadSegmentWasSplit>>((session, e, ct) =>
        {
            // Only the segment that keeps its identifier (largest part) carries modifications.
            if (e.Data.Modifications is null)
            {
                return Task.CompletedTask;
            }

            return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment =>
            {
                segment.Geometry = e.Data.Modifications.Geometry;
                segment.StartNodeId = e.Data.Modifications.StartNodeId;
                segment.EndNodeId = e.Data.Modifications.EndNodeId;
                segment.AccessRestriction = e.Data.Modifications.AccessRestriction.ToStringAttributeValues(x => x.ToString());
                segment.Category = e.Data.Modifications.Category.ToStringAttributeValues(x => x.ToString());
                segment.Morphology = e.Data.Modifications.Morphology.ToStringAttributeValues(x => x.ToString());
                segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.Modifications.StreetNameId);
                segment.MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.Data.Modifications.MaintenanceAuthorityId);
                segment.SurfaceType = e.Data.Modifications.SurfaceType.ToStringAttributeValues(x => x.ToString());
                segment.CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.Modifications.CarTrafficDirection));
                segment.CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.Modifications.CarTrafficDirection));
                segment.BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.Data.Modifications.BikeTrafficDirection));
                segment.BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.Data.Modifications.BikeTrafficDirection));
                segment.PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(e.Data.Modifications.PedestrianTrafficDirection));
                segment.CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.Modifications.CarTrafficDirection);
                segment.BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.Data.Modifications.BikeTrafficDirection);
                segment.PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.Data.Modifications.PedestrianTrafficDirection);
                segment.IsV2 = true;
            }, e.Data, ct);
        });
        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Add(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.EuropeanRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>((session, e, ct) => { return ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.NationalRoadNumbers.Remove(e.Data.Number); }, e.Data, ct); });
        When<IEvent<RoadSegmentStreetNameWasUnlinked>>(async (session, e, ct) =>
        {
            // A road segment that was removed before its street names were let go (the run-once correction of
            // GAWR-7784) is no longer in this projection, and there is nothing left to unlink.
            if (await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId, ct) is null)
            {
                return;
            }

            await ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameIdAttributeValues); }, e.Data, ct);
        });
        When<IEvent<RoadSegmentStreetNameWasRelinked>>(async (session, e, ct) =>
        {
            // The relink leaves removed road segments out, but a road segment this projection no longer holds is not
            // brought back for it either.
            if (await session.LoadAsync<RoadSegmentExtractItem>(e.Data.RoadSegmentId, ct) is null)
            {
                return;
            }

            await ModifyRoadSegment(session, e.Data.RoadSegmentId, segment => { segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.Data.StreetNameIdAttributeValues); }, e.Data, ct);
        });
    }

    // Knotted into the network: the event records the realized state in full, so nothing is left at what it was.
    private Task ProjectConnected(IDocumentOperations session, IRoadSegmentWasConnectedEvent e, CancellationToken ct)
    {
        return ModifyRoadSegment(session, e.RoadSegmentId, segment =>
        {
            segment.Geometry = e.Geometry;
            segment.StartNodeId = e.StartNodeId;
            segment.EndNodeId = e.EndNodeId;
            segment.Status = RoadSegmentStatusChange.ForEvent(e).To;
            segment.AccessRestriction = e.AccessRestriction.ToStringAttributeValues(x => x.ToString());
            segment.Category = e.Category.ToStringAttributeValues(x => x.ToString());
            segment.Morphology = e.Morphology.ToStringAttributeValues(x => x.ToString());
            segment.StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.StreetNameId);
            segment.MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.MaintenanceAuthorityId);
            segment.SurfaceType = e.SurfaceType.ToStringAttributeValues(x => x.ToString());
            segment.CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.CarTrafficDirection);
            segment.BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.BikeTrafficDirection);
            segment.PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.PedestrianTrafficDirection);
        }, e, ct);
    }

    // Unhooked from the network: the geometry and every attribute stay as they were, only the status changes and the
    // road nodes are given up.
    private Task ProjectDisconnected(IDocumentOperations session, IRoadSegmentWasDisconnectedEvent e, CancellationToken ct)
    {
        return ModifyRoadSegment(session, e.RoadSegmentId, segment =>
        {
            segment.Status = RoadSegmentStatusChange.ForEvent(e).To;
            segment.StartNodeId = null;
            segment.EndNodeId = null;
        }, e, ct);
    }

    // Retired by a merger or a split: the road segment is historized, which is the same move as ProjectDisconnected
    // makes, except the event carries no status of its own to read it off.
    private Task Historize<TEvent>(IDocumentOperations session, RoadSegmentId roadSegmentId, TEvent e, CancellationToken ct)
        where TEvent : IMartenEvent
    {
        return ModifyRoadSegment(session, roadSegmentId, segment =>
        {
            segment.Status = RoadSegmentStatusV2.Gehistoreerd.ToString();
            segment.StartNodeId = null;
            segment.EndNodeId = null;
        }, e, ct);
    }

    // Outside the network before and after, so nothing but the status moves.
    private Task ProjectUnconnectedStatusChange(IDocumentOperations session, IRoadSegmentUnconnectedStatusChangeEvent e, CancellationToken ct)
    {
        return ModifyRoadSegment(session, e.RoadSegmentId, segment =>
        {
            segment.Status = RoadSegmentStatusChange.ForEvent(e).To;
        }, e, ct);
    }

    // The migrated road segment as the event has it, in full. Origin is when this projection first saw the road
    // segment, which for a road segment being collected again starts over from this event.
    private static RoadSegmentExtractItem BuildMigratedRoadSegment(RoadSegmentWasMigrated e)
    {
        return new RoadSegmentExtractItem
        {
            RoadSegmentId = e.RoadSegmentId,
            Geometry = e.Geometry,
            StartNodeId = e.StartNodeId,
            EndNodeId = e.EndNodeId,
            GeometryDrawMethod = e.GeometryDrawMethod,
            Status = e.Status,
            AccessRestriction = e.AccessRestriction.ToStringAttributeValues(x => x.ToString()),
            Category = e.Category.ToStringAttributeValues(x => x.ToString()),
            Morphology = e.Morphology.ToStringAttributeValues(x => x.ToString()),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(e.StreetNameId),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(e.MaintenanceAuthorityId),
            SurfaceType = e.SurfaceType.ToStringAttributeValues(x => x.ToString()),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.CarTrafficDirection)),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.CarTrafficDirection)),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(e.BikeTrafficDirection)),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(e.BikeTrafficDirection)),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(e.PedestrianTrafficDirection)),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.CarTrafficDirection),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.BikeTrafficDirection),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.PedestrianTrafficDirection),
            EuropeanRoadNumbers = e.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = e.NationalRoadNumbers.ToList(),
            Origin = e.Provenance.ToEventTimestamp(),
            LastModified = e.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
    }

    // Writes the migrated road segment onto the row the session already holds. Everything the event carries is
    // overwritten; Origin is not, because the road segment was already here and keeps the moment this projection
    // first saw it.
    private static void OverwriteWithMigrated(RoadSegmentExtractItem roadSegment, RoadSegmentExtractItem migrated)
    {
        roadSegment.Geometry = migrated.Geometry;
        roadSegment.StartNodeId = migrated.StartNodeId;
        roadSegment.EndNodeId = migrated.EndNodeId;
        roadSegment.GeometryDrawMethod = migrated.GeometryDrawMethod;
        roadSegment.Status = migrated.Status;
        roadSegment.AccessRestriction = migrated.AccessRestriction;
        roadSegment.Category = migrated.Category;
        roadSegment.Morphology = migrated.Morphology;
        roadSegment.StreetNameId = migrated.StreetNameId;
        roadSegment.MaintenanceAuthorityId = migrated.MaintenanceAuthorityId;
        roadSegment.SurfaceType = migrated.SurfaceType;
        roadSegment.CarAccessForward = migrated.CarAccessForward;
        roadSegment.CarAccessBackward = migrated.CarAccessBackward;
        roadSegment.BikeAccessForward = migrated.BikeAccessForward;
        roadSegment.BikeAccessBackward = migrated.BikeAccessBackward;
        roadSegment.PedestrianAccess = migrated.PedestrianAccess;
        roadSegment.CarTrafficDirection = migrated.CarTrafficDirection;
        roadSegment.BikeTrafficDirection = migrated.BikeTrafficDirection;
        roadSegment.PedestrianTrafficDirection = migrated.PedestrianTrafficDirection;
        roadSegment.EuropeanRoadNumbers = migrated.EuropeanRoadNumbers;
        roadSegment.NationalRoadNumbers = migrated.NationalRoadNumbers;
        roadSegment.LastModified = migrated.LastModified;
        roadSegment.IsV2 = migrated.IsV2;
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
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Links, StreetNameLocalId.FromValue(leftSideStreetNameId) ?? StreetNameLocalId.NotApplicable),
            (RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Rechts, StreetNameLocalId.FromValue(rightSideStreetNameId) ?? StreetNameLocalId.NotApplicable)
        ]);
    }

    private static ExtractRoadSegmentDynamicAttribute<T> ForEntireGeometry<T>(T value, RoadSegmentGeometry geometry)
    {
        return new ExtractRoadSegmentDynamicAttribute<T>([(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), RoadSegmentAttributeSide.Beide, value)]);
    }

    private static RoadSegmentPositionV2 UseGeometryLengthIfPositionIsLast(double position, RoadSegmentGeometry geometry, bool isLast)
    {
        return isLast
            ? new RoadSegmentPositionV2(geometry.Value.Length)
            : new RoadSegmentPositionV2(position);
    }

    private static StreetNameLocalId GetValue(ExtractRoadSegmentDynamicAttribute<StreetNameLocalId> attributes, RoadSegmentAttributeSide side)
    {
        if (side == RoadSegmentAttributeSide.Links)
        {
            return attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Beide || x.Side == RoadSegmentAttributeSide.Links).Value;
        }
        if (side == RoadSegmentAttributeSide.Rechts)
        {
            return attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Beide || x.Side == RoadSegmentAttributeSide.Rechts).Value;
        }
        throw new InvalidOperationException("Only left or right side is allowed.");
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
