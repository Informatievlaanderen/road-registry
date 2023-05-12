namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using AddRoadSegmentToNumberedRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNumberedRoad;

public class AddRoadSegmentToNumberedRoadValidatorTests : ValidatorTest<AddRoadSegmentToNumberedRoad, AddRoadSegmentToNumberedRoadValidator>
{
    public AddRoadSegmentToNumberedRoadValidatorTests()
    {
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeNumberedRoadNumber();
        Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        Fixture.CustomizeRoadSegmentNumberedRoadDirection();

        Model = new AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = Fixture.Create<NumberedRoadNumber>(),
            Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>(),
            Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>()
        };
    }

    [Fact]
    public void DirectionMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Direction, Fixture.Create<string>());
    }

    [Fact]
    public void Ident8MustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Number, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void OrdinalMustBeGreaterThanOrEqualToZero(int value)
    {
        ShouldHaveValidationErrorFor(c => c.Ordinal, value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    [InlineData(RoadSegmentNumberedRoadOrdinal.WellKnownValues.NotKnown)]
    public void OrdinalMustBeGreaterThanOrEqualToZeroOrAcceptedValue(int value)
    {
        ShouldNotHaveValidationErrorFor(c => c.Ordinal, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }
}