namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Abstractions;

using Extensions;
using Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Xunit.Abstractions;

public abstract class WhenChangeOutlineGeometryWithInvalidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeOutlineGeometryFixture
{
    protected readonly TFixture Fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenChangeOutlineGeometryWithInvalidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        OutputHelper = outputHelper;
    }

    protected abstract string ExpectedErrorCode { get; }
    protected abstract string ExpectedErrorMessagePrefix { get; }

    [Fact]
    public void ItShouldHaveExpectedMessage()
    {
        var err = ItShouldHaveValidationException();
        var errMessage = err.Single().ErrorMessage;

        Assert.StartsWith(ExpectedErrorMessagePrefix, errMessage);
    }


    [Fact]
    public void ItShouldHaveExpectedCode()
    {
        var err = ItShouldHaveValidationException();
        var errMessage = err.Single().ErrorCode;

        Assert.Equal(ExpectedErrorCode, errMessage);
    }

    [Fact]
    public IEnumerable<ValidationFailure> ItShouldHaveValidationException()
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
}
