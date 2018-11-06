namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentMorphologyTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadSegmentMorphologyTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadSegmentMorphology.All, type => type.ToInt32());
        }

        [Fact]
        public void VerifyBehavior()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<int>(_knownValues));
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<int>(_fixture),
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
        public void UnknownReturnsExpectedResult()
        {
            Assert.Equal(-8, RoadSegmentMorphology.Unknown);
        }

        [Fact]
        public void MotorwayReturnsExpectedResult()
        {
            Assert.Equal(101, RoadSegmentMorphology.Motorway);
        }

        [Fact]
        public void Road_with_separate_lanes_that_is_not_a_motorwayReturnsExpectedResult()
        {
            Assert.Equal(102, RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway);
        }

        [Fact]
        public void Road_consisting_of_one_roadwayReturnsExpectedResult()
        {
            Assert.Equal(103, RoadSegmentMorphology.Road_consisting_of_one_roadway);
        }

        [Fact]
        public void Traffic_circleReturnsExpectedResult()
        {
            Assert.Equal(104, RoadSegmentMorphology.Traffic_circle);
        }

        [Fact]
        public void Special_traffic_situationReturnsExpectedResult()
        {
            Assert.Equal(105, RoadSegmentMorphology.Special_traffic_situation);
        }

        [Fact]
        public void Traffic_squareReturnsExpectedResult()
        {
            Assert.Equal(106, RoadSegmentMorphology.Traffic_square);
        }

        [Fact]
        public void Entry_or_exit_ramp_belonging_to_a_grade_separated_junctionReturnsExpectedResult()
        {
            Assert.Equal(107, RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction);
        }

        [Fact]
        public void Entry_or_exit_ramp_belonging_to_a_level_junctionReturnsExpectedResult()
        {
            Assert.Equal(108, RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction);
        }

        [Fact]
        public void Parallel_roadReturnsExpectedResult()
        {
            Assert.Equal(109, RoadSegmentMorphology.Parallel_road);
        }

        [Fact]
        public void Frontage_roadReturnsExpectedResult()
        {
            Assert.Equal(110, RoadSegmentMorphology.Frontage_road);
        }

        [Fact]
        public void Entry_or_exit_of_a_car_parkReturnsExpectedResult()
        {
            Assert.Equal(111, RoadSegmentMorphology.Entry_or_exit_of_a_car_park);
        }

        [Fact]
        public void Entry_or_exit_of_a_serviceReturnsExpectedResult()
        {
            Assert.Equal(112, RoadSegmentMorphology.Entry_or_exit_of_a_service);
        }

        [Fact]
        public void Pedestrain_zoneReturnsExpectedResult()
        {
            Assert.Equal(113, RoadSegmentMorphology.Pedestrain_zone);
        }

        [Fact]
        public void Walking_or_cycling_path_not_accessible_to_other_vehiclesReturnsExpectedResult()
        {
            Assert.Equal(114, RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles);
        }

        [Fact]
        public void Tramway_not_accessible_to_other_vehiclesReturnsExpectedResult()
        {
            Assert.Equal(116, RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles);
        }
        
        [Fact]
        public void Service_roadReturnsExpectedResult()
        {
            Assert.Equal(120, RoadSegmentMorphology.Service_road);
        }
        
        [Fact]
        public void Primitive_roadReturnsExpectedResult()
        {
            Assert.Equal(125, RoadSegmentMorphology.Primitive_road);
        }
        [Fact]
        public void FerryReturnsExpectedResult()
        {
            Assert.Equal(130, RoadSegmentMorphology.Ferry);
        }

        [Fact]
        public void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadSegmentMorphology.Unknown,
                    RoadSegmentMorphology.Motorway,
                    RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway,
                    RoadSegmentMorphology.Road_consisting_of_one_roadway,
                    RoadSegmentMorphology.Traffic_circle,
                    RoadSegmentMorphology.Special_traffic_situation,
                    RoadSegmentMorphology.Traffic_square,
                    RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction,
                    RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction,
                    RoadSegmentMorphology.Parallel_road,
                    RoadSegmentMorphology.Frontage_road,
                    RoadSegmentMorphology.Entry_or_exit_of_a_car_park,
                    RoadSegmentMorphology.Entry_or_exit_of_a_service,
                    RoadSegmentMorphology.Pedestrain_zone,
                    RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles,
                    RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles,
                    RoadSegmentMorphology.Service_road,
                    RoadSegmentMorphology.Primitive_road,
                    RoadSegmentMorphology.Ferry
                },
                RoadSegmentMorphology.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var sut = RoadSegmentMorphology.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadSegmentMorphology.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadSegmentMorphology.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadSegmentMorphology.TryParse(value, out RoadSegmentMorphology parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadSegmentMorphology.TryParse(value, out RoadSegmentMorphology parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
