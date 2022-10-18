namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare;

using Fixtures;

public class WhenMessageReceivedWithKnownType : IClassFixture<WhenMessageReceivedWithKnownTypeFixture>
{
    public WhenMessageReceivedWithKnownType(WhenMessageReceivedWithKnownTypeFixture fixture)
    {
        _fixture = fixture;
    }

    private readonly WhenMessageReceivedWithKnownTypeFixture _fixture;

    [Fact(Skip = "TODO: Fixture completion")]
    public void ItShouldSucceed()
    {
        Assert.True(_fixture.Result);
    }
}
