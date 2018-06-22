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
            _fixture.CustomizeDbaseString();
            _fixture.CustomizeDbaseField();
        }

        [Fact]
        public void ConstructionUsingFieldsHasExpectedResult()
        {
            var fields = _fixture.CreateMany<DbaseField>().ToArray();
            var expectedValues = Array.ConvertAll(fields, field => field.CreateFieldValue());

            var sut = new AnonymousDbaseRecord(fields);

            Assert.False(sut.IsDeleted);
            Assert.Equal(expectedValues, sut.Values, new DbaseFieldValueEqualityComparer());
        }

        [Fact]
        public void ConstructionUsingValuesHasExpectedResult()
        {
            var fields = _fixture.CreateMany<DbaseField>().ToArray();
            var values = Array.ConvertAll(fields, field => field.CreateFieldValue());

            var sut = new AnonymousDbaseRecord(values);

            Assert.False(sut.IsDeleted);
            Assert.Same(values, sut.Values);
        }

        [Fact(Skip = "Needs work")]
        public void ReadingCharacterWithDateTimeValueHasExpectedResult()
        {
            var fields = _fixture.CreateMany<DbaseField>().ToArray();
            fields[0] = new DbaseField(
                fields[0].Name,
                DbaseFieldType.Character,
                new ByteOffset(0),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)
            );
            var expectedValues = Array.ConvertAll(fields, field => field.CreateFieldValue());

            var sut = new AnonymousDbaseRecord(fields);

            Assert.False(sut.IsDeleted);
            Assert.Equal(expectedValues, sut.Values, new DbaseFieldValueEqualityComparer());
        }
    }
}
