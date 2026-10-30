namespace RoadRegistry.Pbs.Projections;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using RoadRegistry.BackOffice.Extensions;

public class RoadSegmentPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>, IBatchPrefetchingRoadNetworkChangesProjection<PbsContext>
{
    // The segments this batch loaded in full, and the context they were loaded into. Both halves matter: the ids say
    // whose rows are in the change tracker, and the context says which tracker, because a driver that never calls the
    // prefetch - or one that cleared the tracker since - must not be told the rows are already there. Getting that
    // wrong does not read stale data, it reads nothing, and a missing attribute row is a silently wrong read model.
    private PbsContext? _prefetchedContext;
    private HashSet<int> _prefetchedSegmentIds = [];

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
            var record = await context.RoadSegments.FindAsync([e.Data.RoadSegmentId], ct);
            if (record?.GEOMETRIE is null)
            {
                return;
            }
            var (currentLeft, currentRight) = await CurrentStreetNameSidesFromV1(context, e.Data.RoadSegmentId, ct);
            var streetName = StreetNameFromV1(e.Data.LeftSideStreetNameId ?? currentLeft, e.Data.RightSideStreetNameId ?? currentRight, record.GEOMETRIE.Length);
            await WritePartial(context, e.Data.RoadSegmentId, null, null, null, null, null, null, null, null,
                streetName, null, null, null, null, e.Data.Provenance, ct);
        });

        When<IEvent<RoadSegmentRemoved>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId, ct));
        When<IEvent<OutlinedRoadSegmentRemoved>>((context, e, ct) => Remove(context, e.Data.RoadSegmentId, ct));

        When<IEvent<RoadSegmentAddedToEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = EuropeanRoadNumber.Parse(e.Data.Number).ToString();
            if (!(await RowsFor(context, context.EuropeanRoads, segId, ct)).Any(x => x.EUNUMMER == nummer))
            {
                context.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentRemovedFromEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = EuropeanRoadNumber.Parse(e.Data.Number).ToString();
            var rows = (await RowsFor(context, context.EuropeanRoads, segId, ct)).Where(x => x.EUNUMMER == nummer).ToList();
            context.EuropeanRoads.RemoveRange(rows);
        });
        When<IEvent<RoadSegmentAddedToNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = NationalRoadNumber.Parse(e.Data.Number).ToString();
            if (!(await RowsFor(context, context.NationalRoads, segId, ct)).Any(x => x.NWNUMMER == nummer))
            {
                context.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentRemovedFromNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId;
            var nummer = NationalRoadNumber.Parse(e.Data.Number).ToString();
            var rows = (await RowsFor(context, context.NationalRoads, segId, ct)).Where(x => x.NWNUMMER == nummer).ToList();
            context.NationalRoads.RemoveRange(rows);
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

        When<IEvent<RoadSegmentWasModified>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), m.Geometry, m.Status, m.GeometryDrawMethod,
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

        When<IEvent<RoadSegmentGeometryWasModified>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), m.Geometry, null, null,
                new RoadSegmentNodeIds { Start = m.StartNodeId, End = m.EndNodeId },
                null, null, null, null, null, null, null, null, null, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentStreetNameIdWasChanged>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), null, null, null, null, null, null, null, null,
                m.StreetNameId, null, null, null, null, m.Provenance, ct);
        });

        When<IEvent<RoadSegmentGeometryDrawMethodWasChanged>>(async (context, e, ct) =>
        {
            var m = e.Data;
            await WritePartial(context, m.RoadSegmentId.ToInt32(), null, null, m.GeometryDrawMethod, null, null, null,
                null, null, null, null, null, null, null, m.Provenance, ct);
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
            if (!(await RowsFor(context, context.EuropeanRoads, segId, ct)).Any(x => x.EUNUMMER == nummer))
            {
                context.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentWasRemovedFromEuropeanRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            var rows = (await RowsFor(context, context.EuropeanRoads, segId, ct)).Where(x => x.EUNUMMER == nummer).ToList();
            context.EuropeanRoads.RemoveRange(rows);
        });
        When<IEvent<RoadSegmentWasAddedToNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            if (!(await RowsFor(context, context.NationalRoads, segId, ct)).Any(x => x.NWNUMMER == nummer))
            {
                context.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = nummer, CREATIE = e.Data.Provenance.ToPbsDate(), VERSIE = e.Data.Provenance.ToPbsDate() });
            }
        });
        When<IEvent<RoadSegmentWasRemovedFromNationalRoad>>(async (context, e, ct) =>
        {
            var segId = e.Data.RoadSegmentId.ToInt32();
            var nummer = e.Data.Number.ToString();
            var rows = (await RowsFor(context, context.NationalRoads, segId, ct)).Where(x => x.NWNUMMER == nummer).ToList();
            context.NationalRoads.RemoveRange(rows);
        });
    }

    // One query per table for the whole batch instead of one per event. Every table under Wegsegmenten is read by
    // WS_OIDN and gives back a handful of rows, so against a database a network away the cost of those reads is almost
    // entirely the round trip: a hundred events cost a hundred round trips per table to fetch a few hundred bytes each.
    //
    // What makes this sound rather than a cache is that it loads whole segments. Once every row of a segment is in the
    // change tracker, "the rows of this segment" can be answered from it - including the ones earlier events in the
    // same batch added or removed, which is more than the per-event query could say. A segment whose id cannot be read
    // from its event is left out and its handlers query for it as before.
    public async Task PrefetchBatchAsync(PbsContext context, IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
    {
        // Dropped first: a prefetch that fails halfway must not leave behind a claim that the rows are loaded.
        _prefetchedContext = null;
        _prefetchedSegmentIds = [];

        var segmentIds = events
            .Select(SegmentIdOf)
            .Where(segmentId => segmentId is not null)
            .Select(segmentId => segmentId!.Value)
            .Distinct()
            .ToArray();
        if (segmentIds.Length == 0)
        {
            return;
        }

        await context.RoadSegments.Where(record => segmentIds.Contains(record.WS_OIDN)).LoadAsync(cancellationToken);

        await PrefetchRowsAsync(context.DerivedRoadSegments, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentMorphologyAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentCategoryAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentAccessRestrictionAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentSurfaceTypeAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentStreetNameAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentMaintenanceAuthorityAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentCarTrafficDirectionAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentBikeTrafficDirectionAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.RoadSegmentPedestrianTrafficDirectionAttributes, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.EuropeanRoads, segmentIds, cancellationToken);
        await PrefetchRowsAsync(context.NationalRoads, segmentIds, cancellationToken);

        _prefetchedContext = context;
        _prefetchedSegmentIds = new HashSet<int>(segmentIds);
    }

    private static Task PrefetchRowsAsync<T>(DbSet<T> set, int[] segmentIds, CancellationToken cancellationToken)
        where T : class, IRoadSegmentScopedRecord
    {
        // EF.Property rather than record.WS_OIDN: under the generic constraint that member access resolves against the
        // interface, and an interface property is not what the model maps a column to.
        return set
            .Where(record => segmentIds.Contains(EF.Property<int>(record, nameof(IRoadSegmentScopedRecord.WS_OIDN))))
            .LoadAsync(cancellationToken);
    }

    // The rows of one segment, from the change tracker when this batch loaded that segment and from the database
    // otherwise. Both halves of the guard are load-bearing: the ids say whose rows were loaded, and the context says
    // into which tracker, since nothing stops a driver from skipping the prefetch or clearing the tracker in between.
    private async Task<List<T>> RowsFor<T>(PbsContext context, DbSet<T> set, int segId, CancellationToken cancellationToken)
        where T : class, IRoadSegmentScopedRecord
    {
        if (ReferenceEquals(context, _prefetchedContext) && _prefetchedSegmentIds.Contains(segId))
        {
            // Local leaves out what this batch removed, which is what the caller means: a row a previous event in the
            // batch deleted is gone. The query below cannot say that - the delete is not saved yet, so it reads the
            // row straight back out of the table and unions it back in.
            return set.Local.Where(record => record.WS_OIDN == segId).ToList();
        }

        return await set.IncludeLocalToListAsync(
            q => q.Where(record => EF.Property<int>(record, nameof(IRoadSegmentScopedRecord.WS_OIDN)) == segId),
            cancellationToken);
    }

    // The road segment an event is about, read by property name rather than declared per event type. A table mapping
    // every handled event to its id would be a second list to remember when an event is added, and forgetting it would
    // cost throughput while saying nothing - so the name is the contract, and PbsProjectionEventCoverage holds it.
    // Not finding one is not an error: that segment is left out of the prefetch and read per event as before.
    private static readonly ConcurrentDictionary<Type, Func<object, int?>?> SegmentIdAccessors = new();

    private static int? SegmentIdOf(IEvent @event)
    {
        var data = @event.Data;
        return data is null ? null : SegmentIdAccessors.GetOrAdd(data.GetType(), BuildSegmentIdAccessor)?.Invoke(data);
    }

    // Internal so the coverage test can ask, of every event this projection handles, whether its id can be read -
    // which is the only thing standing between a new event and a silent return to one round trip per lookup.
    internal static Func<object, int?>? BuildSegmentIdAccessor(Type eventType)
    {
        var property = eventType.GetProperty("RoadSegmentId", BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            return null;
        }

        // V1 carries the id as a plain int, V2 as a value object.
        if (property.PropertyType == typeof(int))
        {
            return data => (int?)property.GetValue(data);
        }

        var toInt32 = property.PropertyType.GetMethod("ToInt32", Type.EmptyTypes);
        if (toInt32 is null || toInt32.ReturnType != typeof(int))
        {
            return null;
        }

        return data =>
        {
            var value = property.GetValue(data);
            return value is null ? null : (int?)toInt32.Invoke(value, null);
        };
    }

    private async Task WriteFull(PbsContext context, int segId,
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
        // left the previous derived rows and attribute rows behind.
        var (normal, isNew) = await LoadOrCreate(context, segId, provenance);

        normal.GEOMETRIE = geometry.EnsureLambert08().RoundToCm().Value.Force2D();
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

        var morphologyRows = await RebuildMorphology(context, segId, morphology, ct);
        var categoryRows = await RebuildCategory(context, segId, category, ct);
        var accessRows = await RebuildAccess(context, segId, access, ct);
        var surfaceRows = await RebuildSurface(context, segId, surface, ct);
        var streetNameRows = await RebuildStreetName(context, segId, streetName, ct);
        var maintainerRows = await RebuildMaintainer(context, segId, maintainer, ct);
        var carRows = await RebuildCar(context, segId, car, ct);
        var bikeRows = await RebuildBike(context, segId, bike, ct);
        var pedestrianRows = await RebuildPedestrian(context, segId, pedestrian, ct);

        await RebuildEuropeanRoads(context, segId, europeanRoadNumbers, provenance, ct);
        await RebuildNationalRoads(context, segId, nationalRoadNumbers, provenance, ct);

        await RebuildDerived(context, segId, normal, morphologyRows, categoryRows, accessRows, surfaceRows, streetNameRows, maintainerRows, carRows, bikeRows, pedestrianRows, ct);
    }

    // Partial update: only the non-null arguments are applied to the segment; the rest are kept as stored. Used for
    // modify / geometry-modified / streetname-changed / split, which each carry only a subset of the attributes.
    // Knotted into the network: the event records the realized state in full. The geometry draw method is not
    // touched by a status change.
    private Task ProjectConnected(PbsContext context, IRoadSegmentWasConnectedEvent m, CancellationToken ct)
    {
        return WritePartial(context, m.RoadSegmentId.ToInt32(), m.Geometry, RoadSegmentStatusChange.ForEvent(m).To, null,
            new RoadSegmentNodeIds { Start = m.StartNodeId, End = m.EndNodeId },
            m.Morphology, m.Category, m.AccessRestriction, m.SurfaceType,
            m.StreetNameId, m.MaintenanceAuthorityId, m.CarTrafficDirection, m.BikeTrafficDirection,
            m.PedestrianTrafficDirection, m.Provenance, ct);
    }

    // Unhooked from the network: only the status changes; the geometry and the attributes are untouched. The segment
    // came loose from its road nodes, so the node state is carried with both ids null.
    private Task ProjectDisconnected(PbsContext context, IRoadSegmentWasDisconnectedEvent m, CancellationToken ct)
    {
        return WritePartial(context, m.RoadSegmentId.ToInt32(), null, RoadSegmentStatusChange.ForEvent(m).To, null,
            new RoadSegmentNodeIds(), null, null, null, null, null, null, null, null, null, m.Provenance, ct);
    }

    // Outside the network before and after, so nothing but the status moves - the road nodes are left alone.
    private Task ProjectUnconnectedStatusChange(PbsContext context, IRoadSegmentUnconnectedStatusChangeEvent m, CancellationToken ct)
    {
        return WritePartial(context, m.RoadSegmentId.ToInt32(), null, RoadSegmentStatusChange.ForEvent(m).To, null,
            null, null, null, null, null, null, null, null, null, null, m.Provenance, ct);
    }

    private async Task WritePartial(PbsContext context, int segId,
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
        var (normal, isNew) = await LoadOrCreate(context, segId, provenance);

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
        normal.VERSIE = provenance.ToPbsDate();
        if (isNew)
        {
            context.RoadSegments.Add(normal);
        }

        // Rebuild the attributes this event changed; keep the rest as stored. The flattener needs the full current set,
        // so unchanged attributes are loaded back from the database.
        var morphologyRows = morphology is not null ? await RebuildMorphology(context, segId, morphology, ct) : await RowsFor(context, context.RoadSegmentMorphologyAttributes, segId, ct);
        var categoryRows = category is not null ? await RebuildCategory(context, segId, category, ct) : await RowsFor(context, context.RoadSegmentCategoryAttributes, segId, ct);
        var accessRows = access is not null ? await RebuildAccess(context, segId, access, ct) : await RowsFor(context, context.RoadSegmentAccessRestrictionAttributes, segId, ct);
        var surfaceRows = surface is not null ? await RebuildSurface(context, segId, surface, ct) : await RowsFor(context, context.RoadSegmentSurfaceTypeAttributes, segId, ct);
        var streetNameRows = streetName is not null ? await RebuildStreetName(context, segId, streetName, ct) : await RowsFor(context, context.RoadSegmentStreetNameAttributes, segId, ct);
        var maintainerRows = maintainer is not null ? await RebuildMaintainer(context, segId, maintainer, ct) : await RowsFor(context, context.RoadSegmentMaintenanceAuthorityAttributes, segId, ct);
        var carRows = car is not null ? await RebuildCar(context, segId, car, ct) : await RowsFor(context, context.RoadSegmentCarTrafficDirectionAttributes, segId, ct);
        var bikeRows = bike is not null ? await RebuildBike(context, segId, bike, ct) : await RowsFor(context, context.RoadSegmentBikeTrafficDirectionAttributes, segId, ct);
        var pedestrianRows = pedestrian is not null ? await RebuildPedestrian(context, segId, pedestrian, ct) : await RowsFor(context, context.RoadSegmentPedestrianTrafficDirectionAttributes, segId, ct);

        await RebuildDerived(context, segId, normal, morphologyRows, categoryRows, accessRows, surfaceRows, streetNameRows, maintainerRows, carRows, bikeRows, pedestrianRows, ct);
    }

    private static async Task<(RoadSegmentRecord Normal, bool IsNew)> LoadOrCreate(PbsContext context, int segId, ProvenanceData provenance)
    {
        var normal = await context.RoadSegments.FindAsync(segId);
        var isNew = normal is null;
        normal ??= new RoadSegmentRecord { WS_OIDN = segId, CREATIE = provenance.ToPbsDate() };
        return (normal, isNew);
    }

    private async Task RebuildDerived(PbsContext context, int segId, RoadSegmentRecord normal,
        IReadOnlyList<RoadSegmentMorphologyAttributeRecord> morphology,
        IReadOnlyList<RoadSegmentCategoryAttributeRecord> category,
        IReadOnlyList<RoadSegmentAccessRestrictionAttributeRecord> access,
        IReadOnlyList<RoadSegmentSurfaceTypeAttributeRecord> surface,
        IReadOnlyList<RoadSegmentStreetNameAttributeRecord> streetName,
        IReadOnlyList<RoadSegmentMaintenanceAuthorityAttributeRecord> maintainer,
        IReadOnlyList<RoadSegmentCarTrafficDirectionAttributeRecord> car,
        IReadOnlyList<RoadSegmentBikeTrafficDirectionAttributeRecord> bike,
        IReadOnlyList<RoadSegmentPedestrianTrafficDirectionAttributeRecord> pedestrian,
        CancellationToken ct)
    {
        {
            context.DerivedRoadSegments.RemoveRange(await RowsFor(context, context.DerivedRoadSegments, segId, ct));
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

    private async Task Remove(PbsContext context, int segId, CancellationToken ct)
    {
        var normal = await context.RoadSegments.FindAsync([segId], ct);
        if (normal is not null)
        {
            context.RoadSegments.Remove(normal);
        }
        context.DerivedRoadSegments.RemoveRange(await RowsFor(context, context.DerivedRoadSegments, segId, ct));

        context.RoadSegmentMorphologyAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentMorphologyAttributes, segId, ct));
        context.RoadSegmentStreetNameAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentStreetNameAttributes, segId, ct));
        context.RoadSegmentAccessRestrictionAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentAccessRestrictionAttributes, segId, ct));
        context.RoadSegmentCarTrafficDirectionAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentCarTrafficDirectionAttributes, segId, ct));
        context.RoadSegmentBikeTrafficDirectionAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentBikeTrafficDirectionAttributes, segId, ct));
        context.RoadSegmentPedestrianTrafficDirectionAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentPedestrianTrafficDirectionAttributes, segId, ct));
        context.RoadSegmentMaintenanceAuthorityAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentMaintenanceAuthorityAttributes, segId, ct));
        context.RoadSegmentCategoryAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentCategoryAttributes, segId, ct));
        context.RoadSegmentSurfaceTypeAttributes.RemoveRange(await RowsFor(context, context.RoadSegmentSurfaceTypeAttributes, segId, ct));
        context.EuropeanRoads.RemoveRange(await RowsFor(context, context.EuropeanRoads, segId, ct));
        context.NationalRoads.RemoveRange(await RowsFor(context, context.NationalRoads, segId, ct));
    }

    private async Task<List<RoadSegmentMorphologyAttributeRecord>> RebuildMorphology(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> values, CancellationToken ct)
    {
            c.RoadSegmentMorphologyAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentMorphologyAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentMorphologyAttributeRecord { WS_OIDN = segId, MORF = v.Value?.Translation.Identifier, LBLMORF = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentMorphologyAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentCategoryAttributeRecord>> RebuildCategory(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> values, CancellationToken ct)
    {
            c.RoadSegmentCategoryAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentCategoryAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentCategoryAttributeRecord { WS_OIDN = segId, WEGCAT = v.Value?.Translation.Identifier, LBLWEGCAT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentCategoryAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentAccessRestrictionAttributeRecord>> RebuildAccess(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> values, CancellationToken ct)
    {
            c.RoadSegmentAccessRestrictionAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentAccessRestrictionAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentAccessRestrictionAttributeRecord { WS_OIDN = segId, TOEGANG = v.Value?.Translation.Identifier, LBLTOEGANG = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentAccessRestrictionAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentSurfaceTypeAttributeRecord>> RebuildSurface(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> values, CancellationToken ct)
    {
            c.RoadSegmentSurfaceTypeAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentSurfaceTypeAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentSurfaceTypeAttributeRecord { WS_OIDN = segId, VERHARDING = v.Value?.Translation.Identifier, LBLVERHARD = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentSurfaceTypeAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentStreetNameAttributeRecord>> RebuildStreetName(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<StreetNameLocalId> values, CancellationToken ct)
    {
            c.RoadSegmentStreetNameAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentStreetNameAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentStreetNameAttributeRecord { WS_OIDN = segId, STRTNMID = v.Value.ToInt32(), KANT = v.Side.Translation.Identifier, LBLKANT = v.Side.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentStreetNameAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentMaintenanceAuthorityAttributeRecord>> RebuildMaintainer(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<OrganizationId> values, CancellationToken ct)
    {
            c.RoadSegmentMaintenanceAuthorityAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentMaintenanceAuthorityAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentMaintenanceAuthorityAttributeRecord { WS_OIDN = segId, BEHEER = v.Value.ToString(), KANT = v.Side.Translation.Identifier, LBLKANT = v.Side.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentMaintenanceAuthorityAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentCarTrafficDirectionAttributeRecord>> RebuildCar(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> values, CancellationToken ct)
    {
            c.RoadSegmentCarTrafficDirectionAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentCarTrafficDirectionAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentCarTrafficDirectionAttributeRecord { WS_OIDN = segId, RICHTING = v.Value?.Translation.Identifier, LBLRICHT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentCarTrafficDirectionAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentBikeTrafficDirectionAttributeRecord>> RebuildBike(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> values, CancellationToken ct)
    {
            c.RoadSegmentBikeTrafficDirectionAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentBikeTrafficDirectionAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentBikeTrafficDirectionAttributeRecord { WS_OIDN = segId, RICHTING = v.Value?.Translation.Identifier, LBLRICHT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentBikeTrafficDirectionAttributes.AddRange(rows);
        return rows;
    }

    private async Task<List<RoadSegmentPedestrianTrafficDirectionAttributeRecord>> RebuildPedestrian(PbsContext c, int segId, RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> values, CancellationToken ct)
    {
            c.RoadSegmentPedestrianTrafficDirectionAttributes.RemoveRange(await RowsFor(c, c.RoadSegmentPedestrianTrafficDirectionAttributes, segId, ct));
        var rows = values.Values.Select(v => new RoadSegmentPedestrianTrafficDirectionAttributeRecord { WS_OIDN = segId, RICHTING = v.Value?.Translation.Identifier, LBLRICHT = v.Value?.Translation.Name, VANPOS = v.Coverage.From.ToDouble(), TOTPOS = v.Coverage.To.ToDouble() }).ToList();
        c.RoadSegmentPedestrianTrafficDirectionAttributes.AddRange(rows);
        return rows;
    }

    private async Task RebuildEuropeanRoads(PbsContext c, int segId, IReadOnlyCollection<EuropeanRoadNumber> numbers, ProvenanceData provenance, CancellationToken ct)
    {
            c.EuropeanRoads.RemoveRange(await RowsFor(c, c.EuropeanRoads, segId, ct));
        foreach (var number in numbers)
            c.EuropeanRoads.Add(new EuropeanRoadRecord { WS_OIDN = segId, EUNUMMER = number.ToString(), CREATIE = provenance.ToPbsDate(), VERSIE = provenance.ToPbsDate() });
    }

    private async Task RebuildNationalRoads(PbsContext c, int segId, IReadOnlyCollection<NationalRoadNumber> numbers, ProvenanceData provenance, CancellationToken ct)
    {
            c.NationalRoads.RemoveRange(await RowsFor(c, c.NationalRoads, segId, ct));
        foreach (var number in numbers)
            c.NationalRoads.Add(new NationalRoadRecord { WS_OIDN = segId, NWNUMMER = number.ToString(), CREATIE = provenance.ToPbsDate(), VERSIE = provenance.ToPbsDate() });
    }

    // V1 add/import: full write of the segment. Both callers (Imported/Added) are created events, so the segment cannot
    // exist yet but the lookup and the delete queries run all the same, because a replay delivers a created event again. Maps the legacy string values
    // to V2 (null when unmapped) and sets the European/national roads from the event (Imported carries them inline; Added
    // starts empty and gets separate events).
    private Task WriteV1Full(PbsContext context, int segId, RoadSegmentGeometry geometry,
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
    private Task WriteV1Modified(PbsContext context, int segId, RoadSegmentGeometry geometry,
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

    // The current per-side street name ids, used by the partial V1 events to keep the side they do not change.
    private async Task<(int? Left, int? Right)> CurrentStreetNameSidesFromV1(PbsContext context, int segId, CancellationToken ct)
    {
        var rows = await RowsFor(context, context.RoadSegmentStreetNameAttributes, segId, ct);
        var left = rows.FirstOrDefault(x => x.KANT == RoadSegmentAttributeSide.Beide.Translation.Identifier || x.KANT == RoadSegmentAttributeSide.Links.Translation.Identifier);
        var right = rows.FirstOrDefault(x => x.KANT == RoadSegmentAttributeSide.Beide.Translation.Identifier || x.KANT == RoadSegmentAttributeSide.Rechts.Translation.Identifier);
        return (left?.STRTNMID, right?.STRTNMID);
    }
}
