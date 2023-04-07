namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Abstractions;

using Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit.Abstractions;

public abstract class WhenCreateOutlineWithValidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenCreateOutlineFixture
{
    private readonly TFixture _fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenCreateOutlineWithValidRequest(TFixture fixture, ITestOutputHelper outputHelper)
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
