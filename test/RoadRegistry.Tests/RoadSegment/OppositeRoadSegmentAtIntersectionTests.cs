namespace RoadRegistry.Tests.RoadSegment;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using Xunit;
using Xunit.Abstractions;

// POC: given the four road segments that meet in one node, find the one opposite a given segment.
//
// The rule is purely positional, whatever the angles: order the legs around the node and take the one that is neither
// the clockwise nor the counter-clockwise neighbour. Exactly one leg qualifies, which is why this needs a four-way
// node - with three every other leg is a neighbour, with five there are two non-neighbours.
//
// Drawing a circle around the intersection and walking its border computes precisely that ordering, and "skip one,
// take the next" is "not a neighbour". So the ordering can be taken directly from the bearing of each leg as it
// leaves the node: no buffer, no boundary intersection, no radius to tune, and nothing to go wrong when a leg is
// shorter than the radius or crosses the circle more than once.
public class OppositeRoadSegmentAtIntersectionTests
{
    // How far along the leg the direction is measured, i.e. how the leg "leaves" the node.
    //
    // It is the minimum distance between two vertices, which makes it the shortest probe that is guaranteed to land
    // on real geometry rather than between two coincident points - and, because a leg's turn can begin within the
    // first metre, the largest one that still measures the departure direction instead of where the leg has already
    // turned to. The cost is angular noise: coordinates are stored to the centimetre, so one centimetre of jitter on
    // the first vertex rotates the bearing by atan(0.01 / 0.15) = 3.8 degrees (see ProbeDistance_TradeOff below).
    // Legs that leave the node less than ~4 degrees apart therefore cannot be ordered reliably.
    private static readonly double ProbeDistance = Distances.MinimumDistanceBetweenVertices;
    private const double NodeTolerance = 0.01;

    private readonly ITestOutputHelper _output;

    public OppositeRoadSegmentAtIntersectionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private sealed record Leg(string Id, LineString Geometry);

    // The one leg that is not a neighbour of the given leg in the angular order around the node.
    private static string Opposite(IReadOnlyList<Leg> legs, Coordinate node, string startId, double? probeDistance = null)
    {
        if (legs.Count != 4)
        {
            throw new ArgumentException($"An opposite leg is only well defined on a four-way node, got {legs.Count}.", nameof(legs));
        }

        var ordered = legs
            .OrderBy(leg => BearingFromNode(leg.Geometry, node, probeDistance))
            .ToList();

        var start = ordered.FindIndex(leg => leg.Id == startId);
        var counterClockwiseNeighbour = (start + 1) % ordered.Count;
        var clockwiseNeighbour = (start + ordered.Count - 1) % ordered.Count;

        return ordered
            .Where((_, index) => index != start && index != counterClockwiseNeighbour && index != clockwiseNeighbour)
            .Single()
            .Id;
    }

    // The bearing (radians, [0, 2pi)) of the leg as it leaves the node, regardless of which way it was digitised.
    private static double BearingFromNode(LineString geometry, Coordinate node, double? probeDistance = null)
    {
        var awayFromNode = geometry.StartPoint.Coordinate.Distance(node) <= NodeTolerance
            ? geometry
            : (LineString)geometry.Reverse();

        if (awayFromNode.StartPoint.Coordinate.Distance(node) > NodeTolerance)
        {
            throw new InvalidOperationException("The leg must touch the node with one of its end vertices.");
        }

        // Clamped only as a backstop: a valid road segment is a metre long at least, so this cannot bite unless the
        // caller is handed legacy or not-yet-validated geometry.
        var probe = new LengthIndexedLine(awayFromNode)
            .ExtractPoint(Math.Min(probeDistance ?? ProbeDistance, awayFromNode.Length));

        var angle = Math.Atan2(probe.Y - node.Y, probe.X - node.X);
        return angle < 0 ? angle + 2 * Math.PI : angle;
    }

    private static readonly Coordinate Node = new(0, 0);

    // A straight leg leaving the node at the given bearing, digitised away from the node.
    private static Leg Radial(string id, double degrees, double length = 50)
    {
        return new Leg(id, RadialGeometry(degrees, length));
    }

    private static LineString RadialGeometry(double degrees, double length = 50)
    {
        var radians = degrees * Math.PI / 180;
        return new LineString([
            new Coordinate(0, 0),
            new Coordinate(length * Math.Cos(radians), length * Math.Sin(radians))
        ]);
    }

    // The same leg, but digitised towards the node - which is how they actually meet, on their end vertices.
    private static Leg Reversed(Leg leg) => leg with { Geometry = (LineString)leg.Geometry.Reverse() };

    [Fact]
    public void SquareCrossing_EveryLegFindsItsOpposite()
    {
        List<Leg> legs = [Radial("east", 0), Radial("north", 90), Radial("west", 180), Radial("south", 270)];

        Opposite(legs, Node, "east").Should().Be("west");
        Opposite(legs, Node, "west").Should().Be("east");
        Opposite(legs, Node, "north").Should().Be("south");
        Opposite(legs, Node, "south").Should().Be("north");
    }

    [Fact]
    public void SkewedCrossing_StillFindsTheOpposite()
    {
        // Nothing is at a right angle here.
        List<Leg> legs = [Radial("a", 0), Radial("b", 80), Radial("c", 170), Radial("d", 280)];

        Opposite(legs, Node, "a").Should().Be("c");
        Opposite(legs, Node, "b").Should().Be("d");
        Opposite(legs, Node, "c").Should().Be("a");
        Opposite(legs, Node, "d").Should().Be("b");
    }

    [Fact]
    public void ThreeLegsLeavingInAlmostTheSameDirection_StillResolveByPositionAlone()
    {
        // The angles are lopsided: "a" and "c" leave only 20 degrees apart while "d" heads back the other way. The
        // rule does not care - "b" and "d" are the neighbours of "a", so "c" is the opposite.
        List<Leg> legs = [Radial("a", 0), Radial("b", 10), Radial("c", 20), Radial("d", 190)];

        Opposite(legs, Node, "a").Should().Be("c");
        Opposite(legs, Node, "b").Should().Be("d");
        Opposite(legs, Node, "c").Should().Be("a");
        Opposite(legs, Node, "d").Should().Be("b");
    }

    [Fact]
    public void TheResultIsMutualAndIndependentOfInputOrderAndDigitisationDirection()
    {
        List<Leg> legs = [Radial("a", 15), Radial("b", 95), Radial("c", 200), Radial("d", 290)];

        // all four meeting the node with their end vertex, handed over in a different order
        List<Leg> shuffled = [Reversed(legs[2]), Reversed(legs[0]), Reversed(legs[3]), Reversed(legs[1])];

        foreach (var leg in legs)
        {
            var opposite = Opposite(legs, Node, leg.Id);

            Opposite(shuffled, Node, leg.Id).Should().Be(opposite,
                "neither the order of the input nor the digitisation direction may change the answer");
            Opposite(legs, Node, opposite).Should().Be(leg.Id,
                "the relation must be mutual");
        }
    }

    [Fact]
    public void ACurvingLegIsJudgedOnHowItLeavesTheNode_NotOnWhereItEndsUp()
    {
        // "west" leaves the node heading west, then swings north and ends up north-east of the node - it even crosses
        // the north leg on the way, which is a perfectly normal bridge/tunnel situation. Judging it by its far end
        // would sort it before "north" and hand back the wrong opposite.
        var west = new Leg("west", new LineString([
            new Coordinate(0, 0), new Coordinate(-1, 0), new Coordinate(-20, 10), new Coordinate(4, 45)
        ]));
        List<Leg> legs = [Radial("east", 0), Radial("north", 90), west, Radial("south", 270)];

        Opposite(legs, Node, "east").Should().Be("west");
        Opposite(legs, Node, "west").Should().Be("east");

        // for contrast: the far endpoint of "west" sits at a bearing of ~85 degrees, i.e. before "north"
        (Math.Atan2(45, 4) * 180 / Math.PI).Should().BeLessThan(90);
    }

    [Fact]
    public void ALegThatTurnsWithinTheFirstMetre_IsJudgedOnItsDepartureDirection()
    {
        // This is why the probe is 15cm and not a metre. "west" leaves the node heading due west and has swung round
        // to 137 degrees by the time it is a metre away - past "northwest", which genuinely leaves at 140.
        var west = new Leg("west", new LineString([
            new Coordinate(0, 0), new Coordinate(-0.4, 0), new Coordinate(-0.6, 0.5), new Coordinate(-2, 40)
        ]));
        List<Leg> legs = [Radial("east", 0), Radial("northwest", 140), west, Radial("south", 270)];

        (BearingFromNode(west.Geometry, Node, 0.15) * 180 / Math.PI).Should().BeApproximately(180, 0.01);
        (BearingFromNode(west.Geometry, Node, 1.0) * 180 / Math.PI).Should().BeApproximately(137, 0.01);

        Opposite(legs, Node, "east").Should().Be("west");

        // At a metre "west" sorts before "northwest" and the two swap places, so east is handed the wrong opposite.
        Opposite(legs, Node, "east", probeDistance: 1.0).Should().Be("northwest");
    }

    [Fact]
    public void ALegShorterThanTheProbeDistanceFallsBackToItsFarEnd()
    {
        // A valid road segment is a metre long at least, so this needs legacy data or the one exemption from the
        // minimum-length rule (the feature compare upload reader passes skipMinimumLengthCheck). The clamp keeps it
        // answerable rather than throwing.
        List<Leg> legs = [Radial("east", 0), Radial("north", 90), Radial("west", 180, length: 0.1), Radial("south", 270)];

        Opposite(legs, Node, "east").Should().Be("west");
    }

    [Fact]
    public void ProbeDistance_TradeOff_TheShorterProbeIsNoisier()
    {
        // A leg that runs due east, but whose first vertex carries one centimetre of digitising jitter - the smallest
        // error the stored coordinate precision can express. The probe distance is the lever on how much that jitter
        // rotates the measured bearing, and 15cm is the noisiest of the three.
        var jittery = new LineString([new Coordinate(0, 0), new Coordinate(0.15, 0.01), new Coordinate(50, 0)]);

        (BearingFromNode(jittery, Node, 0.15) * 180 / Math.PI).Should().BeApproximately(3.81, 0.02);
        (BearingFromNode(jittery, Node, 0.5) * 180 / Math.PI).Should().BeApproximately(1.15, 0.02);
        (BearingFromNode(jittery, Node, 1.0) * 180 / Math.PI).Should().BeApproximately(0.57, 0.02);

        // Which is the price of reading the departure direction: two legs leaving less than ~4 degrees apart cannot
        // be told apart, so a node like that needs something other than geometry to resolve it.
    }

    [Fact]
    public void TwoLegsLeavingAlongTheSameBearing_AreOrderedButTheAnswerIsArbitrary()
    {
        // Two legs leave the node on exactly the same bearing (they only diverge further along). Nothing measured at
        // the node can separate them, so which of the two counts as the neighbour is down to the sort being stable.
        // A real four-way node should never look like this; worth detecting rather than trusting the answer.
        List<Leg> legs = [Radial("east", 0), Radial("north", 90), Radial("west", 180), Radial("west-again", 180)];

        var bearings = legs.Select(leg => BearingFromNode(leg.Geometry, Node)).ToList();

        bearings.Distinct().Should().HaveCount(3, "two legs share a bearing, so the ordering between them is arbitrary");
    }

    // Every permutation of four legs, so the scramble below covers all of them rather than a lucky few.
    private static readonly int[][] Permutations = Permute([0, 1, 2, 3]).ToArray();

    private static IEnumerable<int[]> Permute(int[] values)
    {
        if (values.Length <= 1)
        {
            yield return values;
            yield break;
        }

        for (var i = 0; i < values.Length; i++)
        {
            var rest = values.Where((_, index) => index != i).ToArray();
            foreach (var permutation in Permute(rest))
            {
                yield return [values[i], .. permutation];
            }
        }
    }

    [Fact]
    public void EveryFourWayShape_AtOneDegreeIncrements_FindsTheOpposite()
    {
        // The legs are generated in increasing angular order, so by construction leg 0 is opposite leg 2 and leg 1 is
        // opposite leg 3. The first leg is pinned at 0 degrees because rotating the whole node is a separate concern
        // (covered below), which leaves every strictly increasing triple: C(359,3) = 7 647 059 distinct shapes.
        var geometries = Enumerable.Range(0, 360).Select(degrees => RadialGeometry(degrees)).ToArray();
        var failures = new ConcurrentBag<string>();

        Parallel.For(1, 358, second =>
        {
            for (var third = second + 1; third < 359; third++)
            for (var fourth = third + 1; fourth < 360; fourth++)
            {
                Leg[] legs =
                [
                    new("0", geometries[0]),
                    new("1", geometries[second]),
                    new("2", geometries[third]),
                    new("3", geometries[fourth])
                ];

                // Deterministic, but varying across the run, so no ordering assumption can survive.
                var permutation = Permutations[((second * 31 + third) * 31 + fourth) % Permutations.Length];
                var scrambled = permutation.Select(index => legs[index]).ToArray();

                if (Opposite(scrambled, Node, "0") != "2" || Opposite(scrambled, Node, "1") != "3")
                {
                    failures.Add($"0/{second}/{third}/{fourth} (order {string.Join(",", permutation)})");
                }
            }
        });

        failures.Should().BeEmpty();
    }

    // The same leg as Radial, but shaped the way the risk actually shows up: a vertex sitting at the probe distance
    // (the densest a leg may legally be) and every coordinate rounded to the centimetre, which is how v2 stores them.
    private static LineString RoundedRadialGeometry(double degrees, double length = 50)
    {
        var radians = degrees * Math.PI / 180;
        Coordinate Round(double distance) => new(
            Math.Round(distance * Math.Cos(radians), 2),
            Math.Round(distance * Math.Sin(radians), 2));

        return new LineString([new Coordinate(0, 0), Round(ProbeDistance), Round(length)]);
    }

    private sealed class SweepStats
    {
        public long Checked;
        public long SkippedForCoincidentVertices;
        public long Failed;
        public int WidestFailingGap;
        public readonly List<string> Samples = [];
    }

    [Fact]
    public void EveryFourWayShape_WithCentimetreRoundedCoordinates_FindsTheOpposite()
    {
        // Same exhaustive sweep, but on geometry stored the way v2 stores it. Where the rounding makes two legs share
        // the probe vertex outright there is nothing left to tell them apart, so that shape is skipped rather than
        // counted as a failure - it cannot occur as two distinct road segments.
        var geometries = Enumerable.Range(0, 360).Select(degrees => RoundedRadialGeometry(degrees)).ToArray();
        var probeVertices = geometries.Select(geometry => geometry.GetCoordinateN(1)).ToArray();
        var total = new SweepStats();

        Parallel.For(1, 358, () => new SweepStats(), (second, _, stats) =>
        {
            for (var third = second + 1; third < 359; third++)
            for (var fourth = third + 1; fourth < 360; fourth++)
            {
                int[] angles = [0, second, third, fourth];

                if (angles.SelectMany(_ => angles, (left, right) => (left, right))
                    .Any(pair => pair.left < pair.right && probeVertices[pair.left].Equals2D(probeVertices[pair.right])))
                {
                    stats.SkippedForCoincidentVertices++;
                    continue;
                }

                Leg[] legs = angles.Select((angle, index) => new Leg(index.ToString(), geometries[angle])).ToArray();
                var permutation = Permutations[((second * 31 + third) * 31 + fourth) % Permutations.Length];
                var scrambled = permutation.Select(index => legs[index]).ToArray();

                stats.Checked++;
                if (Opposite(scrambled, Node, "0") == "2" && Opposite(scrambled, Node, "1") == "3")
                {
                    continue;
                }

                stats.Failed++;
                var gap = new[] { second, third - second, fourth - third, 360 - fourth }.Min();
                stats.WidestFailingGap = Math.Max(stats.WidestFailingGap, gap);
                if (stats.Samples.Count < 5)
                {
                    stats.Samples.Add($"0/{second}/{third}/{fourth} (narrowest gap {gap} degrees)");
                }
            }

            return stats;
        }, stats =>
        {
            lock (total)
            {
                total.Checked += stats.Checked;
                total.SkippedForCoincidentVertices += stats.SkippedForCoincidentVertices;
                total.Failed += stats.Failed;
                total.WidestFailingGap = Math.Max(total.WidestFailingGap, stats.WidestFailingGap);
                total.Samples.AddRange(stats.Samples.Take(Math.Max(0, 5 - total.Samples.Count)));
            }
        });

        _output.WriteLine($"checked {total.Checked:N0}, skipped {total.SkippedForCoincidentVertices:N0}, failed {total.Failed:N0}");
        _output.WriteLine($"widest narrowest-gap among failures: {total.WidestFailingGap} degrees");
        foreach (var sample in total.Samples)
        {
            _output.WriteLine(sample);
        }

        (total.Checked + total.SkippedForCoincidentVertices).Should().Be(7_647_059, "every shape is accounted for");
        total.SkippedForCoincidentVertices.Should().BeGreaterThan(0,
            "rounding to the centimetre does merge probe vertices, so the skip has to be doing something");

        // The interesting result: of the shapes that survive, not one is ordered wrongly. Rounding the probe point to
        // the centimetre grid moves it, but it does not reorder two legs around the node unless it collapses them
        // onto the same point outright - and that case is excluded above.
        total.Failed.Should().Be(0);
    }

    [Fact]
    public void RotatingTheWholeNode_ChangesNothing()
    {
        // The one degree of freedom the exhaustive test pins down: the same shape at every orientation.
        var failures = new List<int>();

        for (var rotation = 0; rotation < 360; rotation++)
        {
            List<Leg> legs =
            [
                Radial("0", rotation),
                Radial("1", (rotation + 37) % 360),
                Radial("2", (rotation + 180) % 360),
                Radial("3", (rotation + 293) % 360)
            ];

            if (Opposite(legs, Node, "0") != "2" || Opposite(legs, Node, "1") != "3")
            {
                failures.Add(rotation);
            }
        }

        failures.Should().BeEmpty();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void OutsideAFourWayNode_ThereIsNoSingleNonNeighbour(int legCount)
    {
        var legs = Enumerable.Range(0, legCount)
            .Select(i => Radial($"leg{i}", i * 360.0 / legCount))
            .ToList();

        var act = () => Opposite(legs, Node, "leg0");

        act.Should().Throw<ArgumentException>();
    }
}
