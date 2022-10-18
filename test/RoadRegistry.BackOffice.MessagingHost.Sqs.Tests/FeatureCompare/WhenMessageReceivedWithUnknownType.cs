namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare;

using Fixtures;

public class WhenMessageReceivedWithUnknownType : IClassFixture<WhenMessageReceivedWithUnknownTypeFixture>
{
    private readonly WhenMessageReceivedWithUnknownTypeFixture _fixture;

    public WhenMessageReceivedWithUnknownType(WhenMessageReceivedWithUnknownTypeFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "TODO: Fixture completion")]
    public void ItShouldNotSucceed()
    {
        Assert.False(_fixture.Result);
    }
}