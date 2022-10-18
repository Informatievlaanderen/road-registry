namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare;

using Fixtures;

public class WhenMessageReceivedWithUnknownType : IClassFixture<WhenMessageReceivedWithUnknownTypeFixture>
{
    public WhenMessageReceivedWithUnknownType(WhenMessageReceivedWithUnknownTypeFixture fixture)
    {
        _fixture = fixture;
    }

    private readonly WhenMessageReceivedWithUnknownTypeFixture _fixture;

    [Fact(Skip = "TODO: Fixture completion")]
    public void ItShouldNotSucceed()
    {
        Assert.False(_fixture.Result);
    }
}
