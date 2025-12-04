namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Abstractions;

using System.Collections.Generic;
using System.Linq;
using CommandHandling;
using Extensions;
using Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Xunit.Abstractions;

public abstract class WhenDeleteOutlineWithInvalidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenDeleteOutlineFixture
{
    protected readonly TFixture Fixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected WhenDeleteOutlineWithInvalidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        OutputHelper = outputHelper;
    }

    protected abstract string ExpectedErrorCode { get; }
    protected abstract string ExpectedErrorMessagePrefix { get; }

    [Fact]
    public void ItShouldHaveExpectedCode()
    {
        var err = ItShouldHaveValidationException();
        var errMessage = err.Single().ErrorCode;

        Assert.Equal(ExpectedErrorCode, errMessage);
    }

    [Fact]
    public void ItShouldHaveExpectedMessage()
    {
        var err = ItShouldHaveValidationException();
        var errMessage = err.Single().ErrorMessage;

        Assert.StartsWith(ExpectedErrorMessagePrefix, errMessage);
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
}
