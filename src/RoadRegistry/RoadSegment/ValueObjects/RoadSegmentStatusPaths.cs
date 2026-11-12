namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.ValueObjects;

// How a road segment gets from one status to any other, as a series of status changes (hops).
//
// Not every pair of statuses has a status change of its own, so a change of status that is not a single action - an
// upload that sets another status, an upload that leaves a road segment out and so historizes it - walks one of the
// paths below. 'gerealiseerd' is the only status that knots a segment into the road network, so a path only ever
// passes through it as its first or last status: going through it on the way between two statuses outside the
// network would hook the segment up and unhook it again, with the road nodes and crossings that come with that.
//
// The paths are spelled out rather than worked out, so that changing one is changing one line here.
// RoadSegmentStatusPathsTests pins every one of them down.
public static class RoadSegmentStatusPaths
{
    private static readonly RoadSegmentStatusV2 Gepland = RoadSegmentStatusV2.Gepland;
    private static readonly RoadSegmentStatusV2 Gerealiseerd = RoadSegmentStatusV2.Gerealiseerd;
    private static readonly RoadSegmentStatusV2 NietGerealiseerd = RoadSegmentStatusV2.NietGerealiseerd;
    private static readonly RoadSegmentStatusV2 BuitenGebruik = RoadSegmentStatusV2.BuitenGebruik;
    private static readonly RoadSegmentStatusV2 Gehistoreerd = RoadSegmentStatusV2.Gehistoreerd;

    private static readonly IReadOnlyDictionary<(RoadSegmentStatusV2 From, RoadSegmentStatusV2 To), RoadSegmentStatusChange[]> Paths =
        new Dictionary<(RoadSegmentStatusV2, RoadSegmentStatusV2), RoadSegmentStatusChange[]>
        {
            [(Gepland, Gerealiseerd)] = [RoadSegmentStatusChange.PlannedToRealized],
            [(Gepland, NietGerealiseerd)] = [RoadSegmentStatusChange.PlannedToNotRealized],
            [(Gepland, BuitenGebruik)] = [RoadSegmentStatusChange.PlannedToHistorized, RoadSegmentStatusChange.HistorizedToOutOfUse],
            [(Gepland, Gehistoreerd)] = [RoadSegmentStatusChange.PlannedToHistorized],

            [(Gerealiseerd, Gepland)] = [RoadSegmentStatusChange.RealizedToPlanned],
            [(Gerealiseerd, NietGerealiseerd)] = [RoadSegmentStatusChange.RealizedToPlanned, RoadSegmentStatusChange.PlannedToNotRealized],
            [(Gerealiseerd, BuitenGebruik)] = [RoadSegmentStatusChange.RealizedToOutOfUse],
            [(Gerealiseerd, Gehistoreerd)] = [RoadSegmentStatusChange.RealizedToHistorized],

            [(NietGerealiseerd, Gepland)] = [RoadSegmentStatusChange.NotRealizedToPlanned],
            [(NietGerealiseerd, Gerealiseerd)] = [RoadSegmentStatusChange.NotRealizedToPlanned, RoadSegmentStatusChange.PlannedToRealized],
            [(NietGerealiseerd, BuitenGebruik)] = [RoadSegmentStatusChange.NotRealizedToHistorized, RoadSegmentStatusChange.HistorizedToOutOfUse],
            [(NietGerealiseerd, Gehistoreerd)] = [RoadSegmentStatusChange.NotRealizedToHistorized],

            [(BuitenGebruik, Gepland)] = [RoadSegmentStatusChange.OutOfUseToHistorized, RoadSegmentStatusChange.HistorizedToPlanned],
            [(BuitenGebruik, Gerealiseerd)] = [RoadSegmentStatusChange.OutOfUseToRealized],
            [(BuitenGebruik, NietGerealiseerd)] = [RoadSegmentStatusChange.OutOfUseToHistorized, RoadSegmentStatusChange.HistorizedToPlanned, RoadSegmentStatusChange.PlannedToNotRealized],
            [(BuitenGebruik, Gehistoreerd)] = [RoadSegmentStatusChange.OutOfUseToHistorized],

            [(Gehistoreerd, Gepland)] = [RoadSegmentStatusChange.HistorizedToPlanned],
            [(Gehistoreerd, Gerealiseerd)] = [RoadSegmentStatusChange.HistorizedToRealized],
            [(Gehistoreerd, NietGerealiseerd)] = [RoadSegmentStatusChange.HistorizedToPlanned, RoadSegmentStatusChange.PlannedToNotRealized],
            [(Gehistoreerd, BuitenGebruik)] = [RoadSegmentStatusChange.HistorizedToOutOfUse],
        };

    // The status changes that take a road segment from one status to the other, in order. Empty when the two are the
    // same: there is nothing to change.
    public static IReadOnlyList<RoadSegmentStatusChange> Get(RoadSegmentStatusV2 from, RoadSegmentStatusV2 to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (from == to)
        {
            return [];
        }

        return Paths.TryGetValue((from, to), out var path)
            ? path
            : throw new InvalidOperationException($"There is no status path from {from} to {to}.");
    }

    public static IEnumerable<(RoadSegmentStatusV2 From, RoadSegmentStatusV2 To, IReadOnlyList<RoadSegmentStatusChange> Path)> All
        => Paths.Select(x => (x.Key.From, x.Key.To, (IReadOnlyList<RoadSegmentStatusChange>)x.Value));
}
