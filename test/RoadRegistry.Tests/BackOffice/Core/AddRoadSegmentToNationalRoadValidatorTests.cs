namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using AddRoadSegmentToNationalRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNationalRoad;

public class AddRoadSegmentToNationalRoadValidatorTests : ValidatorTest<AddRoadSegmentToNationalRoad, AddRoadSegmentToNationalRoadValidator>
{
    public AddRoadSegmentToNationalRoadValidatorTests()
    {
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeNationalRoadNumber();
        Fixture.CustomizeRoadSegmentGeometryDrawMethod();

        Model = new AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = Fixture.Create<NationalRoadNumber>(),
            SegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>()
        };
    }

    [Fact]
    public void Ident2MustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Number, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }
}
