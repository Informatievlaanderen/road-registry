namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;

public class AddGradeSeparatedJunctionValidatorTests
{
    public AddGradeSeparatedJunctionValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeGradeSeparatedJunctionId();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeGradeSeparatedJunctionType();
        Validator = new AddGradeSeparatedJunctionValidator();
        RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction model = new()
        {
            TemporaryId = value
        };

    }

    public Fixture Fixture { get; }

    public AddGradeSeparatedJunctionValidator Validator { get; }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryIdMustBeGreaterThan(int value)
    {
        var result = Validator.TestValidate(Model);
        result.ShouldHaveValidationErrorFor(c => c.TemporaryId);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void UpperSegmentIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.UpperSegmentId, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void LowerSegmentIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.LowerSegmentId, value);
    }

    [Fact]
    public void TypeMustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
    }

    [Fact]
    public void VerifyValid()
    {
        Fixture.CustomizePoint();

        var data = new RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction
        {
            TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
            UpperSegmentId = Fixture.Create<RoadSegmentId>(),
            LowerSegmentId = Fixture.Create<RoadSegmentId>(),
            Type = Fixture.Create<GradeSeparatedJunctionType>()
        };

        Validator.ValidateAndThrow(data);
    }
}
