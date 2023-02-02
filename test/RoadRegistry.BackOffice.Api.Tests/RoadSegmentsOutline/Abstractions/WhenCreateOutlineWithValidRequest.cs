namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Abstractions;

using Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit.Abstractions;

public class WhenCreateOutlineWithValidRequest : IClassFixture<WhenCreateOutlineWithValidRequestFixture>
{
    private readonly WhenCreateOutlineWithValidRequestFixture _fixture;
    protected readonly ITestOutputHelper _outputHelper;

    public WhenCreateOutlineWithValidRequest(WhenCreateOutlineWithValidRequestFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _outputHelper = outputHelper;
    }

    [Fact]
    public void ItShouldSucceed()
    {
        if (_fixture.Exception is not null)
        {
            _outputHelper.WriteLine($"{nameof(_fixture.Parameters)}: {JsonConvert.SerializeObject(_fixture.Parameters)}");
            _outputHelper.WriteLine(_fixture.Exception.ToString());
        }
        
        Assert.IsType<AcceptedResult>(_fixture.Result);
    }
}
