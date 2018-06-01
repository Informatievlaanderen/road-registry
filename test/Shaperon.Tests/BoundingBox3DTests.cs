namespace Shaperon
{
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class BoundingBox3DTests
    {
        [Fact]
        public void VerifyEquality()
        {
            var fixture = new Fixture();
            new CompositeIdiomaticAssertion(
                new EqualsNewObjectAssertion(fixture),
                new EqualsNullAssertion(fixture),
                new EqualsSelfAssertion(fixture),
                new EqualsSuccessiveAssertion(fixture),
                new GetHashCodeSuccessiveAssertion(fixture)
            ).Verify(typeof(BoundingBox3D));
        }
    }
}
