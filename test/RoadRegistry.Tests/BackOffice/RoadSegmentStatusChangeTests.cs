namespace RoadRegistry.Tests.BackOffice;

using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;

// Guards the table every layer works from. A status change that is wired up wrong here goes wrong everywhere.
public class RoadSegmentStatusChangeTests
{
    public static TheoryData<string> AllChanges()
    {
        var data = new TheoryData<string>();
        foreach (var statusChange in RoadSegmentStatusChange.All)
        {
            data.Add(statusChange.Name);
        }
        return data;
    }

    [Fact]
    public void AllIsCompleteAndUnique()
    {
        // Every declared transition is in All exactly once, so nothing is added and then forgotten.
        var declared = typeof(RoadSegmentStatusChange)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(RoadSegmentStatusChange))
            .Select(x => (RoadSegmentStatusChange)x.GetValue(null)!)
            .ToArray();

        RoadSegmentStatusChange.All.Should().BeEquivalentTo(declared);
        RoadSegmentStatusChange.All.Select(x => x.Name).Should().OnlyHaveUniqueItems();
        RoadSegmentStatusChange.All.Select(x => x.EventType).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public void AChangeAlwaysMovesBetweenTwoDifferentStatuses(string name)
    {
        var statusChange = RoadSegmentStatusChange.Parse(name);

        statusChange.From.Should().NotBe(statusChange.To);
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public void AChangeHasExactlyOneOfTheThreeShapes(string name)
    {
        var statusChange = RoadSegmentStatusChange.Parse(name);

        new[] { statusChange.Connects, statusChange.Disconnects, statusChange.StaysUnconnected }
            .Count(x => x).Should().Be(1, "'gerealiseerd' is the only connecting status, so the shape follows from the two statuses");
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public void TheEventTypeMatchesTheShapeOfTheChange(string name)
    {
        var statusChange = RoadSegmentStatusChange.Parse(name);

        var expectedInterface = statusChange.Connects ? typeof(IRoadSegmentWasConnectedEvent)
            : statusChange.Disconnects ? typeof(IRoadSegmentWasDisconnectedEvent)
            : typeof(IRoadSegmentUnconnectedStatusChangeEvent);

        expectedInterface.IsAssignableFrom(statusChange.EventType).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public void ForEventFindsTheChangeBackFromTheEventItRaises(string name)
    {
        var statusChange = RoadSegmentStatusChange.Parse(name);

        // Built without going through the domain: what matters here is that the factory really does produce the event
        // type the table names, because that is what ForEvent - and with it every projection - relies on.
        var @event = BuildEventFor(statusChange);

        RoadSegmentStatusChange.ForEvent(@event).Should().Be(statusChange);
    }

    [Fact]
    public void EveryStatusChangeEventInTheDomainIsInTheTable()
    {
        // A new status change event that is not wired into the table would leave ForEvent - and with it every
        // projection and the aggregate itself - unable to say what status the segment ended up in.
        var eventTypes = typeof(IRoadSegmentStatusChangeEvent).Assembly
            .GetTypes()
            .Where(x => typeof(IRoadSegmentStatusChangeEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .ToArray();

        eventTypes.Should().BeEquivalentTo(RoadSegmentStatusChange.All.Select(x => x.EventType));
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public void AChangeRoundTripsThroughItsName(string name)
    {
        var statusChange = RoadSegmentStatusChange.Parse(name);

        RoadSegmentStatusChange.Parse(statusChange.ToString()).Should().Be(statusChange);
    }

    [Fact]
    public void ParsingSomethingElseThrows()
    {
        Assert.Throws<FormatException>(() => RoadSegmentStatusChange.Parse("NotAStatusChange"));
    }

    [Fact]
    public void GerealiseerdIsTheOnlyStatusThatConnectsToRoadNodes()
    {
        RoadSegmentStatusV2.All.Where(x => x.ConnectsToRoadNodes)
            .Should().BeEquivalentTo([RoadSegmentStatusV2.Gerealiseerd]);
    }

    // The event as the table's own factory builds it, with placeholder data: the values do not matter, only the type.
    private static IRoadSegmentStatusChangeEvent BuildEventFor(RoadSegmentStatusChange statusChange)
    {
        var testData = new RoadNetworkTestDataV2();
        var roadSegmentId = new RoadSegmentId(1);
        var provenance = new ProvenanceData(testData.Provenance);

        if (statusChange.Disconnects)
        {
            return statusChange.BuildEvent(new RoadSegmentDisconnectedChangeData
            {
                RoadSegmentId = roadSegmentId,
                PreviousStartNodeId = new RoadNodeId(1),
                PreviousEndNodeId = new RoadNodeId(2),
                Provenance = provenance
            });
        }

        if (statusChange.StaysUnconnected)
        {
            return statusChange.BuildEvent(new RoadSegmentUnconnectedChangeData
            {
                RoadSegmentId = roadSegmentId,
                Provenance = provenance
            });
        }

        var template = testData.Segment1Added;

        return statusChange.BuildEvent(new RoadSegmentConnectedChangeData
        {
            RoadSegmentId = roadSegmentId,
            Geometry = template.Geometry,
            StartNodeId = new RoadNodeId(1),
            EndNodeId = new RoadNodeId(2),
            Attributes = new RoadSegmentAttributes
            {
                GeometryDrawMethod = template.GeometryDrawMethod,
                AccessRestriction = template.AccessRestriction,
                Category = template.Category,
                Morphology = template.Morphology,
                StreetNameId = template.StreetNameId,
                MaintenanceAuthorityId = template.MaintenanceAuthorityId,
                SurfaceType = template.SurfaceType,
                CarTrafficDirection = template.CarTrafficDirection,
                BikeTrafficDirection = template.BikeTrafficDirection,
                PedestrianTrafficDirection = template.PedestrianTrafficDirection
            },
            Provenance = provenance
        });
    }
}
