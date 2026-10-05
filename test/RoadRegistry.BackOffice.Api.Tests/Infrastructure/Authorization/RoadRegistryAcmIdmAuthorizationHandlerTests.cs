namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Authorization;

using System.Security.Claims;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm.AuthorizationHandlers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using RoadRegistry.BackOffice.Api.Infrastructure.Authorization;

public class RoadRegistryAcmIdmAuthorizationHandlerTests
{
    private const string BlacklistedOvoCode = "OVO000001";

    [Fact]
    public async Task WhenScopeIsItsOwnClaim_ThenSucceeds()
    {
        var result = await Authorize([Scope(Scopes.DvWrGeschetsteWegBeheer)]);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WhenScopeSitsInASpaceSeparatedClaim_ThenSucceeds()
    {
        // What the Bearer scheme produces: ACM/IDM returns every scope of the token in a single value. Comparing that
        // value to the required scope refuses the caller, which is the bug this handler exists for.
        var result = await Authorize([Scope($"vo_info {Scopes.DvWrGeschetsteWegBeheer} {Scopes.DvWrIngemetenWegBeheer}")]);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WhenScopeIsOneOfSeveralClaims_ThenSucceeds()
    {
        var result = await Authorize([Scope("vo_info"), Scope(Scopes.DvWrGeschetsteWegBeheer)]);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WhenScopeDiffersInCasing_ThenSucceeds()
    {
        var result = await Authorize([Scope(Scopes.DvWrGeschetsteWegBeheer.ToUpperInvariant())]);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("vo_info")]
    [InlineData("vo_info dv_wr_ingemetenweg_beheer")]
    // A scope the required one is a prefix of must not be mistaken for it.
    [InlineData("dv_wr_geschetsteweg_beheer_readonly")]
    public async Task WhenTheRequiredScopeIsMissing_ThenFails(string scopeClaimValue)
    {
        var result = await Authorize([Scope(scopeClaimValue)]);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenThereAreNoClaims_ThenFails()
    {
        var result = await Authorize([]);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenTheOvoCodeIsBlacklisted_ThenFailsEvenWithTheScope()
    {
        var result = await Authorize([
            Scope(Scopes.DvWrGeschetsteWegBeheer),
            new Claim(AcmIdmClaimTypes.VoOvoCode, BlacklistedOvoCode)
        ]);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenTheOrgCodeIsABlacklistedOvoCode_ThenFails()
    {
        // No vo_ovocode claim, so an organisation code that looks like an OVO code is used instead.
        var result = await Authorize([
            Scope(Scopes.DvWrGeschetsteWegBeheer),
            new Claim(AcmIdmClaimTypes.VoOrgCode, BlacklistedOvoCode.ToLowerInvariant())
        ]);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenTheOvoCodeIsNotBlacklisted_ThenSucceeds()
    {
        var result = await Authorize([
            Scope(Scopes.DvWrGeschetsteWegBeheer),
            new Claim(AcmIdmClaimTypes.VoOvoCode, "OVO002949")
        ]);

        result.Should().BeTrue();
    }

    private static Claim Scope(string value)
    {
        return new Claim(AcmIdmClaimTypes.Scope, value);
    }

    private static async Task<bool> Authorize(Claim[] claims)
    {
        var requirement = new AcmIdmAuthorizationRequirement(
            [Scopes.DvWrGeschetsteWegBeheer],
            [BlacklistedOvoCode]);

        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme")),
            null);

        await new RoadRegistryAcmIdmAuthorizationHandler().HandleAsync(context);

        return context.HasSucceeded;
    }
}
