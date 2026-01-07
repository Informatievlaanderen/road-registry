namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;

public class RoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .WhenMigrate(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    OriginalId = TestData.Segment1Added.RoadSegmentId,
                    Geometry = TestData.AddSegment1.Geometry,
                    StartNodeId = TestData.AddSegment1.StartNodeId,
                    EndNodeId = TestData.AddSegment1.EndNodeId,
                    GeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
                    AccessRestriction = TestData.AddSegment1.AccessRestriction,
                    Category = TestData.AddSegment1.Category,
                    Morphology = TestData.AddSegment1.Morphology,
                    Status = TestData.AddSegment1.Status,
                    StreetNameId = TestData.AddSegment1.StreetNameId,
                    MaintenanceAuthorityId = TestData.AddSegment1.MaintenanceAuthorityId,
                    SurfaceType = TestData.AddSegment1.SurfaceType
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadSegments.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WithEuropeanRoadNumbers_ThenIncludedInEvent()
    {
        var linkedNumber = Fixture.Create<EuropeanRoadNumber>();
        var removedNumber = Fixture.CreateWhichIsDifferentThan(linkedNumber);

        return Run(scenario => scenario
            .Given(given => given)
            .WhenMigrate(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    OriginalId = TestData.Segment1Added.RoadSegmentId,
                    Geometry = TestData.AddSegment1.Geometry,
                    StartNodeId = TestData.AddSegment1.StartNodeId,
                    EndNodeId = TestData.AddSegment1.EndNodeId,
                    GeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
                    AccessRestriction = TestData.AddSegment1.AccessRestriction,
                    Category = TestData.AddSegment1.Category,
                    Morphology = TestData.AddSegment1.Morphology,
                    Status = TestData.AddSegment1.Status,
                    StreetNameId = TestData.AddSegment1.StreetNameId,
                    MaintenanceAuthorityId = TestData.AddSegment1.MaintenanceAuthorityId,
                    SurfaceType = TestData.AddSegment1.SurfaceType
                })
                .Add(new AddRoadSegmentToEuropeanRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = linkedNumber
                })
                .Add(new AddRoadSegmentToEuropeanRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                })
                .Add(new RemoveRoadSegmentFromEuropeanRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                })
            )
            .Then((result, events) =>
            {
                var roadSegmentWasMigrated = events.OfType<RoadSegmentWasMigrated>().Single();

                roadSegmentWasMigrated.EuropeanRoadNumbers.Should().HaveCount(1);
                roadSegmentWasMigrated.EuropeanRoadNumbers.Should().Contain(linkedNumber);
                roadSegmentWasMigrated.NationalRoadNumbers.Should().BeEmpty();
            })
        );
    }

    [Fact]
    public Task WithNationalRoadNumbers_ThenIncludedInEvent()
    {
        var linkedNumber = Fixture.Create<NationalRoadNumber>();
        var removedNumber = Fixture.CreateWhichIsDifferentThan(linkedNumber);

        return Run(scenario => scenario
            .Given(given => given)
            .WhenMigrate(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    OriginalId = TestData.Segment1Added.RoadSegmentId,
                    Geometry = TestData.AddSegment1.Geometry,
                    StartNodeId = TestData.AddSegment1.StartNodeId,
                    EndNodeId = TestData.AddSegment1.EndNodeId,
                    GeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
                    AccessRestriction = TestData.AddSegment1.AccessRestriction,
                    Category = TestData.AddSegment1.Category,
                    Morphology = TestData.AddSegment1.Morphology,
                    Status = TestData.AddSegment1.Status,
                    StreetNameId = TestData.AddSegment1.StreetNameId,
                    MaintenanceAuthorityId = TestData.AddSegment1.MaintenanceAuthorityId,
                    SurfaceType = TestData.AddSegment1.SurfaceType
                })
                .Add(new AddRoadSegmentToNationalRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = linkedNumber
                })
                .Add(new AddRoadSegmentToNationalRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                })
                .Add(new RemoveRoadSegmentFromNationalRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                })
            )
            .Then((result, events) =>
            {
                var roadSegmentWasMigrated = events.OfType<RoadSegmentWasMigrated>().Single();

                roadSegmentWasMigrated.NationalRoadNumbers.Should().HaveCount(1);
                roadSegmentWasMigrated.NationalRoadNumbers.Should().Contain(linkedNumber);
                roadSegmentWasMigrated.EuropeanRoadNumbers.Should().BeEmpty();
            })
        );
    }
}
