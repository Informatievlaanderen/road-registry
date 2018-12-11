namespace RoadRegistry.Model
{
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
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
        }

        public Fixture Fixture { get; }

        public AddGradeSeparatedJunctionValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void TemporaryIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.TemporaryId, value);
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
            Fixture.CustomizePointM();

            var data = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                UpperSegmentId = Fixture.Create<RoadSegmentId>(),
                LowerSegmentId = Fixture.Create<RoadSegmentId>(),
                Type = Fixture.Create<GradeSeparatedJunctionType>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
