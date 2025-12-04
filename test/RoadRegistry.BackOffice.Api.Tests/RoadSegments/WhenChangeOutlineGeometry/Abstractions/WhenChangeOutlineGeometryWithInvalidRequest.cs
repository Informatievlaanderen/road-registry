namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Abstractions;

using System.Collections.Generic;
using System.Linq;
using CommandHandling;
using Extensions;
using Fixtures;
using FluentValidation;
using FluentValidation.Results;

public abstract class WhenChangeOutlineGeometryWithInvalidRequest<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeOutlineGeometryFixture
{
    private readonly TFixture _fixture;

    protected WhenChangeOutlineGeometryWithInvalidRequest(TFixture fixture)
    {
        _fixture = fixture;
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
        var ex = Assert.IsType<ValidationException>(_fixture.Exception);
        var err = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(ex.Errors);
        return err.TranslateToDutch();
    }

    [Fact]
    public void ItShouldThrow()
    {
        Assert.True(_fixture.HasException);
        Assert.NotNull(_fixture.Exception);
        Assert.Null(_fixture.Result);
    }
}
