namespace RoadRegistry.Tests.ValueObjectTests;

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;
using Xunit;

// Pins down every status path. A path is a choice, not a consequence, so changing one has to be changing it here too.
public class RoadSegmentStatusPathsTests
{
    public static IEnumerable<object[]> ExpectedPaths()
    {
        // From Gepland
        yield return Path("Gepland", "Gerealiseerd", RoadSegmentStatusChange.PlannedToRealized);
        yield return Path("Gepland", "NietGerealiseerd", RoadSegmentStatusChange.PlannedToNotRealized);
        yield return Path("Gepland", "BuitenGebruik", RoadSegmentStatusChange.PlannedToHistorized, RoadSegmentStatusChange.HistorizedToOutOfUse);
        yield return Path("Gepland", "Gehistoreerd", RoadSegmentStatusChange.PlannedToHistorized);

        // From Gerealiseerd
        yield return Path("Gerealiseerd", "Gepland", RoadSegmentStatusChange.RealizedToPlanned);
        yield return Path("Gerealiseerd", "NietGerealiseerd", RoadSegmentStatusChange.RealizedToPlanned, RoadSegmentStatusChange.PlannedToNotRealized);
        yield return Path("Gerealiseerd", "BuitenGebruik", RoadSegmentStatusChange.RealizedToOutOfUse);
        yield return Path("Gerealiseerd", "Gehistoreerd", RoadSegmentStatusChange.RealizedToHistorized);

        // From NietGerealiseerd
        yield return Path("NietGerealiseerd", "Gepland", RoadSegmentStatusChange.NotRealizedToPlanned);
        yield return Path("NietGerealiseerd", "Gerealiseerd", RoadSegmentStatusChange.NotRealizedToPlanned, RoadSegmentStatusChange.PlannedToRealized);
        yield return Path("NietGerealiseerd", "BuitenGebruik", RoadSegmentStatusChange.NotRealizedToHistorized, RoadSegmentStatusChange.HistorizedToOutOfUse);
        yield return Path("NietGerealiseerd", "Gehistoreerd", RoadSegmentStatusChange.NotRealizedToHistorized);

        // From BuitenGebruik
        yield return Path("BuitenGebruik", "Gepland", RoadSegmentStatusChange.OutOfUseToHistorized, RoadSegmentStatusChange.HistorizedToPlanned);
        yield return Path("BuitenGebruik", "Gerealiseerd", RoadSegmentStatusChange.OutOfUseToRealized);
        yield return Path("BuitenGebruik", "NietGerealiseerd", RoadSegmentStatusChange.OutOfUseToHistorized, RoadSegmentStatusChange.HistorizedToPlanned, RoadSegmentStatusChange.PlannedToNotRealized);
        yield return Path("BuitenGebruik", "Gehistoreerd", RoadSegmentStatusChange.OutOfUseToHistorized);

        // From Gehistoreerd
        yield return Path("Gehistoreerd", "Gepland", RoadSegmentStatusChange.HistorizedToPlanned);
        yield return Path("Gehistoreerd", "Gerealiseerd", RoadSegmentStatusChange.HistorizedToRealized);
        yield return Path("Gehistoreerd", "NietGerealiseerd", RoadSegmentStatusChange.HistorizedToPlanned, RoadSegmentStatusChange.PlannedToNotRealized);
        yield return Path("Gehistoreerd", "BuitenGebruik", RoadSegmentStatusChange.HistorizedToOutOfUse);
    }

    private static object[] Path(string from, string to, params RoadSegmentStatusChange[] path)
        => [from, to, path.Select(x => x.Name).ToArray()];

    [Theory]
    [MemberData(nameof(ExpectedPaths))]
    public void ThePathIsTheAgreedOne(string from, string to, string[] expectedPath)
    {
        var path = RoadSegmentStatusPaths.Get(RoadSegmentStatusV2.Parse(from), RoadSegmentStatusV2.Parse(to));

        path.Select(x => x.Name).Should().Equal(expectedPath);
    }

    [Fact]
    public void EveryPairOfDifferentStatusesHasAPath()
    {
        var pairs = RoadSegmentStatusV2.All
            .SelectMany(from => RoadSegmentStatusV2.All.Where(to => to != from).Select(to => (from, to)))
            .ToArray();

        RoadSegmentStatusPaths.All.Select(x => (x.From, x.To)).Should().BeEquivalentTo(pairs);
        ExpectedPaths().Should().HaveCount(pairs.Length);
    }

    [Fact]
    public void EveryPathIsConnectedAndEndsWhereItShould()
    {
        foreach (var (from, to, path) in RoadSegmentStatusPaths.All)
        {
            path.Should().NotBeEmpty();
            path[0].From.Should().Be(from, $"the path from {from} to {to} starts there");
            path[^1].To.Should().Be(to, $"the path from {from} to {to} ends there");

            for (var i = 1; i < path.Count; i++)
            {
                path[i].From.Should().Be(path[i - 1].To, $"the path from {from} to {to} has no gaps");
            }
        }
    }

    [Fact]
    public void APathOnlyPassesThroughGerealiseerdAsItsFirstOrLastStatus()
    {
        // Passing through 'gerealiseerd' would knot the segment into the network on the way between two statuses
        // outside it.
        foreach (var (from, to, path) in RoadSegmentStatusPaths.All)
        {
            path.Take(path.Count - 1)
                .Select(x => x.To)
                .Should().NotContain(RoadSegmentStatusV2.Gerealiseerd, $"the path from {from} to {to} stays outside the network in between");
        }
    }

    [Fact]
    public void TheSameStatusHasAnEmptyPath()
    {
        foreach (var status in RoadSegmentStatusV2.All)
        {
            RoadSegmentStatusPaths.Get(status, status).Should().BeEmpty();
        }
    }

    [Fact]
    public void EveryStatusHasADirectStatusChangeToGehistoreerd()
    {
        foreach (var status in RoadSegmentStatusV2.All.Where(x => x != RoadSegmentStatusV2.Gehistoreerd))
        {
            RoadSegmentStatusPaths.Get(status, RoadSegmentStatusV2.Gehistoreerd).Should().ContainSingle();
        }
    }
}
