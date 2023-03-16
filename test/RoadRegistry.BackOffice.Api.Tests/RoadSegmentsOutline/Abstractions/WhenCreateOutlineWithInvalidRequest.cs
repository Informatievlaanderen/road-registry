namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Abstractions;

using Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Xunit.Abstractions;

public abstract class WhenCreateOutlineWithInvalidRequest<TFixture> : IClassFixture<TFixture> where TFixture : WhenCreateOutlineFixture
{
    protected readonly TFixture _fixture;
    protected readonly ITestOutputHelper _outputHelper;

    protected WhenCreateOutlineWithInvalidRequest(TFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _outputHelper = outputHelper;
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
        var ex = Assert.IsType<ValidationException>(_fixture.Exception);
        var err = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(ex.Errors);
        return err;
    }

    [Fact]
    public void ItShouldThrow()
    {
        Assert.True(_fixture.HasException);
        Assert.NotNull(_fixture.Exception);
        Assert.Null(_fixture.Result);
    }
}
