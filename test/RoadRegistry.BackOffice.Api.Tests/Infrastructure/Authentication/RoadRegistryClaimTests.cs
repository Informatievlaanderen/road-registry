namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Authentication;

using System.Data;
using System.Security.Claims;
using Api.Infrastructure.Authentication;

public class RoadRegistryClaimTests
{
    [Theory]
    [InlineData("admin")]
    [InlineData("editeerder")]
    [InlineData("lezer")]
    public void CanReadSuccesfully(string role)
    {
        var claim = new Claim("dv_wegenregister", $"DVWegenregister-{role}:OVO002949");
        var sut = RoadRegistryClaim.ReadFrom(claim);

        Assert.NotNull(sut);
        Assert.Equal(role, sut.Role);
    }

    [Fact]
    public void ReturnsNullWhenInvalidClaimType()
    {
        var claim = new Claim("abc", "123");
        var sut = RoadRegistryClaim.ReadFrom(claim);

        Assert.Null(sut);
    }
}
