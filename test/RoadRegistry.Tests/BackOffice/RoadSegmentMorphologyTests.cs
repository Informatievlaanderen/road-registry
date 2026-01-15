namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentMorphologyTests
{
    private readonly Fixture _fixture;
    private readonly string[] _knownValues;

    public RoadSegmentMorphologyTests()
    {
        _fixture = FixtureFactory.Create();
        _knownValues = Array.ConvertAll(RoadSegmentMorphology.All, type => type.ToString());
    }

    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            new[]
            {
                RoadSegmentMorphology.Unknown,
                RoadSegmentMorphology.Motorway,
                RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway,
                RoadSegmentMorphology.Road_consisting_of_one_roadway,
                RoadSegmentMorphology.TrafficCircle,
                RoadSegmentMorphology.SpecialTrafficSituation,
                RoadSegmentMorphology.TrafficSquare,
                RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction,
                RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction,
                RoadSegmentMorphology.ParallelRoad,
                RoadSegmentMorphology.FrontageRoad,
                RoadSegmentMorphology.Entry_or_exit_of_a_car_park,
                RoadSegmentMorphology.Entry_or_exit_of_a_service,
                RoadSegmentMorphology.PedestrainZone,
                RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles,
                RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles,
                RoadSegmentMorphology.ServiceRoad,
                RoadSegmentMorphology.PrimitiveRoad,
                RoadSegmentMorphology.Ferry
            },
            RoadSegmentMorphology.All);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentMorphology.CanParse(value);
        Assert.False(result);
    }

    [Fact]
    public void CanParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentMorphology.CanParse(value);
        Assert.True(result);
    }

    [Fact]
    public void CanParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentMorphology.CanParse(null));
    }

    [Fact]
    public void Entry_or_exit_of_a_car_parkReturnsExpectedResult()
    {
        Assert.Equal("Entry_or_exit_of_a_car_park", RoadSegmentMorphology.Entry_or_exit_of_a_car_park);
    }

    [Fact]
    public void Entry_or_exit_of_a_car_parkTranslationReturnsExpectedResult()
    {
        Assert.Equal(111, RoadSegmentMorphology.Entry_or_exit_of_a_car_park.Translation.Identifier);
    }

    [Fact]
    public void Entry_or_exit_of_a_serviceReturnsExpectedResult()
    {
        Assert.Equal("Entry_or_exit_of_a_service", RoadSegmentMorphology.Entry_or_exit_of_a_service);
    }

    [Fact]
    public void Entry_or_exit_of_a_serviceTranslationReturnsExpectedResult()
    {
        Assert.Equal(112, RoadSegmentMorphology.Entry_or_exit_of_a_service.Translation.Identifier);
    }

    [Fact]
    public void Entry_or_exit_ramp_belonging_to_a_grade_separated_junctionReturnsExpectedResult()
    {
        Assert.Equal("Entry_or_exit_ramp_belonging_to_a_grade_separated_junction", RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction);
    }

    [Fact]
    public void Entry_or_exit_ramp_belonging_to_a_grade_separated_junctionTranslationReturnsExpectedResult()
    {
        Assert.Equal(107, RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction.Translation.Identifier);
    }

    [Fact]
    public void Entry_or_exit_ramp_belonging_to_a_level_junctionReturnsExpectedResult()
    {
        Assert.Equal("Entry_or_exit_ramp_belonging_to_a_level_junction", RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction);
    }

    [Fact]
    public void Entry_or_exit_ramp_belonging_to_a_level_junctionTranslationReturnsExpectedResult()
    {
        Assert.Equal(108, RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction.Translation.Identifier);
    }

    [Fact]
    public void FerryReturnsExpectedResult()
    {
        Assert.Equal("Ferry", RoadSegmentMorphology.Ferry);
    }

    [Fact]
    public void FerryTranslationReturnsExpectedResult()
    {
        Assert.Equal(130, RoadSegmentMorphology.Ferry.Translation.Identifier);
    }

    [Fact]
    public void FrontageRoadReturnsExpectedResult()
    {
        Assert.Equal("FrontageRoad", RoadSegmentMorphology.FrontageRoad);
    }

    [Fact]
    public void FrontageRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal(110, RoadSegmentMorphology.FrontageRoad.Translation.Identifier);
    }

    [Fact]
    public void MotorwayReturnsExpectedResult()
    {
        Assert.Equal("Motorway", RoadSegmentMorphology.Motorway);
    }

    [Fact]
    public void MotorwayTranslationReturnsExpectedResult()
    {
        Assert.Equal(101, RoadSegmentMorphology.Motorway.Translation.Identifier);
    }

    [Fact]
    public void ParallelRoadReturnsExpectedResult()
    {
        Assert.Equal("ParallelRoad", RoadSegmentMorphology.ParallelRoad);
    }

    [Fact]
    public void ParallelRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal(109, RoadSegmentMorphology.ParallelRoad.Translation.Identifier);
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        Assert.Throws<FormatException>(() => RoadSegmentMorphology.Parse(value));
    }

    [Fact]
    public void ParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        Assert.NotNull(RoadSegmentMorphology.Parse(value));
    }

    [Fact]
    public void ParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentMorphology.Parse(null));
    }

    [Fact]
    public void PedestrainZoneReturnsExpectedResult()
    {
        Assert.Equal("PedestrainZone", RoadSegmentMorphology.PedestrainZone);
    }

    [Fact]
    public void PedestrainZoneTranslationReturnsExpectedResult()
    {
        Assert.Equal(113, RoadSegmentMorphology.PedestrainZone.Translation.Identifier);
    }

    [Fact]
    public void PrimitiveRoadReturnsExpectedResult()
    {
        Assert.Equal("PrimitiveRoad", RoadSegmentMorphology.PrimitiveRoad);
    }

    [Fact]
    public void PrimitiveRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal(125, RoadSegmentMorphology.PrimitiveRoad.Translation.Identifier);
    }

    [Fact]
    public void Road_consisting_of_one_roadwayReturnsExpectedResult()
    {
        Assert.Equal("Road_consisting_of_one_roadway", RoadSegmentMorphology.Road_consisting_of_one_roadway);
    }

    [Fact]
    public void Road_consisting_of_one_roadwayTranslationReturnsExpectedResult()
    {
        Assert.Equal(103, RoadSegmentMorphology.Road_consisting_of_one_roadway.Translation.Identifier);
    }

    [Fact]
    public void Road_with_separate_lanes_that_is_not_a_motorwayReturnsExpectedResult()
    {
        Assert.Equal("Road_with_separate_lanes_that_is_not_a_motorway", RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway);
    }

    [Fact]
    public void Road_with_separate_lanes_that_is_not_a_motorwayTranslationReturnsExpectedResult()
    {
        Assert.Equal(102, RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway.Translation.Identifier);
    }

    [Fact]
    public void ServiceRoadReturnsExpectedResult()
    {
        Assert.Equal("ServiceRoad", RoadSegmentMorphology.ServiceRoad);
    }

    [Fact]
    public void ServiceRoadTranslationReturnsExpectedResult()
    {
        Assert.Equal(120, RoadSegmentMorphology.ServiceRoad.Translation.Identifier);
    }

    [Fact]
    public void SpecialTrafficSituationReturnsExpectedResult()
    {
        Assert.Equal("SpecialTrafficSituation", RoadSegmentMorphology.SpecialTrafficSituation);
    }

    [Fact]
    public void SpecialTrafficSituationTranslationReturnsExpectedResult()
    {
        Assert.Equal(105, RoadSegmentMorphology.SpecialTrafficSituation.Translation.Identifier);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var sut = RoadSegmentMorphology.Parse(value);
        var result = sut.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public void TrafficCircleReturnsExpectedResult()
    {
        Assert.Equal("TrafficCircle", RoadSegmentMorphology.TrafficCircle);
    }

    [Fact]
    public void TrafficCircleTranslationReturnsExpectedResult()
    {
        Assert.Equal(104, RoadSegmentMorphology.TrafficCircle.Translation.Identifier);
    }

    [Fact]
    public void TrafficSquareReturnsExpectedResult()
    {
        Assert.Equal("TrafficSquare", RoadSegmentMorphology.TrafficSquare);
    }

    [Fact]
    public void TrafficSquareTranslationReturnsExpectedResult()
    {
        Assert.Equal(106, RoadSegmentMorphology.TrafficSquare.Translation.Identifier);
    }

    [Fact]
    public void Tramway_not_accessible_to_other_vehiclesReturnsExpectedResult()
    {
        Assert.Equal("Tramway_not_accessible_to_other_vehicles", RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles);
    }

    [Fact]
    public void Tramway_not_accessible_to_other_vehiclesTranslationReturnsExpectedResult()
    {
        Assert.Equal(116, RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles.Translation.Identifier);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsUnknown()
    {
        var value = _fixture.Create<string>();
        var result = RoadSegmentMorphology.TryParse(value, out var parsed);
        Assert.False(result);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
    {
        var value = _knownValues[new Random().Next(0, _knownValues.Length)];
        var result = RoadSegmentMorphology.TryParse(value, out var parsed);
        Assert.True(result);
        Assert.NotNull(parsed);
        Assert.Equal(value, parsed.ToString());
    }

    [Fact]
    public void TryParseValueCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => RoadSegmentMorphology.TryParse(null, out _));
    }

    [Fact]
    public void UnknownReturnsExpectedResult()
    {
        Assert.Equal("Unknown", RoadSegmentMorphology.Unknown);
    }

    [Fact]
    public void UnknownTranslationReturnsExpectedResult()
    {
        Assert.Equal(-8, RoadSegmentMorphology.Unknown.Translation.Identifier);
    }

    [Fact]
    public void VerifyBehavior()
    {
        _fixture.Customizations.Add(
            new FiniteSequenceGenerator<string>(_knownValues));
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<string?>(_fixture),
            new EquatableEqualsSelfAssertion(_fixture),
            new EquatableEqualsOtherAssertion(_fixture),
            new EqualityOperatorEqualsSelfAssertion(_fixture),
            new EqualityOperatorEqualsOtherAssertion(_fixture),
            new InequalityOperatorEqualsSelfAssertion(_fixture),
            new InequalityOperatorEqualsOtherAssertion(_fixture),
            new EqualsNewObjectAssertion(_fixture),
            new EqualsNullAssertion(_fixture),
            new EqualsSelfAssertion(_fixture),
            new EqualsOtherAssertion(_fixture),
            new EqualsSuccessiveAssertion(_fixture),
            new GetHashCodeSuccessiveAssertion(_fixture)
        ).Verify(typeof(RoadSegmentMorphology));
    }

    [Fact]
    public void Walking_or_cycling_path_not_accessible_to_other_vehiclesReturnsExpectedResult()
    {
        Assert.Equal("Walking_or_cycling_path_not_accessible_to_other_vehicles", RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles);
    }

    [Fact]
    public void Walking_or_cycling_path_not_accessible_to_other_vehiclesTranslationReturnsExpectedResult()
    {
        Assert.Equal(114, RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles.Translation.Identifier);
    }
}
