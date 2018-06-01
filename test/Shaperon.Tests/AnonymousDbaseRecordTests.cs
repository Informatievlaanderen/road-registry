namespace Shaperon
{
    using System;
    using System.Linq;
    using AutoFixture;
    using Xunit;

    public class AnonymousDbaseRecordTests
    {
        private readonly Fixture _fixture;

        public AnonymousDbaseRecordTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseFieldName();
            _fixture.CustomizeDbaseFieldLength();
            _fixture.CustomizeDbaseDecimalCount();
            _fixture.CustomizeDbaseInt32();
            _fixture.CustomizeDbaseDouble();
            _fixture.CustomizeDbaseString();
            _fixture.CustomizeDbaseDateTime();
        }

        [Fact]
        public void ConstructionHasExpectedResult()
        {
            var fields = _fixture.CreateMany<DbaseField>().ToArray();
            var expectedValues = Array.ConvertAll(fields, field => field.CreateFieldValue());

            var sut = new AnonymousDbaseRecord(fields);

            Assert.False(sut.IsDeleted);
            Assert.Equal(expectedValues, sut.Values, new DbaseFieldValueEqualityComparer());
        }
    }
}
