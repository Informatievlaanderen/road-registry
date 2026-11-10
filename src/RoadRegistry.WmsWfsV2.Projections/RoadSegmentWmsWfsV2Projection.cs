namespace RoadRegistry.WmsWfsV2.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;
using RoadRegistry.BackOffice.Extensions;

public class RoadSegmentWmsWfsV2Projection : RunnerDbContextRoadNetworkChangesProjection<WmsWfsV2Context>,
    IProjectionCatchUpAware, IBatchPrefetchingRoadNetworkChangesProjection<WmsWfsV2Context>
{
    private readonly DerivedLabelCache _labelCache;

    // Which context the rows below were loaded into, and whose. Both halves are load-bearing: the ids say which
    // segments were loaded, the context into which change tracker, since nothing stops the driver from skipping
    // the prefetch or handing out a different context in between.
    // Weak, because this projection outlives the batch and the driver disposes the context at the end of
    // one. Only its identity is wanted - "are these the rows of the context I am being handed?" - and a
    // strong field would keep a disposed context and its whole change tracker alive until the next batch
    // overwrote it, which on a projection that has caught up is indefinitely.
    private readonly WeakReference<WmsWfsV2Context> _prefetchedContext = new(null!);
    private HashSet<int> _prefetchedSegmentIds = [];

    // The cache is shared with the sibling sub-projections that write the label tables. Constructed on its own
    // - as the tests do - it gets a private one, which is never loaded and so always resolves against the database.
    public RoadSegmentWmsWfsV2Projection(DerivedLabelCache? labelCache = null)
    {
        _labelCache = labelCache ?? new DerivedLabelCache();

        // V1
        // Legacy events carry raw string values; each is mapped to its V2 equivalent (see V1ToV2), storing null when a
        // value has no V2 mapping. Traffic direction does not exist in V1, so it is left empty. Numbered roads are not
        // part of the WMS v2 product, so those events are ignored.
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
            var record = await FindSegment(context, m.RoadSegmentId, ct);
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
                var (currentLeft, currentRight) = CurrentStreetNameSides(record.DynamicAttributes.StreetName);
                streetName = StreetNameFromV1(m.LeftSide?.StreetNameId ?? currentLeft, m.RightSide?.StreetNameId ?? currentRight, length);
            }

            await WritePartial(context, m.RoadSegmentId, null, status, null, null,
                morphology, category, access, surface, streetName, maintainer, null, null, null, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentGeometryModified>>((context, e, ct) =>
        {
            var length = e.Data.Geometry.EnsureLambert08().RoundToCm().Value.Length;
            return WritePartial(context, e.Data.RoadSegmentId, e.Data.Geometry, null, null, null,
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
            var record = await FindSegment(context, e.Data.RoadSegmentId, ct);
            if (record?.GEOMETRIE is null)
            {
                return;
            }
            var (currentLeft, currentRight) = CurrentStreetNameSides(record.DynamicAttributes.StreetName);
            var streetName = StreetNameFromV1(e.Data.LeftSideStreetNameId ?? currentLeft, e.Data.RightSideStreetNameId ?? currentRight, record.GEOMETRIE.Length);
            await WritePartial(context, e.Data.RoadSegmentId, null, null, null, null, null, null, null, null,
                streetName, null, null, null, null, e.Data.Provenance, ct);
        });

        When<IEvent<RoadSegmentRemoved>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId, ct));
        // OutlinedRoadSegmentRemoved removes nothing here. In the old system it moved a road segment to another
        // SqlStreamStore stream when its geometry stopped being outlined; the outlined variant was removed while the
        // road segment itself was measured and stayed. Marten has no such streams, so there is nothing to do - the
        // read and extract projections and the network topology all ignore it for the same reason.
        When<IEvent<OutlinedRoadSegmentRemoved>>((_, _, _) => Task.CompletedTask);

        When<IEvent<RoadSegmentAddedToEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = EuropeanRoadNumber.Parse(e.Data.Number).ToString();
            if (!await context.EuropeanRoads.IncludeLocalAnyAsync(x => x.WS_OIDN == segId && x.EUNUMMER == nummer, ct))
            {
                context.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = nummer, CREATIE = e.Data.Provenance.Timestamp.ToDateTimeOffset(), VERSIE = e.Data.Provenance.Timestamp.ToDateTimeOffset() });
            }
            await RefreshDerivedEuropeanNumbers(context, segId, nummer, null, ct);
        });
        When<IEvent<RoadSegmentRemovedFromEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = EuropeanRoadNumber.Parse(e.Data.Number).ToString();
            var rows = await context.EuropeanRoads.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId && x.EUNUMMER == nummer), ct);
            context.EuropeanRoads.RemoveRange(rows);
            await RefreshDerivedEuropeanNumbers(context, segId, null, nummer, ct);
        });
        When<IEvent<RoadSegmentAddedToNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = NationalRoadNumber.Parse(e.Data.Number).ToString();
            if (!await context.NationalRoads.IncludeLocalAnyAsync(x => x.WS_OIDN == segId && x.NWNUMMER == nummer, ct))
            {
                context.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = nummer, CREATIE = e.Data.Provenance.Timestamp.ToDateTimeOffset(), VERSIE = e.Data.Provenance.Timestamp.ToDateTimeOffset() });
            }
            await RefreshDerivedNationalNumbers(context, segId, nummer, null, ct);
        });
        When<IEvent<RoadSegmentRemovedFromNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = NationalRoadNumber.Parse(e.Data.Number).ToString();
            var rows = await context.NationalRoads.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId && x.NWNUMMER == nummer), ct);
            context.NationalRoads.RemoveRange(rows);
            await RefreshDerivedNationalNumbers(context, segId, null, nummer, ct);
        });
        When<IEvent<RoadSegmentAddedToNumberedRoad>>((_, _, _) => Task.CompletedTask);
        When<IEvent<RoadSegmentRemovedFromNumberedRoad>>((_, _, _) => Task.CompletedTask);

        // V2
        When<IEvent<RoadSegmentWasAdded>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), m.Geometry, m.Status, m.GeometryDrawMethod,
                m.StartNodeId, m.EndNodeId, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.EuropeanRoadNumbers, m.NationalRoadNumbers, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasMigrated>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), m.Geometry, m.Status, m.GeometryDrawMethod,
                m.StartNodeId, m.EndNodeId, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.EuropeanRoadNumbers, m.NationalRoadNumbers, m.Provenance, ct);
        });

        When<IEvent<OutlinedRoadSegmentWasAdded>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), m.Geometry, m.Status, RoadSegmentGeometryDrawMethodV2.Ingeschetst,
                null, null, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, [], [], m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasMerged>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WriteFull(context, m.RoadSegmentId.ToInt32(), m.Geometry, m.Status, m.GeometryDrawMethod,
                m.StartNodeId, m.EndNodeId, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.EuropeanRoadNumbers, m.NationalRoadNumbers, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasChanged>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), m.Geometry, null, null,
                m.NodeIds, m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
                m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
                m.PedestrianTrafficDirection, m.Provenance, ct);
        });

        // The status changes. Each transition raises an event of its own - the status a segment takes on is the
        // event itself - but what a projection has to do with one follows entirely from the kind of change it is, so
        // the three shapes below are all there is. RoadSegmentStatusChange.ForEvent supplies the status.
        When<IEvent<RoadSegmentWasRealizedFromPlanned>>((context, e, ct) => ProjectConnected(context, e.Data, ct));
        When<IEvent<RoadSegmentWasRealizedFromOutOfUse>>((context, e, ct) => ProjectConnected(context, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromHistorizedToRealized>>((context, e, ct) => ProjectConnected(context, e.Data, ct));

        When<IEvent<RoadSegmentWasCorrectedFromRealizedToPlanned>>((context, e, ct) => ProjectDisconnected(context, e.Data, ct));
        When<IEvent<RoadSegmentWasTakenOutOfUseFromRealized>>((context, e, ct) => ProjectDisconnected(context, e.Data, ct));
        When<IEvent<RoadSegmentWasHistorizedFromRealized>>((context, e, ct) => ProjectDisconnected(context, e.Data, ct));

        When<IEvent<RoadSegmentWasHistorizedFromOutOfUse>>((context, e, ct) => ProjectUnconnectedStatusChange(context, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromNotRealizedToPlanned>>((context, e, ct) => ProjectUnconnectedStatusChange(context, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromHistorizedToOutOfUse>>((context, e, ct) => ProjectUnconnectedStatusChange(context, e.Data, ct));
        When<IEvent<RoadSegmentWasNotRealizedFromPlanned>>((context, e, ct) => ProjectUnconnectedStatusChange(context, e.Data, ct));
        When<IEvent<RoadSegmentWasHistorizedFromPlanned>>((context, e, ct) => ProjectUnconnectedStatusChange(context, e.Data, ct));
        When<IEvent<RoadSegmentWasHistorizedFromNotRealized>>((context, e, ct) => ProjectUnconnectedStatusChange(context, e.Data, ct));
        When<IEvent<RoadSegmentWasCorrectedFromHistorizedToPlanned>>((context, e, ct) => ProjectUnconnectedStatusChange(context, e.Data, ct));
        When<IEvent<RoadSegmentWasMeasured>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), null, null, RoadSegmentGeometryDrawMethodV2.Ingemeten, null, null, null,
                null, null, null, null, null, null, null, m.Provenance, ct);
        });
        When<IEvent<RoadSegmentWasCorrectedToOutlined>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), null, null, RoadSegmentGeometryDrawMethodV2.Ingeschetst, null, null, null,
                null, null, null, null, null, null, null, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentStreetNameIdWasChanged>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), null, null, null, null, null, null, null, null,
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
                new RoadSegmentNodeIds { Start = mod.StartNodeId, End = mod.EndNodeId },
                mod.Morphology, mod.Category, mod.AccessRestriction, mod.SurfaceType,
                mod.StreetNameId, mod.MaintenanceAuthorityId, mod.CarTrafficDirection, mod.BikeTrafficDirection,
                mod.PedestrianTrafficDirection, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentWasRemoved>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentWasRemovedBecauseOfMigration>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentWasRetiredBecauseOfMerger>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentWasRetiredBecauseOfSplit>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId.ToInt32(), ct));

        When<IEvent<RoadSegmentWasAddedToEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            if (!await context.EuropeanRoads.IncludeLocalAnyAsync(x => x.WS_OIDN == segId && x.EUNUMMER == nummer, ct))
            {
                context.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = nummer, CREATIE = e.Data.Provenance.Timestamp.ToDateTimeOffset(), VERSIE = e.Data.Provenance.Timestamp.ToDateTimeOffset() });
            }
            await RefreshDerivedEuropeanNumbers(context, segId, nummer, null, ct);
        });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            var rows = await context.EuropeanRoads.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId && x.EUNUMMER == nummer), ct);
            context.EuropeanRoads.RemoveRange(rows);
            await RefreshDerivedEuropeanNumbers(context, segId, null, nummer, ct);
        });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            if (!await context.NationalRoads.IncludeLocalAnyAsync(x => x.WS_OIDN == segId && x.NWNUMMER == nummer, ct))
            {
                context.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = nummer, CREATIE = e.Data.Provenance.Timestamp.ToDateTimeOffset(), VERSIE = e.Data.Provenance.Timestamp.ToDateTimeOffset() });
            }
            await RefreshDerivedNationalNumbers(context, segId, nummer, null, ct);
        });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            var rows = await context.NationalRoads.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId && x.NWNUMMER == nummer), ct);
            context.NationalRoads.RemoveRange(rows);
            await RefreshDerivedNationalNumbers(context, segId, null, nummer, ct);
        });
    }

    private async Task WriteFull(WmsWfsV2Context context, int segId,
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
        // Always a lookup, and always the deletes below. The shortcut that skipped both assumed a created event
        // arrives once per id; the projection-state position it leaned on is a position, and a replay from before it -
        // a recovery, a rebuild - delivers the event again. That turned the insert into a primary key violation and
        // left the previous derived rows and road numbers behind.
        var (normal, isNew) = await LoadOrCreate(context, segId, provenance, ct);

        normal.GEOMETRIE = geometry.EnsureLambert08().RoundToCm().Value.Force2D();
        normal.STATUS = status?.Translation.Identifier;
        normal.LBLSTATUS = status?.Translation.Name;
        normal.METHODE = method?.Translation.Identifier;
        normal.LBLMETHODE = method?.Translation.Name;
        normal.B_WK_OIDN = startNode?.ToInt32();
        normal.E_WK_OIDN = endNode?.ToInt32();
        normal.VERSIE = provenance.Timestamp.ToDateTimeOffset();
        normal.DynamicAttributes = new RoadSegmentDynamicAttributes
        {
            Morphology = BuildMorphology(morphology),
            Category = BuildCategory(category),
            AccessRestriction = BuildAccess(access),
            SurfaceType = BuildSurface(surface),
            StreetName = BuildStreetName(streetName),
            MaintenanceAuthority = BuildMaintainer(maintainer),
            CarTrafficDirection = BuildCar(car),
            BikeTrafficDirection = BuildBike(bike),
            PedestrianTrafficDirection = BuildPedestrian(pedestrian)
        };
        if (isNew)
        {
            context.RoadSegments.Add(normal);
        }

        await RebuildEuropeanRoads(context, segId, europeanRoadNumbers, provenance, ct);
        await RebuildNationalRoads(context, segId, nationalRoadNumbers, provenance, ct);

        var euNummers = AggregateRoadNumbers(europeanRoadNumbers.Select(x => x.ToString()));
        var nwNummers = AggregateRoadNumbers(nationalRoadNumbers.Select(x => x.ToString()));
        await RebuildDerived(context, segId, normal, euNummers, nwNummers, ct);
    }

    // Partial update: only the non-null arguments are applied to the segment; the rest are kept as stored. Used for
    // modify / geometry-modified / streetname-changed / split, which each carry only a subset of the attributes. The
    // unchanged dynamic attributes come straight from the loaded segment's JSON blob — no extra queries.
    // Knotted into the network: the event records the realized state in full. The geometry draw method is not
    // touched by a status change.
    private Task ProjectConnected(WmsWfsV2Context context, IRoadSegmentWasConnectedEvent m, CancellationToken ct)
    {
        return WritePartial(context, m.RoadSegmentId.ToInt32(), m.Geometry, RoadSegmentStatusChange.ForEvent(m).To, null,
            new RoadSegmentNodeIds { Start = m.StartNodeId, End = m.EndNodeId },
            m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
            m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
            m.PedestrianTrafficDirection, m.Provenance, ct);
    }

    // Unhooked from the network: only the status changes; the geometry and the attributes are untouched. The segment
    // came loose from its road nodes, so the node state is carried with both ids null.
    private Task ProjectDisconnected(WmsWfsV2Context context, IRoadSegmentWasDisconnectedEvent m, CancellationToken ct)
    {
        return WritePartial(context, m.RoadSegmentId.ToInt32(), null, RoadSegmentStatusChange.ForEvent(m).To, null,
            new RoadSegmentNodeIds(), null, null, null, null, null, null, null, null, null, m.Provenance, ct);
    }

    // Outside the network before and after, so nothing but the status moves - the road nodes are left alone.
    private Task ProjectUnconnectedStatusChange(WmsWfsV2Context context, IRoadSegmentUnconnectedStatusChangeEvent m, CancellationToken ct)
    {
        return WritePartial(context, m.RoadSegmentId.ToInt32(), null, RoadSegmentStatusChange.ForEvent(m).To, null,
            null, null, null, null, null, null, null, null, null, null, m.Provenance, ct);
    }

    private async Task WritePartial(WmsWfsV2Context context, int segId,
        RoadSegmentGeometry? geometry, RoadSegmentStatusV2? status, RoadSegmentGeometryDrawMethodV2? method,
        RoadSegmentNodeIds? nodeIds,
        RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>? morphology,
        RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>? category,
        RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>? access,
        RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>? surface,
        RoadSegmentDynamicAttributeValues<StreetNameLocalId>? streetName,
        RoadSegmentDynamicAttributeValues<OrganizationId>? maintainer,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? car,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? bike,
        RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>? pedestrian,
        ProvenanceData provenance, CancellationToken ct)
    {
        var (normal, isNew) = await LoadOrCreate(context, segId, provenance, ct);

        if (geometry is not null)
        {
            normal.GEOMETRIE = geometry.EnsureLambert08().RoundToCm().Value.Force2D();
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
        // Null means "not carried by this event"; a value is the full new node state, where a null id inside
        // it clears the column, because a segment that is not realized has no road nodes at all.
        if (nodeIds is not null)
        {
            normal.B_WK_OIDN = nodeIds.Start?.ToInt32();
            normal.E_WK_OIDN = nodeIds.End?.ToInt32();
        }
        normal.VERSIE = provenance.Timestamp.ToDateTimeOffset();

        var current = normal.DynamicAttributes;
        normal.DynamicAttributes = new RoadSegmentDynamicAttributes
        {
            Morphology = morphology is not null ? BuildMorphology(morphology) : current.Morphology,
            Category = category is not null ? BuildCategory(category) : current.Category,
            AccessRestriction = access is not null ? BuildAccess(access) : current.AccessRestriction,
            SurfaceType = surface is not null ? BuildSurface(surface) : current.SurfaceType,
            StreetName = streetName is not null ? BuildStreetName(streetName) : current.StreetName,
            MaintenanceAuthority = maintainer is not null ? BuildMaintainer(maintainer) : current.MaintenanceAuthority,
            CarTrafficDirection = car is not null ? BuildCar(car) : current.CarTrafficDirection,
            BikeTrafficDirection = bike is not null ? BuildBike(bike) : current.BikeTrafficDirection,
            PedestrianTrafficDirection = pedestrian is not null ? BuildPedestrian(pedestrian) : current.PedestrianTrafficDirection
        };
        if (isNew)
        {
            context.RoadSegments.Add(normal);
        }

        // Road numbers are not carried by the partial events, so keep the stored aggregate from the road tables.
        var euNummers = AggregateRoadNumbers((await RowsFor(context, context.EuropeanRoads, segId, ct)).Select(x => x.EUNUMMER).ToList());
        var nwNummers = AggregateRoadNumbers((await RowsFor(context, context.NationalRoads, segId, ct)).Select(x => x.NWNUMMER).ToList());
        await RebuildDerived(context, segId, normal, euNummers, nwNummers, ct);
    }

    private async Task<(RoadSegmentRecord Normal, bool IsNew)> LoadOrCreate(WmsWfsV2Context context, int segId, ProvenanceData provenance, CancellationToken ct)
    {
        var normal = await FindSegment(context, segId, ct);
        var isNew = normal is null;
        normal ??= new RoadSegmentRecord { WS_OIDN = segId, CREATIE = provenance.Timestamp.ToDateTimeOffset() };
        return (normal, isNew);
    }

    private async Task RebuildDerived(WmsWfsV2Context context, int segId, RoadSegmentRecord normal, string? euNummers, string? nwNummers, CancellationToken ct)
    {
        {
            context.DerivedRoadSegments.RemoveRange(await RowsFor(context, context.DerivedRoadSegments, segId, ct));
        }
        if (normal.GEOMETRIE is null)
        {
            return;
        }

        var a = normal.DynamicAttributes;
        var flatRows = DerivedWegsegmentFlattener.Flatten(segId, normal.GEOMETRIE,
            normal.STATUS, normal.LBLSTATUS, normal.METHODE, normal.LBLMETHODE, normal.B_WK_OIDN, normal.E_WK_OIDN,
            a.Morphology, a.Category, a.AccessRestriction, a.SurfaceType, a.StreetName, a.MaintenanceAuthority,
            a.CarTrafficDirection, a.BikeTrafficDirection, a.PedestrianTrafficDirection,
            euNummers, nwNummers,
            normal.CREATIE, normal.VERSIE);
        await ApplyStreetNameAndOrganizationLabels(context, flatRows, ct);
        context.DerivedRoadSegments.AddRange(flatRows);
    }

    // Resolve the denormalized street-name (LSTRNM/RSTRNM/STRNM) and maintainer (LBLBEHEER) labels for freshly derived
    // rows from the street-name and organization caches, so the WMS view can read them straight from the table.
    private async Task ApplyStreetNameAndOrganizationLabels(WmsWfsV2Context context, IReadOnlyList<DerivedRoadSegmentRecord> rows, CancellationToken ct)
    {
        var streetNameIds = rows
            .SelectMany(r => new[] { r.LSTRNMID, r.RSTRNMID })
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();
        var orgIds = rows
            .SelectMany(r => new[] { r.LBEHEER, r.RBEHEER })
            .Where(x => x is not null)
            .Select(x => x!)
            .Distinct()
            .ToList();

        // While catching up both lookup tables are held in memory, which is what removes the last per-event round trip
        // from the insert path. Live the cache stays empty and these resolve against the database as before.
        if (IsCatchingUp && !_labelCache.IsLoaded)
        {
            await _labelCache.LoadAsync(context, ct);
        }

        var streetNames = streetNameIds.Count == 0
            ? new Dictionary<int, string?>()
            : _labelCache.IsLoaded
                ? _labelCache.GetStreetNames(streetNameIds)
                : await context.StreetNameCache.Where(x => streetNameIds.Contains(x.StraatnaamId)).ToDictionaryAsync(x => x.StraatnaamId, x => x.Naam, ct);
        var orgNames = orgIds.Count == 0
            ? new Dictionary<string, string?>()
            : _labelCache.IsLoaded
                ? _labelCache.GetOrganizations(orgIds)
                : await context.OrganizationCache.Where(x => orgIds.Contains(x.OrganisatieId)).ToDictionaryAsync(x => x.OrganisatieId!, x => x.Naam, ct);

        foreach (var r in rows)
        {
            var lName = r.LSTRNMID is int lid && streetNames.TryGetValue(lid, out var ln) ? ln : null;
            var rName = r.RSTRNMID is int rid && streetNames.TryGetValue(rid, out var rn) ? rn : null;
            r.LSTRNM = lName;
            r.RSTRNM = rName;
            r.STRNM = WmsWfsV2DerivedLabels.BuildStreetNameLabel(r.LSTRNMID, r.RSTRNMID, lName, rName);

            var lOrg = r.LBEHEER is not null && orgNames.TryGetValue(r.LBEHEER, out var lo) ? lo : null;
            var rOrg = r.RBEHEER is not null && orgNames.TryGetValue(r.RBEHEER, out var ro) ? ro : null;
            r.LBLLBEHEER = lOrg;
            r.LBLRBEHEER = rOrg;
            r.LBLBEHEER = WmsWfsV2DerivedLabels.BuildMaintainerCategoryLabel(r.STATUS, r.LBEHEER, r.RBEHEER, lOrg, rOrg);
        }
    }

    public Task OnCatchUpStartedAsync(CancellationToken cancellationToken)
    {
        // Loaded lazily on first use instead, where a DbContext is at hand.
        return Task.CompletedTask;
    }

    public Task OnCatchUpFinishedAsync(CancellationToken cancellationToken)
    {
        _labelCache.Clear();
        return Task.CompletedTask;
    }

    // Distinct, alphabetically sorted road numbers joined with " / " (null when there are none).
    private static string? AggregateRoadNumbers(IEnumerable<string?> numbers)
    {
        var distinct = numbers
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x!)
            .Distinct()
            .OrderBy(x => x, System.StringComparer.Ordinal)
            .ToList();
        return distinct.Count > 0 ? string.Join(" / ", distinct) : null;
    }

    private static async Task SetDerivedEuNummer(WmsWfsV2Context c, int segId, string? value, CancellationToken ct)
    {
        foreach (var d in await c.DerivedRoadSegments.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId), ct))
        {
            d.EUNUMMERS = value;
        }
    }

    private static async Task SetDerivedNwNummer(WmsWfsV2Context c, int segId, string? value, CancellationToken ct)
    {
        foreach (var d in await c.DerivedRoadSegments.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId), ct))
        {
            d.NWNUMMERS = value;
        }
    }

    // Recompute the derived EUNUMMER after a European-road add/remove. The pending change is not yet in the database,
    // so it is folded into the queried set explicitly (add) or filtered out (remove).
    private static async Task RefreshDerivedEuropeanNumbers(WmsWfsV2Context c, int segId, string? added, string? removed, CancellationToken ct)
    {
        var numbers = (await c.EuropeanRoads
                .IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId && (removed == null || x.EUNUMMER != removed)), ct))
            .Select(x => x.EUNUMMER)
            .ToList();
        if (added != null)
        {
            numbers.Add(added);
        }
        await SetDerivedEuNummer(c, segId, AggregateRoadNumbers(numbers), ct);
    }

    private static async Task RefreshDerivedNationalNumbers(WmsWfsV2Context c, int segId, string? added, string? removed, CancellationToken ct)
    {
        var numbers = (await c.NationalRoads
                .IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId && (removed == null || x.NWNUMMER != removed)), ct))
            .Select(x => x.NWNUMMER)
            .ToList();
        if (added != null)
        {
            numbers.Add(added);
        }
        await SetDerivedNwNummer(c, segId, AggregateRoadNumbers(numbers), ct);
    }

    private async Task Remove(WmsWfsV2Context context, int segId, CancellationToken ct)
    {
        var normal = await FindSegment(context, segId, ct);
        if (normal is not null)
        {
            context.RoadSegments.Remove(normal);
        }
        context.DerivedRoadSegments.RemoveRange(await RowsFor(context, context.DerivedRoadSegments, segId, ct));
        context.EuropeanRoads.RemoveRange(await RowsFor(context, context.EuropeanRoads, segId, ct));
        context.NationalRoads.RemoveRange(await RowsFor(context, context.NationalRoads, segId, ct));
    }

    // One query per table for the whole batch instead of one per event. The three tables under the road segment are
    // read by WS_OIDN and give back a handful of rows, so against a database a network away the cost of those reads is
    // almost entirely the round trip: a hundred events cost a hundred round trips per table to fetch a few hundred
    // bytes each.
    //
    // What makes this sound rather than a cache is that it loads whole segments. Once every row of a segment is in the
    // change tracker, "the rows of this segment" can be answered from it - including the ones earlier events in the
    // same batch added or removed, which is more than the per-event query could say. A segment whose id cannot be read
    // from its event is left out and its handlers query for it as before.
    public async Task PrefetchBatchAsync(WmsWfsV2Context context, IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
    {
        // Dropped first: a prefetch that fails halfway must not leave behind a claim that the rows are loaded.
        _prefetchedContext.SetTarget(null!);
        _prefetchedSegmentIds = [];

        var segmentIds = events
            .Select(RoadSegmentEventId.Of)
            .Where(segmentId => segmentId is not null)
            .Select(segmentId => segmentId!.Value)
            .Distinct()
            .ToArray();
        if (segmentIds.Length == 0)
        {
            return;
        }

        await context.RoadSegments.Where(record => segmentIds.Contains(record.WS_OIDN)).LoadAsync(cancellationToken);
        await context.DerivedRoadSegments.Where(record => segmentIds.Contains(record.WS_OIDN)).LoadAsync(cancellationToken);
        await context.EuropeanRoads.Where(record => segmentIds.Contains(record.WS_OIDN)).LoadAsync(cancellationToken);
        await context.NationalRoads.Where(record => segmentIds.Contains(record.WS_OIDN)).LoadAsync(cancellationToken);

        _prefetchedContext.SetTarget(context);
        _prefetchedSegmentIds = new HashSet<int>(segmentIds);
    }

    // The segment itself, from the change tracker when this batch prefetched it. FindAsync looks there first too, but
    // on a miss it falls back to the database - and for a segment the prefetch did not bring back, that question has
    // already been asked and answered for the whole batch. Asking it again is a round trip per event for nothing, and
    // on a replay largely made of removals of segments that are already gone, that is most of the events.
    //
    // Entries rather than Local, because Find resolves a tracked entity whatever its state and Local leaves out the
    // deleted ones: a segment an earlier event in this batch removed has to stay found here, exactly as before.
    private bool IsPrefetched(WmsWfsV2Context context)
        => _prefetchedContext.TryGetTarget(out var prefetched) && ReferenceEquals(context, prefetched);

    private async Task<RoadSegmentRecord?> FindSegment(WmsWfsV2Context context, int segId, CancellationToken cancellationToken)
    {
        if (IsPrefetched(context) && _prefetchedSegmentIds.Contains(segId))
        {
            return context.ChangeTracker
                .Entries<RoadSegmentRecord>()
                .Select(entry => entry.Entity)
                .FirstOrDefault(record => record.WS_OIDN == segId);
        }

        return await context.RoadSegments.FindAsync([segId], cancellationToken);
    }

    // The rows of one segment, from the change tracker when this batch loaded that segment and from the database
    // otherwise.
    private async Task<List<T>> RowsFor<T>(WmsWfsV2Context context, DbSet<T> set, int segId, CancellationToken cancellationToken)
        where T : class, IRoadSegmentScopedRecord
    {
        if (IsPrefetched(context) && _prefetchedSegmentIds.Contains(segId))
        {
            // Local leaves out what this batch removed, which is what the caller means: a row a previous event in the
            // batch deleted is gone. The query below cannot say that - the delete is not saved yet, so it reads the
            // row straight back out of the table and unions it back in.
            return set.Local.Where(record => record.WS_OIDN == segId).ToList();
        }

        // What IncludeLocalToListAsync does, spelled out, because it cannot be used here: it runs one predicate over
        // both the tracked entities and the table, and the two need different ones. Reading WS_OIDN off the interface
        // is fine in memory but is not what the model maps a column to, and EF.Property is the other way round - it
        // only exists inside a query. So each side gets the form it can take, and the union stays as it was.
        var tracked = set.Local.Where(record => record.WS_OIDN == segId).ToList();
        var stored = await set
            .Where(record => EF.Property<int>(record, nameof(IRoadSegmentScopedRecord.WS_OIDN)) == segId)
            .ToListAsync(cancellationToken);

        return stored.Union(tracked).ToList();
    }

    // The dynamic-attribute builders are pure: they turn the event's per-position values into the staging lists that go
    // into the segment's JSON blob. No database access.
    private static List<RoadSegmentMorphologyAttributeRecord> BuildMorphology(RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> values)
        => values.Values.Select(v => new RoadSegmentMorphologyAttributeRecord { MORF = v.Value?.Translation.Identifier, LBLMORF = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentCategoryAttributeRecord> BuildCategory(RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> values)
        => values.Values.Select(v => new RoadSegmentCategoryAttributeRecord { WEGCAT = v.Value?.Translation.Identifier, LBLWEGCAT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentAccessRestrictionAttributeRecord> BuildAccess(RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> values)
        => values.Values.Select(v => new RoadSegmentAccessRestrictionAttributeRecord { TOEGANG = v.Value?.Translation.Identifier, LBLTOEGANG = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentSurfaceTypeAttributeRecord> BuildSurface(RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> values)
        => values.Values.Select(v => new RoadSegmentSurfaceTypeAttributeRecord { VERHARDING = v.Value?.Translation.Identifier, LBLVERHARD = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentStreetNameAttributeRecord> BuildStreetName(RoadSegmentDynamicAttributeValues<StreetNameLocalId> values)
        => values.Values.Select(v => new RoadSegmentStreetNameAttributeRecord { STRTNMID = v.Value.ToInt32(), KANT = v.Side.Translation.Identifier, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentMaintenanceAuthorityAttributeRecord> BuildMaintainer(RoadSegmentDynamicAttributeValues<OrganizationId> values)
        => values.Values.Select(v => new RoadSegmentMaintenanceAuthorityAttributeRecord { BEHEER = v.Value.ToString(), KANT = v.Side.Translation.Identifier, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentCarTrafficDirectionAttributeRecord> BuildCar(RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> values)
        => values.Values.Select(v => new RoadSegmentCarTrafficDirectionAttributeRecord { RICHTING = v.Value?.Translation.Identifier, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentBikeTrafficDirectionAttributeRecord> BuildBike(RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> values)
        => values.Values.Select(v => new RoadSegmentBikeTrafficDirectionAttributeRecord { RICHTING = v.Value?.Translation.Identifier, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static List<RoadSegmentPedestrianTrafficDirectionAttributeRecord> BuildPedestrian(RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> values)
        => values.Values.Select(v => new RoadSegmentPedestrianTrafficDirectionAttributeRecord { RICHTING = v.Value?.Translation.Identifier, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();

    private static async Task RebuildEuropeanRoads(WmsWfsV2Context c, int segId, IReadOnlyCollection<EuropeanRoadNumber> numbers, ProvenanceData provenance, CancellationToken ct)
    {
        {
            c.EuropeanRoads.RemoveRange(await c.EuropeanRoads.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId), ct));
        }
        foreach (var number in numbers)
            c.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = number.ToString(), CREATIE = provenance.Timestamp.ToDateTimeOffset(), VERSIE = provenance.Timestamp.ToDateTimeOffset() });
    }

    private static async Task RebuildNationalRoads(WmsWfsV2Context c, int segId, IReadOnlyCollection<NationalRoadNumber> numbers, ProvenanceData provenance, CancellationToken ct)
    {
        {
            c.NationalRoads.RemoveRange(await c.NationalRoads.IncludeLocalToListAsync(q => q.Where(x => x.WS_OIDN == segId), ct));
        }
        foreach (var number in numbers)
            c.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = number.ToString(), CREATIE = provenance.Timestamp.ToDateTimeOffset(), VERSIE = provenance.Timestamp.ToDateTimeOffset() });
    }

    // V1 add/import: full write of the segment. Both callers (Imported/Added) are created events, but the lookup and
    // the delete queries run all the same, because a replay delivers a created event again. Maps the legacy string values to
    // V2 (null when unmapped) and sets the European/national roads from the event (Imported carries them inline; Added
    // starts empty and gets separate events).
    private Task WriteV1Full(WmsWfsV2Context context, int segId, RoadSegmentGeometry geometry,
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
        return WriteFull(context, segId, geometry, v2Status, v2Method,
            startNode > 0 ? new RoadNodeId(startNode) : null, endNode > 0 ? new RoadNodeId(endNode) : null,
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
    private Task WriteV1Modified(WmsWfsV2Context context, int segId, RoadSegmentGeometry geometry,
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
            new RoadSegmentNodeIds { Start = startNode > 0 ? new RoadNodeId(startNode) : null, End = endNode > 0 ? new RoadNodeId(endNode) : null },
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

    // The current per-side street name ids (read from the segment's stored dynamic attributes), used by the partial V1
    // events to keep the side they do not change.
    private static (int? Left, int? Right) CurrentStreetNameSides(IReadOnlyList<RoadSegmentStreetNameAttributeRecord> rows)
    {
        var left = rows.FirstOrDefault(x => x.KANT == RoadSegmentAttributeSide.Beide.Translation.Identifier || x.KANT == RoadSegmentAttributeSide.Links.Translation.Identifier);
        var right = rows.FirstOrDefault(x => x.KANT == RoadSegmentAttributeSide.Beide.Translation.Identifier || x.KANT == RoadSegmentAttributeSide.Rechts.Translation.Identifier);
        return (left?.STRTNMID, right?.STRTNMID);
    }
}
