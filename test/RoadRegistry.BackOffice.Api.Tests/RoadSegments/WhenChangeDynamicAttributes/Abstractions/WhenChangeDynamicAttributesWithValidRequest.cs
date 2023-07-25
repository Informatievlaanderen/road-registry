namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes.Abstractions;

using Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit.Abstractions;

public abstract class WhenChangeDynamicAttributesWithValidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeDynamicAttributesFixture
{
    protected readonly TFixture Fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenChangeDynamicAttributesWithValidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        OutputHelper = outputHelper;
    }

    [Fact]
    public void ItShouldSucceed()
    {
        if (Fixture.Exception is not null)
        {
            OutputHelper.WriteLine($"{nameof(Fixture.Request)}: {JsonConvert.SerializeObject(Fixture.Request)}");
            OutputHelper.WriteLine(Fixture.Exception.ToString());
        }

        Assert.IsType<AcceptedResult>(Fixture.Result);
    }
}