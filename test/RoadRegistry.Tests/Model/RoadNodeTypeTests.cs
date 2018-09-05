namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadNodeTypeTests
    {
        private readonly Fixture _fixture;
        private readonly int[] _knownValues;

        public RoadNodeTypeTests()
        {
            _fixture = new Fixture();
            _knownValues = Array.ConvertAll(RoadNodeType.All, type => type.ToInt32());
        }

        [Fact]
        public void VerifyBehavior()
        {
            _fixture.Customizations.Add(
                new RandomNumericSequenceGenerator(_knownValues.Min(), _knownValues.Max()));
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<Int32>(_fixture),
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
            ).Verify(typeof(RoadNodeType));
        }

        [Fact]
        public static void RealNodeReturnsExpectedResult()
        {
            Assert.Equal(1, RoadNodeType.RealNode);
        }

        [Fact]
        public static void FakeNodeReturnsExpectedResult()
        {
            Assert.Equal(2, RoadNodeType.FakeNode);
        }

        [Fact]
        public static void EndNodeReturnsExpectedResult()
        {
            Assert.Equal(3, RoadNodeType.EndNode);
        }

        [Fact]
        public static void MiniRoundaboutReturnsExpectedResult()
        {
            Assert.Equal(4, RoadNodeType.MiniRoundabout);
        }

        [Fact]
        public static void TurnLoopNodeReturnsExpectedResult()
        {
            Assert.Equal(5, RoadNodeType.TurnLoopNode);
        }

        [Fact]
        public static void AllReturnsExpectedResult()
        {
            Assert.Equal(
                new []
                {
                    RoadNodeType.RealNode,
                    RoadNodeType.FakeNode,
                    RoadNodeType.EndNode,
                    RoadNodeType.MiniRoundabout,
                    RoadNodeType.TurnLoopNode,
                },
                RoadNodeType.All);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            _fixture.Customizations.Add(
                new RandomNumericSequenceGenerator(_knownValues.Min(), _knownValues.Max()));

            var value = _fixture.Create<int>();
            var sut = RoadNodeType.Parse(value);
            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            Assert.NotNull(RoadNodeType.Parse(value));
        }

        [Fact]
        public void ParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            Assert.Throws<FormatException>(() => RoadNodeType.Parse(value));
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsWellKnown()
        {
            var value = new Generator<int>(_fixture).First(candidate => _knownValues.Contains(candidate));
            var result = RoadNodeType.TryParse(value, out RoadNodeType parsed);
            Assert.True(result);
            Assert.NotNull(parsed);
            Assert.Equal(value, parsed.ToInt32());
        }

        [Fact]
        public void TryParseReturnsExpectedResultWhenValueIsUnknown()
        {
            var value = new Generator<int>(_fixture).First(candidate => !_knownValues.Contains(candidate));
            var result = RoadNodeType.TryParse(value, out RoadNodeType parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }
    }
}
