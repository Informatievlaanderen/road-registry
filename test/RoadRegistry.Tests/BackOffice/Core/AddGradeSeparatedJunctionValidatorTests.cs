namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;
using AddGradeSeparatedJunction = RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction;

public class AddGradeSeparatedJunctionValidatorTests
{
    public AddGradeSeparatedJunctionValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeGradeSeparatedJunctionId();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeGradeSeparatedJunctionType();
        Validator = new AddGradeSeparatedJunctionValidator();
    }

    public Fixture Fixture { get; }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void LowerSegmentIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.LowerSegmentId, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.TemporaryId, value);
    }

    [Fact]
    public void TypeMustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void UpperSegmentIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.UpperSegmentId, value);
    }

    public AddGradeSeparatedJunctionValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        Fixture.CustomizePoint();

        var data = new AddGradeSeparatedJunction
        {
            TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
            UpperSegmentId = Fixture.Create<RoadSegmentId>(),
            LowerSegmentId = Fixture.Create<RoadSegmentId>(),
            Type = Fixture.Create<GradeSeparatedJunctionType>()
        };

        Validator.ValidateAndThrow(data);
    }
}