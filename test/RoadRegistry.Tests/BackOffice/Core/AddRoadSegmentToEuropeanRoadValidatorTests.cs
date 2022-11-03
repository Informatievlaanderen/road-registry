namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;
using AddRoadSegmentToEuropeanRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToEuropeanRoad;

public class AddRoadSegmentToEuropeanRoadValidatorTests : ValidatorTest<AddRoadSegmentToEuropeanRoad, AddRoadSegmentToEuropeanRoadValidator>
{
    public AddRoadSegmentToEuropeanRoadValidatorTests()
    {
        Fixture.CustomizeAttributeId();

        Model = new AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = EuropeanRoadNumber.All[new Random().Next(0, EuropeanRoadNumber.All.Length)].ToString()
        };
    }

    [Fact]
    public void RoadNumberMustBeWithinDomain()
    {
        var acceptable = Array.ConvertAll(EuropeanRoadNumber.All, candidate => candidate.ToString());
        var value = new Generator<string>(Fixture).First(candidate => !acceptable.Contains(candidate));
        ShouldHaveValidationErrorFor(c => c.Number, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }
}