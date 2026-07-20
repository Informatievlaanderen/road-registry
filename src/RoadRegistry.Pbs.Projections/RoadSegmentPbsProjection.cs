namespace RoadRegistry.Pbs.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class RoadSegmentPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public RoadSegmentPbsProjection()
    {
        // V1
        // Legacy events carry raw string values; each is mapped to its V2 equivalent (see V1ToV2), storing null when a
        // value has no V2 mapping. Traffic direction does not exist in V1, so it is left empty. Numbered roads are not
        // part of the PBS product, so those events are ignored.
        When<IEvent<ImportedRoadSegment>>((context, e, ct) =>
            WriteV1Full(context, e.Data.RoadSegmentId, e.Data.Geometry, e.Data.Status, e.Data.GeometryDrawMethod,
                e.Data.StartNodeId, e.Data.EndNodeId, e.Data.Morphology, e.Data.Category, e.Data.AccessRestriction,
                e.Data.Surfaces.Select(s => (s.FromPosition, s.ToPosition, s.Type)).ToList(),
                e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, e.Data.MaintenanceAuthority.Code,
                e.Data.PartOfEuropeanRoads.Select(x => EuropeanRoadNumber.Parse(x.Number)).ToList(),
                e.Data.PartOfNationalRoads.Select(x => NationalRoadNumber.Parse(x.Number)).ToList(),
                e.Data.Provenance, ct));

        When<IEvent<RoadSegmentAdded>>((context, e, ct) =>
            WriteV1Full(context, e.Data.RoadSegmentId, e.Data.Geometry, e.Data.Status, e.Data.GeometryDrawMethod,
                e.Data.StartNodeId, e.Data.EndNodeId, e.Data.Morphology, e.Data.Category, e.Data.AccessRestriction,
                e.Data.Surfaces.Select(s => (s.FromPosition, s.ToPosition, s.Type)).ToList(),
                e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, e.Data.MaintenanceAuthority.Code,
                [], [], e.Data.Provenance, ct));

        When<IEvent<RoadSegmentModified>>((context, e, ct) =>
            WriteV1Modified(context, e.Data.RoadSegmentId, e.Data.Geometry, e.Data.Status, e.Data.GeometryDrawMethod,
                e.Data.StartNodeId, e.Data.EndNodeId, e.Data.Morphology, e.Data.Category, e.Data.AccessRestriction,
                e.Data.Surfaces.Select(s => (s.FromPosition, s.ToPosition, s.Type)).ToList(),
                e.Data.LeftSide.StreetNameId, e.Data.RightSide.StreetNameId, e.Data.MaintenanceAuthority.Code,
                e.Data.Provenance, ct));

        When<IEvent<RoadSegmentAttributesModified>>(async (context, e, ct) =>
        {
            var m = e.Data;
            var record = await context.RoadSegments.FindAsync([m.RoadSegmentId], ct);
            if (record?.GEOMETRIE is null)
            {
                return;
            }
            var length = record.GEOMETRIE.Length;

            RoadSegmentStatusV2 status = null;
            if (m.Status is not null)
            {
                var method = record.METHODE is int methodIdentifier && RoadSegmentGeometryDrawMethodV2.ByIdentifier.TryGetValue(methodIdentifier, out var storedMethod) ? storedMethod : null;
                status = V1ToV2.Status(m.Status, method);
            }

            var morphology = m.Morphology is not null ? ForEntireGeometry(V1ToV2.Morphology(m.Morphology), length) : null;
            var category = m.Category is not null ? ForEntireGeometry(V1ToV2.Category(m.Category), length) : null;
            var access = m.AccessRestriction is not null ? ForEntireGeometry(V1ToV2.AccessRestriction(m.AccessRestriction), length) : null;
            var surface = m.Surfaces is not null ? SurfaceFromV1(m.Surfaces.Select(s => (s.FromPosition, s.ToPosition, s.Type)).ToList(), length) : null;
            var maintainer = m.MaintenanceAuthority is not null ? MaintainerFromV1(m.MaintenanceAuthority.Code, length) : null;

            RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetName = null;
            if (m.LeftSide is not null || m.RightSide is not null)
            {
                var (currentLeft, currentRight) = await CurrentStreetNameSidesFromV1(context, m.RoadSegmentId, ct);
                streetName = StreetNameFromV1(m.LeftSide?.StreetNameId ?? currentLeft, m.RightSide?.StreetNameId ?? currentRight, length);
            }

            await WritePartial(context, m.RoadSegmentId, null, status, null, null, null,
                morphology, category, access, surface, streetName, maintainer, null, null, null, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentGeometryModified>>((context, e, ct) =>
        {
            var length = e.Data.Geometry.EnsureLambert08().RoundToCm().Value.Length;
            return WritePartial(context, e.Data.RoadSegmentId, e.Data.Geometry, null, null, null, null,
                null, null, null,
                SurfaceFromV1(e.Data.Surfaces.Select(s => (s.FromPosition, s.ToPosition, s.Type)).ToList(), length),
                null, null, null, null, null, e.Data.Provenance, ct);
        });

        When<IEvent<RoadSegmentStreetNamesChanged>>(async (context, e, ct) =>
        {
            if (e.Data.LeftSideStreetNameId is null && e.Data.RightSideStreetNameId is null)
            {
                return;
            }
            var record = await context.RoadSegments.FindAsync([e.Data.RoadSegmentId], ct);
            if (record?.GEOMETRIE is null)
            {
                return;
            }
            var (currentLeft, currentRight) = await CurrentStreetNameSidesFromV1(context, e.Data.RoadSegmentId, ct);
            var streetName = StreetNameFromV1(e.Data.LeftSideStreetNameId ?? currentLeft, e.Data.RightSideStreetNameId ?? currentRight, record.GEOMETRIE.Length);
            await WritePartial(context, e.Data.RoadSegmentId, null, null, null, null, null, null, null, null, null,
                streetName, null, null, null, null, e.Data.Provenance, ct);
        });

        When<IEvent<RoadSegmentRemoved>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId, ct));
        When<IEvent<OutlinedRoadSegmentRemoved>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId, ct));

        When<IEvent<RoadSegmentAddedToEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = EuropeanRoadNumber.Parse(e.Data.Number).ToString();
            if (!await context.EuropeanRoads.AnyAsync(x => x.WS_OIDN == segId && x.EUNUMMER == nummer, ct))
            {
                context.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentRemovedFromEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = EuropeanRoadNumber.Parse(e.Data.Number).ToString();
            var rows = await context.EuropeanRoads.Where(x => x.WS_OIDN == segId && x.EUNUMMER == nummer).ToListAsync(ct);
            context.EuropeanRoads.RemoveRange(rows);
        });
        When<IEvent<RoadSegmentAddedToNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = NationalRoadNumber.Parse(e.Data.Number).ToString();
            if (!await context.NationalRoads.AnyAsync(x => x.WS_OIDN == segId && x.NWNUMMER == nummer, ct))
            {
                context.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentRemovedFromNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = NationalRoadNumber.Parse(e.Data.Number).ToString();
            var rows = await context.NationalRoads.Where(x => x.WS_OIDN == segId && x.NWNUMMER == nummer).ToListAsync(ct);
            context.NationalRoads.RemoveRange(rows);
        });
        When<IEvent<RoadSegmentAddedToNumberedRoad>>((_, _, _) => Task.CompletedTask);
        When<IEvent<RoadSegmentRemovedFromNumberedRoad>>((_, _, _) => Task.CompletedTask);

        // V2
        When<IEvent<RoadSegmentWasAdded>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), true, m.Geometry, m.Status, m.GeometryDrawMethod,
                m.StartNodeId, m.EndNodeId, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.EuropeanRoadNumbers, m.NationalRoadNumbers, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasMigrated>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), false, m.Geometry, m.Status, m.GeometryDrawMethod,
                m.StartNodeId, m.EndNodeId, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.EuropeanRoadNumbers, m.NationalRoadNumbers, m.Provenance, ct);
        });

        When<IEvent<OutlinedRoadSegmentWasAdded>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), true, m.Geometry, m.Status, RoadSegmentGeometryDrawMethodV2.Ingeschetst,
                null, null, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, [], [], m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasMerged>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), false, m.Geometry, m.Status, m.GeometryDrawMethod,
                m.StartNodeId, m.EndNodeId, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.EuropeanRoadNumbers, m.NationalRoadNumbers, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasModified>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), m.Geometry, m.Status, m.GeometryDrawMethod,
                m.StartNodeId, m.EndNodeId, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentGeometryWasModified>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), m.Geometry, null, null,
                m.StartNodeId, m.EndNodeId, null, null, null, null, null, null, null, null, null, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentStreetNameIdWasChanged>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), null, null, null, null, null, null, null, null, null,
                m.StreetNameId, null, null, null, null, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasSplit>>(async (context, e, ct) =>
        {
            var m = e.Data;
            if (m.Modifications is null)
            {
                return;
            }

            var mod = m.Modifications;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), mod.Geometry, null, null,
                mod.StartNodeId, mod.EndNodeId, mod.Morphology, mod.Category, mod.AccessRestriction, mod.SurfaceType,
                mod.StreetNameId, mod.MaintenanceAuthorityId, mod.CarTrafficDirection, mod.BikeTrafficDirection,
                mod.PedestrianTrafficDirection, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasRemoved>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentWasRemovedBecauseOfMigration>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentWasRetired>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentWasRetiredBecauseOfMerger>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentWasRetiredBecauseOfSplit>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));

        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            if (!await context.EuropeanRoads.AnyAsync(x => x.WS_OIDN == segId && x.EUNUMMER == nummer, ct))
            {
                context.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            var rows = await context.EuropeanRoads.Where(x => x.WS_OIDN == segId && x.EUNUMMER == nummer).ToListAsync(ct);
            context.EuropeanRoads.RemoveRange(rows);
        });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            if (!await context.NationalRoads.AnyAsync(x => x.WS_OIDN == segId && x.NWNUMMER == nummer, ct))
            {
                context.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            var rows = await context.NationalRoads.Where(x => x.WS_OIDN == segId && x.NWNUMMER == nummer).ToListAsync(ct);
            context.NationalRoads.RemoveRange(rows);
        });
    }

    private static async Task WriteFull(PbsContext context, int segId, bool assumeNew,
        RoadSegmentGeometry geometry, RoadSegmentStatusV2 status, RoadSegmentGeometryDrawMethodV2 method,
        RoadNodeId? startNode, RoadNodeId? endNode,
        RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> morphology,
        RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> category,
        RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> access,
        RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> surface,
        RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetName,
        RoadSegmentDynamicAttributeValues<OrganizationId> maintainer,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> car,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> bike,
        RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> pedestrian,
        IReadOnlyCollection<EuropeanRoadNumber> europeanRoadNumbers,
        IReadOnlyCollection<NationalRoadNumber> nationalRoadNumbers,
        ProvenanceData provenance, CancellationToken ct)
    {
        // For a created event the segment cannot exist yet, so skip the lookup and always insert (re-delivery is guarded
        // by the projection-state position in the base projection). The same flag lets the attribute/road rebuilds skip
        // their delete queries since there is nothing to remove.
        var (normal, isNew) = assumeNew
            ? (new RoadSegmentRecord { WS_OIDN = segId, CREATIE = provenance.ToPbsDate() }, true)
            : await LoadOrCreate(context, segId, provenance);

        normal.GEOMETRIE = geometry.EnsureLambert08().RoundToCm().Value;
        normal.STATUS = status?.Translation.Identifier;
        normal.LBLSTATUS = status?.Translation.Name;
        normal.METHODE = method?.Translation.Identifier;
        normal.LBLMETHODE = method?.Translation.Name;
        normal.B_WK_OIDN = startNode?.ToInt32();
        normal.E_WK_OIDN = endNode?.ToInt32();
        normal.VERSIE = provenance.ToPbsDate();
        if (isNew)
        {
            context.RoadSegments.Add(normal);
        }

        var morphologyRows = await RebuildMorphology(context, segId, morphology, assumeNew, ct);
        var categoryRows = await RebuildCategory(context, segId, category, assumeNew, ct);
        var accessRows = await RebuildAccess(context, segId, access, assumeNew, ct);
        var surfaceRows = await RebuildSurface(context, segId, surface, assumeNew, ct);
        var streetNameRows = await RebuildStreetName(context, segId, streetName, assumeNew, ct);
        var maintainerRows = await RebuildMaintainer(context, segId, maintainer, assumeNew, ct);
        var carRows = await RebuildCar(context, segId, car, assumeNew, ct);
        var bikeRows = await RebuildBike(context, segId, bike, assumeNew, ct);
        var pedestrianRows = await RebuildPedestrian(context, segId, pedestrian, assumeNew, ct);

        await RebuildEuropeanRoads(context, segId, europeanRoadNumbers, provenance, assumeNew, ct);
        await RebuildNationalRoads(context, segId, nationalRoadNumbers, provenance, assumeNew, ct);

        await RebuildDerived(context, segId, normal, morphologyRows, categoryRows, accessRows, surfaceRows, streetNameRows, maintainerRows, carRows, bikeRows, pedestrianRows, assumeNew, ct);
    }

    // Partial update: only the non-null arguments are applied to the segment; the rest are kept as stored. Used for
    // modify / geometry-modified / streetname-changed / split, which each carry only a subset of the attributes.
    private static async Task WritePartial(PbsContext context, int segId,
        RoadSegmentGeometry geometry, RoadSegmentStatusV2 status, RoadSegmentGeometryDrawMethodV2 method,
        RoadNodeId? startNode, RoadNodeId? endNode,
        RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> morphology,
        RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> category,
        RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> access,
        RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> surface,
        RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetName,
        RoadSegmentDynamicAttributeValues<OrganizationId> maintainer,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> car,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> bike,
        RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> pedestrian,
        ProvenanceData provenance, CancellationToken ct)
    {
        var (normal, isNew) = await LoadOrCreate(context, segId, provenance);

        if (geometry is not null)
        {
            normal.GEOMETRIE = geometry.EnsureLambert08().RoundToCm().Value;
        }
        if (status is not null)
        {
            normal.STATUS = status.Translation.Identifier;
            normal.LBLSTATUS = status.Translation.Name;
        }
        if (method is not null)
        {
            normal.METHODE = method.Translation.Identifier;
            normal.LBLMETHODE = method.Translation.Name;
        }
        if (startNode is not null)
        {
            normal.B_WK_OIDN = startNode.Value.ToInt32();
        }
        if (endNode is not null)
        {
            normal.E_WK_OIDN = endNode.Value.ToInt32();
        }
        normal.VERSIE = provenance.ToPbsDate();
        if (isNew)
        {
            context.RoadSegments.Add(normal);
        }

        // Rebuild the attributes this event changed; keep the rest as stored. The flattener needs the full current set,
        // so unchanged attributes are loaded back from the database.
        var morphologyRows = morphology is not null ? await RebuildMorphology(context, segId, morphology, false, ct) : await context.RoadSegmentMorphologyAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var categoryRows = category is not null ? await RebuildCategory(context, segId, category, false, ct) : await context.RoadSegmentCategoryAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var accessRows = access is not null ? await RebuildAccess(context, segId, access, false, ct) : await context.RoadSegmentAccessRestrictionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var surfaceRows = surface is not null ? await RebuildSurface(context, segId, surface, false, ct) : await context.RoadSegmentSurfaceTypeAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var streetNameRows = streetName is not null ? await RebuildStreetName(context, segId, streetName, false, ct) : await context.RoadSegmentStreetNameAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var maintainerRows = maintainer is not null ? await RebuildMaintainer(context, segId, maintainer, false, ct) : await context.RoadSegmentMaintenanceAuthorityAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var carRows = car is not null ? await RebuildCar(context, segId, car, false, ct) : await context.RoadSegmentCarTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var bikeRows = bike is not null ? await RebuildBike(context, segId, bike, false, ct) : await context.RoadSegmentBikeTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var pedestrianRows = pedestrian is not null ? await RebuildPedestrian(context, segId, pedestrian, false, ct) : await context.RoadSegmentPedestrianTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct);

        await RebuildDerived(context, segId, normal, morphologyRows, categoryRows, accessRows, surfaceRows, streetNameRows, maintainerRows, carRows, bikeRows, pedestrianRows, false, ct);
    }

    private static async Task<(RoadSegmentRecord Normal, bool IsNew)> LoadOrCreate(PbsContext context, int segId, ProvenanceData provenance)
    {
        var normal = await context.RoadSegments.FindAsync(segId);
        var isNew = normal is null;
        normal ??= new RoadSegmentRecord { WS_OIDN = segId, CREATIE = provenance.ToPbsDate() };
        return (normal, isNew);
    }

    private static async Task RebuildDerived(PbsContext context, int segId, RoadSegmentRecord normal,
        IReadOnlyList<RoadSegmentMorphologyAttributeRecord> morphology,
        IReadOnlyList<RoadSegmentCategoryAttributeRecord> category,
        IReadOnlyList<RoadSegmentAccessRestrictionAttributeRecord> access,
        IReadOnlyList<RoadSegmentSurfaceTypeAttributeRecord> surface,
        IReadOnlyList<RoadSegmentStreetNameAttributeRecord> streetName,
        IReadOnlyList<RoadSegmentMaintenanceAuthorityAttributeRecord> maintainer,
        IReadOnlyList<RoadSegmentCarTrafficDirectionAttributeRecord> car,
        IReadOnlyList<RoadSegmentBikeTrafficDirectionAttributeRecord> bike,
        IReadOnlyList<RoadSegmentPedestrianTrafficDirectionAttributeRecord> pedestrian,
        bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
        {
            context.DerivedRoadSegments.RemoveRange(await context.DerivedRoadSegments.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        }
        if (normal.GEOMETRIE is null)
        {
            return;
        }

        var flatRows = DerivedWegsegmentFlattener.Flatten(segId, normal.GEOMETRIE,
            normal.STATUS, normal.LBLSTATUS, normal.METHODE, normal.LBLMETHODE, normal.B_WK_OIDN, normal.E_WK_OIDN,
            morphology, category, access, surface, streetName, maintainer, car, bike, pedestrian,
            normal.CREATIE, normal.VERSIE);
        context.DerivedRoadSegments.AddRange(flatRows);
    }

    private static async Task Remove(PbsContext context, int segId, CancellationToken ct)
    {
        var normal = await context.RoadSegments.FindAsync([segId], ct);
        if (normal is not null)
        {
            context.RoadSegments.Remove(normal);
        }
        context.DerivedRoadSegments.RemoveRange(await context.DerivedRoadSegments.Where(x => x.WS_OIDN == segId).ToListAsync(ct));

        context.RoadSegmentMorphologyAttributes.RemoveRange(await context.RoadSegmentMorphologyAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentStreetNameAttributes.RemoveRange(await context.RoadSegmentStreetNameAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentAccessRestrictionAttributes.RemoveRange(await context.RoadSegmentAccessRestrictionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentCarTrafficDirectionAttributes.RemoveRange(await context.RoadSegmentCarTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentBikeTrafficDirectionAttributes.RemoveRange(await context.RoadSegmentBikeTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentPedestrianTrafficDirectionAttributes.RemoveRange(await context.RoadSegmentPedestrianTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentMaintenanceAuthorityAttributes.RemoveRange(await context.RoadSegmentMaintenanceAuthorityAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentCategoryAttributes.RemoveRange(await context.RoadSegmentCategoryAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.RoadSegmentSurfaceTypeAttributes.RemoveRange(await context.RoadSegmentSurfaceTypeAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.EuropeanRoads.RemoveRange(await context.EuropeanRoads.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        context.NationalRoads.RemoveRange(await context.NationalRoads.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
    }

    private static async Task<List<RoadSegmentMorphologyAttributeRecord>> RebuildMorphology(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentMorphologyAttributes.RemoveRange(await c.RoadSegmentMorphologyAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentMorphologyAttributeRecord { WS_OIDN = segId, MORF = v.Value?.Translation.Identifier, LBLMORF = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentMorphologyAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentCategoryAttributeRecord>> RebuildCategory(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentCategoryAttributes.RemoveRange(await c.RoadSegmentCategoryAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentCategoryAttributeRecord { WS_OIDN = segId, WEGCAT = v.Value?.Translation.Identifier, LBLWEGCAT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentCategoryAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentAccessRestrictionAttributeRecord>> RebuildAccess(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentAccessRestrictionAttributes.RemoveRange(await c.RoadSegmentAccessRestrictionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentAccessRestrictionAttributeRecord { WS_OIDN = segId, TOEGANG = v.Value?.Translation.Identifier, LBLTOEGANG = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentAccessRestrictionAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentSurfaceTypeAttributeRecord>> RebuildSurface(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentSurfaceTypeAttributes.RemoveRange(await c.RoadSegmentSurfaceTypeAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentSurfaceTypeAttributeRecord { WS_OIDN = segId, VERHARDING = v.Value?.Translation.Identifier, LBLVERHARD = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentSurfaceTypeAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentStreetNameAttributeRecord>> RebuildStreetName(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<StreetNameLocalId> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentStreetNameAttributes.RemoveRange(await c.RoadSegmentStreetNameAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentStreetNameAttributeRecord { WS_OIDN = segId, STRTNMID = v.Value.ToInt32(), KANT = v.Side.Translation.Identifier, LBLKANT = v.Side.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentStreetNameAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentMaintenanceAuthorityAttributeRecord>> RebuildMaintainer(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<OrganizationId> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentMaintenanceAuthorityAttributes.RemoveRange(await c.RoadSegmentMaintenanceAuthorityAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentMaintenanceAuthorityAttributeRecord { WS_OIDN = segId, BEHEER = v.Value.ToString(), KANT = v.Side.Translation.Identifier, LBLKANT = v.Side.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentMaintenanceAuthorityAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentCarTrafficDirectionAttributeRecord>> RebuildCar(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentCarTrafficDirectionAttributes.RemoveRange(await c.RoadSegmentCarTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentCarTrafficDirectionAttributeRecord { WS_OIDN = segId, RICHTING = v.Value?.Translation.Identifier, LBLRICHT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentCarTrafficDirectionAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentBikeTrafficDirectionAttributeRecord>> RebuildBike(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentBikeTrafficDirectionAttributes.RemoveRange(await c.RoadSegmentBikeTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentBikeTrafficDirectionAttributeRecord { WS_OIDN = segId, RICHTING = v.Value?.Translation.Identifier, LBLRICHT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentBikeTrafficDirectionAttributes.AddRange(rows);
        return rows;
    }

    private static async Task<List<RoadSegmentPedestrianTrafficDirectionAttributeRecord>> RebuildPedestrian(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> values, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.RoadSegmentPedestrianTrafficDirectionAttributes.RemoveRange(await c.RoadSegmentPedestrianTrafficDirectionAttributes.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        var rows = values.Values.Select(v => new RoadSegmentPedestrianTrafficDirectionAttributeRecord { WS_OIDN = segId, RICHTING = v.Value?.Translation.Identifier, LBLRICHT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentPedestrianTrafficDirectionAttributes.AddRange(rows);
        return rows;
    }

    private static async Task RebuildEuropeanRoads(PbsContext c, int segId, IReadOnlyCollection<EuropeanRoadNumber> numbers, ProvenanceData provenance, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.EuropeanRoads.RemoveRange(await c.EuropeanRoads.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        foreach (var number in numbers)
            c.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = number.ToString(), CREATIE = provenance.ToPbsDate(), VERSIE = provenance.ToPbsDate() });
    }

    private static async Task RebuildNationalRoads(PbsContext c, int segId, IReadOnlyCollection<NationalRoadNumber> numbers, ProvenanceData provenance, bool assumeNew, CancellationToken ct)
    {
        if (!assumeNew)
            c.NationalRoads.RemoveRange(await c.NationalRoads.Where(x => x.WS_OIDN == segId).ToListAsync(ct));
        foreach (var number in numbers)
            c.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = number.ToString(), CREATIE = provenance.ToPbsDate(), VERSIE = provenance.ToPbsDate() });
    }

    // V1 add/import: full write of the segment. Both callers (Imported/Added) are created events, so the segment cannot
    // exist yet (assumeNew) — the lookup and all attribute/road delete queries are skipped. Maps the legacy string values
    // to V2 (null when unmapped) and sets the European/national roads from the event (Imported carries them inline; Added
    // starts empty and gets separate events).
    private static Task WriteV1Full(PbsContext context, int segId, RoadSegmentGeometry geometry,
        string status, string method, int startNode, int endNode,
        string morphology, string category, string accessRestriction,
        IReadOnlyList<(double From, double To, string Type)> surfaces,
        int? leftStreetNameId, int? rightStreetNameId, string maintenanceAuthorityCode,
        IReadOnlyCollection<EuropeanRoadNumber> europeanRoadNumbers, IReadOnlyCollection<NationalRoadNumber> nationalRoadNumbers,
        ProvenanceData provenance, CancellationToken ct)
    {
        var length = geometry.EnsureLambert08().RoundToCm().Value.Length;
        var v2Method = V1ToV2.Method(method);
        var v2Status = V1ToV2.Status(status, v2Method);
        return WriteFull(context, segId, true, geometry, v2Status, v2Method,
            new RoadNodeId(startNode), new RoadNodeId(endNode),
            ForEntireGeometry(V1ToV2.Morphology(morphology), length),
            ForEntireGeometry(V1ToV2.Category(category), length),
            ForEntireGeometry(V1ToV2.AccessRestriction(accessRestriction), length),
            SurfaceFromV1(surfaces, length),
            StreetNameFromV1(leftStreetNameId, rightStreetNameId, length),
            MaintainerFromV1(maintenanceAuthorityCode, length),
            new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(),
            new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(),
            new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>(),
            europeanRoadNumbers, nationalRoadNumbers, provenance, ct);
    }

    // V1 modify: rewrites every attribute the event carries but leaves the European/national roads untouched (those are
    // maintained through their own add/remove events).
    private static Task WriteV1Modified(PbsContext context, int segId, RoadSegmentGeometry geometry,
        string status, string method, int startNode, int endNode,
        string morphology, string category, string accessRestriction,
        IReadOnlyList<(double From, double To, string Type)> surfaces,
        int? leftStreetNameId, int? rightStreetNameId, string maintenanceAuthorityCode,
        ProvenanceData provenance, CancellationToken ct)
    {
        var length = geometry.EnsureLambert08().RoundToCm().Value.Length;
        var v2Method = V1ToV2.Method(method);
        var v2Status = V1ToV2.Status(status, v2Method);
        return WritePartial(context, segId, geometry, v2Status, v2Method,
            new RoadNodeId(startNode), new RoadNodeId(endNode),
            ForEntireGeometry(V1ToV2.Morphology(morphology), length),
            ForEntireGeometry(V1ToV2.Category(category), length),
            ForEntireGeometry(V1ToV2.AccessRestriction(accessRestriction), length),
            SurfaceFromV1(surfaces, length),
            StreetNameFromV1(leftStreetNameId, rightStreetNameId, length),
            MaintainerFromV1(maintenanceAuthorityCode, length),
            new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(),
            new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(),
            new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>(),
            provenance, ct);
    }

    // A single value spanning the whole segment ([0, length], both sides). Value may be null (no V2 mapping).
    private static RoadSegmentDynamicAttributeValues<T> ForEntireGeometry<T>(T value, double length)
        where T : class
    {
        return new RoadSegmentDynamicAttributeValues<T>([
            new RoadSegmentDynamicAttributeValue<T>
            {
                Coverage = new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(length)),
                Side = RoadSegmentAttributeSide.Beide,
                Value = value
            }
        ]);
    }

    private static RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceFromV1(IReadOnlyList<(double From, double To, string Type)> surfaces, double length)
    {
        return new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(
            surfaces
                .OrderBy(x => x.From)
                .Select((x, i) => (
                    new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(x.From), new RoadSegmentPositionV2(i == surfaces.Count - 1 ? length : x.To)),
                    RoadSegmentAttributeSide.Beide,
                    V1ToV2.SurfaceType(x.Type))));
    }

    // V1 street names are given per side; equal (or both absent) sides collapse to a single Both value.
    private static RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameFromV1(int? leftStreetNameId, int? rightStreetNameId, double length)
    {
        var to = new RoadSegmentPositionV2(length);
        if (leftStreetNameId is null && rightStreetNameId is null)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>([
                new RoadSegmentDynamicAttributeValue<StreetNameLocalId> { Coverage = new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, to), Side = RoadSegmentAttributeSide.Beide, Value = StreetNameLocalId.NotApplicable }
            ]);
        }

        if (leftStreetNameId == rightStreetNameId)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>([
                new RoadSegmentDynamicAttributeValue<StreetNameLocalId> { Coverage = new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, to), Side = RoadSegmentAttributeSide.Beide, Value = new StreetNameLocalId(leftStreetNameId.Value) }
            ]);
        }

        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>([
            new RoadSegmentDynamicAttributeValue<StreetNameLocalId> { Coverage = new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, to), Side = RoadSegmentAttributeSide.Links, Value = StreetNameLocalId.FromValue(leftStreetNameId) ?? StreetNameLocalId.NotApplicable },
            new RoadSegmentDynamicAttributeValue<StreetNameLocalId> { Coverage = new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, to), Side = RoadSegmentAttributeSide.Rechts, Value = StreetNameLocalId.FromValue(rightStreetNameId) ?? StreetNameLocalId.NotApplicable }
        ]);
    }

    private static RoadSegmentDynamicAttributeValues<OrganizationId> MaintainerFromV1(string maintenanceAuthorityCode, double length)
    {
        return new RoadSegmentDynamicAttributeValues<OrganizationId>([
            new RoadSegmentDynamicAttributeValue<OrganizationId> { Coverage = new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(length)), Side = RoadSegmentAttributeSide.Beide, Value = new OrganizationId(maintenanceAuthorityCode) }
        ]);
    }

    // The current per-side street name ids, used by the partial V1 events to keep the side they do not change.
    private static async Task<(int? Left, int? Right)> CurrentStreetNameSidesFromV1(PbsContext context, int segId, CancellationToken ct)
    {
        var rows = await context.RoadSegmentStreetNameAttributes.AsNoTracking().Where(x => x.WS_OIDN == segId).ToListAsync(ct);
        var left = rows.FirstOrDefault(x => x.KANT == RoadSegmentAttributeSide.Beide.Translation.Identifier || x.KANT == RoadSegmentAttributeSide.Links.Translation.Identifier);
        var right = rows.FirstOrDefault(x => x.KANT == RoadSegmentAttributeSide.Beide.Translation.Identifier || x.KANT == RoadSegmentAttributeSide.Rechts.Translation.Identifier);
        return (left?.STRTNMID, right?.STRTNMID);
    }
}
