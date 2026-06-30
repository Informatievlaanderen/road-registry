namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2.Abstractions;

using Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit.Abstractions;

public abstract class WhenCreateOutlineV2WithValidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenCreateOutlineV2Fixture
{
    private readonly TFixture _fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenCreateOutlineV2WithValidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        OutputHelper = outputHelper;
    }

    [Fact]
    public void ItShouldSucceed()
    {
        if (_fixture.Exception is not null)
        {
            OutputHelper.WriteLine($"{nameof(_fixture.Request)}: {JsonConvert.SerializeObject(_fixture.Request)}");
            OutputHelper.WriteLine(_fixture.Exception.ToString());
        }

        Assert.IsType<AcceptedResult>(_fixture.Result);
    }
}
