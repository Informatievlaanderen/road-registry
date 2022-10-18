namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare;

using Fixtures;

public class WhenMessageReceivedWithKnownType : IClassFixture<WhenMessageReceivedWithKnownTypeFixture>
{
    private readonly WhenMessageReceivedWithKnownTypeFixture _fixture;

    public WhenMessageReceivedWithKnownType(WhenMessageReceivedWithKnownTypeFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "TODO: Fixture completion")]
    public void ItShouldSucceed()
    {
        Assert.True(_fixture.Result);
    }
}