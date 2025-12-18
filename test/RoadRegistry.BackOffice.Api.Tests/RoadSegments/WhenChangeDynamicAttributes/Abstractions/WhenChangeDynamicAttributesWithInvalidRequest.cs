namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes.Abstractions;

using System.Collections.Generic;
using System.Linq;
using Api.RoadSegments.ChangeDynamicAttributes;
using CommandHandling;
using Fixtures;
using FluentValidation;
using FluentValidation.Results;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.Infrastructure;
using Xunit.Abstractions;

public abstract class WhenChangeDynamicAttributesWithInvalidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeDynamicAttributesFixture
{
    protected readonly TFixture Fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenChangeDynamicAttributesWithInvalidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        OutputHelper = outputHelper;
    }

    protected async Task ItShouldHaveExpectedError(ChangeRoadSegmentsDynamicAttributesParameters request, string expectedErrorCode, string expectedErrorMessagePrefix = null)
    {
        await Fixture.ExecuteAsync(request);

        var errors = ItShouldHaveValidationException().ToArray();

        if (expectedErrorCode is not null)
        {
            Assert.Contains(expectedErrorCode, errors.Select(x => x.ErrorCode));
        }

        if (expectedErrorMessagePrefix is not null)
        {
            Assert.True(errors != null && errors.Any(x => x.ErrorMessage.StartsWith(expectedErrorMessagePrefix)));
        }
    }

    private IEnumerable<ValidationFailure> ItShouldHaveValidationException()
    {
        var ex = Assert.IsType<ValidationException>(Fixture.Exception);
        var err = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(ex.Errors);
        return err.TranslateToDutch();
    }
}
