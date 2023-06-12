namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange.Abstractions;

using Api.RoadSegments.Change;
using Extensions;
using Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Xunit.Abstractions;

public abstract class WhenChangeWithInvalidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeFixture
{
    protected readonly TFixture Fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenChangeWithInvalidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        OutputHelper = outputHelper;
    }

    protected async Task ItShouldHaveExpectedError(ChangeRoadSegmentsParameters request, string expectedErrorCode, string expectedErrorMessagePrefix = null)
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
