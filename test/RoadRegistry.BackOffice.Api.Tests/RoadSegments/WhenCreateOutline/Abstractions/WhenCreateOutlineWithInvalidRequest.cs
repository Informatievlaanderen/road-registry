namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Abstractions;

using Api.RoadSegments;
using Extensions;
using Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Xunit.Abstractions;

public abstract class WhenCreateOutlineWithInvalidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenCreateOutlineFixture
{
    protected readonly TFixture Fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenCreateOutlineWithInvalidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        OutputHelper = outputHelper;
    }

    protected abstract string ExpectedErrorCode { get; }
    protected abstract string ExpectedErrorMessagePrefix { get; }

    [Fact]
    public void ItShouldHaveExpectedCode()
    {
        var errMessage = ItShouldHaveSingleError().ErrorCode;

        Assert.Equal(ExpectedErrorCode, errMessage);
    }

    [Fact]
    public void ItShouldHaveExpectedMessage()
    {
        var errMessage = ItShouldHaveSingleError().ErrorMessage;

        Assert.StartsWith(ExpectedErrorMessagePrefix, errMessage);
    }
    
    protected async Task ItShouldHaveExpectedError(PostRoadSegmentOutlineParameters request, string expectedErrorCode, string expectedErrorMessagePrefix)
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

    [Fact]
    public void ItShouldThrow()
    {
        Assert.True(Fixture.HasException);
        Assert.NotNull(Fixture.Exception);
        Assert.Null(Fixture.Result);
    }

    private ValidationFailure ItShouldHaveSingleError()
    {
        var err = ItShouldHaveValidationException().ToArray();
        if (err.Length > 1)
        {
            foreach (var error in err)
            {
                OutputHelper.WriteLine(error.ToString());
            }
        }

        return err.Single();
    }
}
