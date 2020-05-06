namespace RoadRegistry.BackOffice.Core
{
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using Xunit;

    public class RequestedRoadSegmentSurfaceAttributeValidatorTests
    {
        public RequestedRoadSegmentSurfaceAttributeValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeAttributeId();
            Fixture.CustomizeRoadSegmentPosition();
            Fixture.CustomizeRoadSegmentSurfaceType();
            Validator = new RequestedRoadSegmentSurfaceAttributeValidator();
        }

        public Fixture Fixture { get; }

        public RequestedRoadSegmentSurfaceAttributeValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void AttributeIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.AttributeId, value);
        }

        [Theory]
        [MemberData(nameof(DynamicAttributePositionCases.NegativeFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
        public void FromPositionMustBePositive(decimal value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.FromPosition, value);
        }

        [Theory]
        [MemberData(nameof(DynamicAttributePositionCases.ToPositionLessThanFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
        public void ToPositionMustBeGreaterThanFromPosition(decimal from, decimal to)
        {
            var data = new RequestedRoadSegmentSurfaceAttribute
            {
                FromPosition = from,
                ToPosition = to
            };
            Validator.ShouldHaveValidationErrorFor(c => c.ToPosition, data);
        }

        [Fact]
        public void TypeMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
        }

        [Fact]
        public void VerifyValid()
        {
            var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
            var from = positionGenerator.First(candidate => candidate >= 0.0m);

            var data = new RequestedRoadSegmentSurfaceAttribute
            {
                AttributeId = Fixture.Create<AttributeId>(),
                FromPosition = from,
                ToPosition = positionGenerator.First(candidate => candidate > from),
                Type = Fixture.Create<RoadSegmentSurfaceType>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
