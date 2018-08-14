namespace Shaperon
{
    using System;
    using System.Collections.Generic;
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
            _fixture.CustomizeAnonymousDbaseSchema();
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
            var fields = _fixture.GenerateDbaseFields(fieldCount);

            Assert.Throws<ArgumentException>(() => new AnonymousDbaseSchema(fields));
        }


        [Theory]
        [MemberData(nameof(FieldOffsetMismatchCases))]
        public void FieldsOffsetMustMatchFieldPosition(DbaseField[] fields)
        {
            Assert.Throws<ArgumentException>(() => new AnonymousDbaseSchema(fields));
        }

        public static IEnumerable<object[]> FieldOffsetMismatchCases
        {
            get {
                var fixture = new Fixture();
                fixture.CustomizeByteOffset();
                fixture.CustomizeDbaseFieldName();
                fixture.CustomizeDbaseFieldLength();
                fixture.CustomizeDbaseDecimalCount();
                fixture.CustomizeDbaseField();

                var count = new Generator<int>(fixture).First(specimen => specimen > 0 && specimen < DbaseSchema.MaximumFieldCount);
                var fields = fixture.GenerateDbaseFields(count);
                var offsetGenerator = new Generator<ByteOffset>(fixture);

                for(var index = 0; index < count; index++)
                {
                    var current = fields[index];
                    var mismatch = (DbaseField[])fields.Clone();
                    if (index == 0)
                    {
                        mismatch[index] = current.At(offsetGenerator.First(specimen => specimen != ByteOffset.Initial));
                    }
                    else
                    {
                        var previous = fields[index - 1];
                        mismatch[index] = current.At(offsetGenerator.First(specimen => specimen != previous.Offset.Plus(previous.Length)));
                    }
                    yield return new object[] { mismatch };
                }
            }
        }

        [Fact]
        public void ConstructionUsingFieldsHasExpectedResult()
        {
            var fields = _fixture.GenerateDbaseFields();
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
