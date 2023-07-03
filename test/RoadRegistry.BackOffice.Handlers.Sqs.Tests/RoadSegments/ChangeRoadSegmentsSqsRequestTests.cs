namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.RoadSegments;

using Abstractions.RoadSegments;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using EqualityComparers;
using Newtonsoft.Json;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Sqs.RoadSegments;

public class ChangeRoadSegmentsSqsRequestTests
{
    [Fact]
    public void SerializeDeserialize_Equals()
    {
        var serializer = JsonSerializer.Create(new FakeSqsOptions().JsonSerializerSettings);

        var request = CreateRequest();

        var sqsJsonMessage = SqsJsonMessage.Create(request, serializer);
        var deserializedRequest = Assert.IsType<ChangeRoadSegmentsSqsRequest>(sqsJsonMessage.Map(serializer));

        Assert.Equal(request.Request, deserializedRequest.Request, new ChangeRoadSegmentsRequestEqualityComparer());
    }

    [Fact]
    public void SerializeDeserialize_NotEquals()
    {
        var serializer = JsonSerializer.Create(new FakeSqsOptions().JsonSerializerSettings);

        var request = CreateRequest();

        var sqsJsonMessage = SqsJsonMessage.Create(request, serializer);
        var deserializedRequest = Assert.IsType<ChangeRoadSegmentsSqsRequest>(sqsJsonMessage.Map(serializer));

        deserializedRequest.Request.Add(new RoadSegmentId(3), _ => { });

        Assert.NotEqual(request.Request, deserializedRequest.Request, new ChangeRoadSegmentsRequestEqualityComparer());
    }

    private static ChangeRoadSegmentsSqsRequest CreateRequest()
    {
        var fixture = new Fixture();
        var testData = new RoadNetworkTestData();
        testData.CopyCustomizationsTo(fixture);

        return new ChangeRoadSegmentsSqsRequest
        {
            Request = new ChangeRoadSegmentsRequest
            {
                ChangeRequests = new ChangeRoadSegmentRequest[]
                {
                    new()
                    {
                        Id = new RoadSegmentId(1),
                        Lanes = new ChangeRoadSegmentLaneAttributeRequest[]
                        {
                            new()
                            {
                                FromPosition = fixture.Create<RoadSegmentPosition>(),
                                ToPosition = fixture.Create<RoadSegmentPosition>(),
                                Direction = fixture.Create<RoadSegmentLaneDirection>(),
                                Count = fixture.Create<RoadSegmentLaneCount>()
                            }
                        },
                        Surfaces = new ChangeRoadSegmentSurfaceAttributeRequest[]
                        {
                            new()
                            {
                                FromPosition = fixture.Create<RoadSegmentPosition>(),
                                ToPosition = fixture.Create<RoadSegmentPosition>(),
                                Type = fixture.Create<RoadSegmentSurfaceType>()
                            }
                        },
                        Widths = new ChangeRoadSegmentWidthAttributeRequest[]
                        {
                            new()
                            {
                                FromPosition = fixture.Create<RoadSegmentPosition>(),
                                ToPosition = fixture.Create<RoadSegmentPosition>(),
                                Width = fixture.Create<RoadSegmentWidth>()
                            }
                        }
                    }
                }
            }
        };
    }
}
