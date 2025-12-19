namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using AddGradeSeparatedJunction = RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction;

public class AddGradeSeparatedJunctionValidatorTests : ValidatorTest<AddGradeSeparatedJunction, AddGradeSeparatedJunctionValidator>
{
    public AddGradeSeparatedJunctionValidatorTests()
    {
        Fixture.CustomizeGradeSeparatedJunctionId();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeGradeSeparatedJunctionType();
        Fixture.CustomizePoint();

        Model = new AddGradeSeparatedJunction
        {
            TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
            UpperSegmentId = Fixture.Create<RoadSegmentId>(),
            LowerSegmentId = Fixture.Create<RoadSegmentId>(),
            Type = Fixture.Create<GradeSeparatedJunctionType>()
        };
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void LowerSegmentIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.LowerSegmentId, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.TemporaryId, value);
    }

    [Fact]
    public void TypeMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void UpperSegmentIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.UpperSegmentId, value);
    }
}
