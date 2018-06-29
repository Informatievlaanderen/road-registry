namespace Shaperon
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class AnonymousDbaseSchemaTests
    {
        private readonly Fixture _fixture;

        public AnonymousDbaseSchemaTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseFieldName();
            _fixture.CustomizeDbaseFieldLength();
            _fixture.CustomizeDbaseDecimalCount();
            _fixture.CustomizeDbaseField();
        }

        [Fact]
        public void MaximumFieldCountHasExpectedValue()
        {
            Assert.Equal(128, DbaseSchema.MaximumFieldCount);
        }

        [Fact]
        public void FieldsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnonymousDbaseSchema(null));
        }

        [Fact]
        public void FieldsCanNotExceedMaximumFieldCount()
        {
            var fieldCount = new Generator<int>(_fixture).First(specimen => specimen > DbaseSchema.MaximumFieldCount && specimen < DbaseSchema.MaximumFieldCount * 2);
            var fields = _fixture.CreateMany<DbaseField>(fieldCount).ToArray();
            
            Assert.Throws<ArgumentException>(() => new AnonymousDbaseSchema(fields));
        }

        [Fact]
        public void ConstructionUsingFieldsHasExpectedResult()
        {
            var fieldCount = new Generator<int>(_fixture).First(specimen => specimen < DbaseSchema.MaximumFieldCount);
            var fields = _fixture.CreateMany<DbaseField>(fieldCount).ToArray();
            var length = fields.Aggregate(DbaseRecordLength.Initial, (current, field) => current.Plus(field.Length));

            var sut = new AnonymousDbaseSchema(fields);

            Assert.Equal(fields, sut.Fields);
            Assert.Equal(length, sut.Length);
        }

        [Fact]
        public void VerifyEquality()
        {
            new CompositeIdiomaticAssertion(
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(AnonymousDbaseSchema));
        }

    }
}
