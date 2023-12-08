namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Abstractions;

using Api.RoadSegments.ChangeAttributes;
using Extensions;
using Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Xunit.Abstractions;

public abstract class WhenChangeAttributesWithInvalidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeAttributesFixture
{
    public readonly TFixture Fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenChangeAttributesWithInvalidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        OutputHelper = outputHelper;
    }

    protected async Task ItShouldHaveExpectedError(ChangeRoadSegmentAttributesParameters request, string expectedErrorCode, string expectedErrorMessagePrefix)
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
