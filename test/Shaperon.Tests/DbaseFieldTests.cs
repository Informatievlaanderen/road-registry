namespace Shaperon
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class DbaseFieldTests
    {
        private readonly Fixture _fixture;

        public DbaseFieldTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseFieldName();
            _fixture.CustomizeByteOffset();
            _fixture.CustomizeDbaseFieldLength();
            _fixture.CustomizeDbaseDecimalCount();
            _fixture.CustomizeDbaseField();
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
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
            ).Verify(typeof(DbaseField));
        }

        // Field type, length, decimal count validation related tests

        [Fact]
        public void CreateCharacterFieldFailsWhenDecimalCountIsNot0()
        {
            var decimalCount = new Generator<DbaseDecimalCount>(_fixture)
                .First(specimen => specimen.ToInt32() != 0);

            Assert.Throws<ArgumentException>(() =>
                new DbaseField(
                    _fixture.Create<DbaseFieldName>(),
                    DbaseFieldType.Character,
                    _fixture.Create<ByteOffset>(),
                    _fixture.Create<DbaseFieldLength>(),
                    decimalCount));
        }

        [Fact]
        public void CreateNumberFieldFailsWhenDecimalCountIsNotZeroAndNotLessThanLengthMinus2()
        {
            var length = _fixture.Create<DbaseFieldLength>();
            var decimalCount = new Generator<DbaseDecimalCount>(_fixture)
                .First(specimen => specimen.ToInt32() >= length.ToInt32() - 2);
            Assert.Throws<ArgumentException>(() =>
                new DbaseField(
                    _fixture.Create<DbaseFieldName>(),
                    DbaseFieldType.Number,
                    _fixture.Create<ByteOffset>(),
                    length,
                    decimalCount));
        }

        [Fact]
        public void CreateNumberFieldReturnsExpectedResultWhenDecimalCountIsZeroAndNotLessThanLengthMinus2()
        {
            var name = _fixture.Create<DbaseFieldName>();
            var offset = _fixture.Create<ByteOffset>();
            //var length = _fixture.Create<DbaseFieldLength>();
            var length = new DbaseFieldLength(1);
            var decimalCount = new DbaseDecimalCount(0);
            var result =
                new DbaseField(
                    name,
                    DbaseFieldType.Number,
                    offset,
                    length,
                    decimalCount);

            Assert.Equal(name, result.Name);
            Assert.Equal(DbaseFieldType.Number, result.FieldType);
            Assert.Equal(offset, result.Offset);
            Assert.Equal(length, result.Length);
            Assert.Equal(decimalCount, result.DecimalCount);
        }

        [Fact]
        public void CreateDateTimeFieldFailsWhenLengthIsNot15()
        {
            var length = new Generator<DbaseFieldLength>(_fixture)
                .First(specimen => specimen.ToInt32() != 15);
            Assert.Throws<ArgumentException>(() =>
                new DbaseField(
                    _fixture.Create<DbaseFieldName>(),
                    DbaseFieldType.DateTime,
                    _fixture.Create<ByteOffset>(),
                    length,
                    new DbaseDecimalCount(0)));
        }

        [Fact]
        public void CreateDateTimeFieldFailsWhenDecimalCountIsNot0()
        {
            var decimalCount = new Generator<DbaseDecimalCount>(_fixture)
                .First(specimen => specimen.ToInt32() != 0);
            Assert.Throws<ArgumentException>(() =>
                new DbaseField(
                    _fixture.Create<DbaseFieldName>(),
                    DbaseFieldType.DateTime,
                    _fixture.Create<ByteOffset>(),
                    new DbaseFieldLength(15),
                    decimalCount));
        }

        // Factory method tests

        [Fact]
        public void CreateStringFieldReturnsExpectedResult()
        {
            var name = _fixture.Create<DbaseFieldName>();
            var offset =_fixture.Create<ByteOffset>();
            var length = _fixture.Create<DbaseFieldLength>();

            var result = DbaseField.CreateStringField(
                name,
                offset,
                length);

            Assert.Equal(name, result.Name);
            Assert.Equal(DbaseFieldType.Character, result.FieldType);
            Assert.Equal(offset, result.Offset);
            Assert.Equal(length, result.Length);
            Assert.Equal(new DbaseDecimalCount(0), result.DecimalCount);
        }


        [Fact]
        public void CreateInt32FieldReturnsExpectedResult()
        {
            var name = _fixture.Create<DbaseFieldName>();
            var offset =_fixture.Create<ByteOffset>();
            var length = _fixture.Create<DbaseFieldLength>();

            var result = DbaseField.CreateInt32Field(
                name,
                offset,
                length);

            Assert.Equal(name, result.Name);
            Assert.Equal(DbaseFieldType.Number, result.FieldType);
            Assert.Equal(offset, result.Offset);
            Assert.Equal(length, result.Length);
            Assert.Equal(new DbaseDecimalCount(0), result.DecimalCount);
        }

        [Fact]
        public void CreateDateTimeFieldReturnsExpectedResult()
        {
            var name = _fixture.Create<DbaseFieldName>();
            var offset =_fixture.Create<ByteOffset>();

            var result = DbaseField.CreateDateTimeField(
                name,
                offset);

            Assert.Equal(name, result.Name);
            Assert.Equal(DbaseFieldType.DateTime, result.FieldType);
            Assert.Equal(offset, result.Offset);
            Assert.Equal(new DbaseFieldLength(15), result.Length);
            Assert.Equal(new DbaseDecimalCount(0), result.DecimalCount);
        }

        [Fact]
        public void CreateDoubleFieldReturnsExpectedResult()
        {
            var name = _fixture.Create<DbaseFieldName>();
            var offset =_fixture.Create<ByteOffset>();
            var length = _fixture.GenerateDbaseDoubleLength();
            var decimalCount = _fixture.GenerateDbaseDoubleDecimalCount(length);

            var result = DbaseField.CreateDoubleField(
                name,
                offset,
                length,
                decimalCount);

            Assert.Equal(name, result.Name);
            Assert.Equal(DbaseFieldType.Number, result.FieldType);
            Assert.Equal(offset, result.Offset);
            Assert.Equal(length, result.Length);
            Assert.Equal(decimalCount, result.DecimalCount);
        }

        [Fact]
        public void CreateDoubleFieldThrowsWhenDecimalCountGreaterThanOrEqualToLength()
        {
            var name = _fixture.Create<DbaseFieldName>();
            var offset =_fixture.Create<ByteOffset>();
            var length = _fixture.Create<DbaseFieldLength>();
            var decimalCount = new Generator<DbaseDecimalCount>(_fixture)
                .First(specimen => specimen.ToInt32() >= length.ToInt32());

            Assert.Throws<ArgumentException>(() => DbaseField.CreateDoubleField(
                name,
                offset,
                length,
                decimalCount));
        }

        // Field value factory related tests

        [Fact]
        public void CreateCharacterFieldValueReturnsExpectedResult()
        {
            var sut = new DbaseField(
                _fixture.Create<DbaseFieldName>(),
                DbaseFieldType.Character,
                _fixture.Create<ByteOffset>(),
                _fixture.Create<DbaseFieldLength>(),
                new DbaseDecimalCount(0));

            var result = sut.CreateFieldValue();

            Assert.Equal(sut, result.Field);
            Assert.IsType<DbaseString>(result);
        }

        [Fact]
        public void CreateNumberFieldValueReturnsExpectedResult()
        {
            var length = _fixture.GenerateDbaseDoubleLength();
            var decimalCount = _fixture.GenerateDbaseDoubleDecimalCount(length);
            var sut = new DbaseField(
                _fixture.Create<DbaseFieldName>(),
                DbaseFieldType.Number,
                _fixture.Create<ByteOffset>(),
                length,
                decimalCount);

            var result = sut.CreateFieldValue();

            Assert.Equal(sut, result.Field);
            if(sut.DecimalCount == 0)
            {
                Assert.IsType<DbaseInt32>(result);
            }
            else
            {
                Assert.IsType<DbaseDouble>(result);
            }
        }

        [Fact]
        public void CreateDateTimeFieldValueReturnsExpectedResult()
        {
            var sut = new DbaseField(
                _fixture.Create<DbaseFieldName>(),
                DbaseFieldType.DateTime,
                _fixture.Create<ByteOffset>(),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            var result = sut.CreateFieldValue();

            Assert.Equal(sut, result.Field);
            Assert.IsType<DbaseDateTime>(result);
        }

        [Fact]
        public void ReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => DbaseField.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<DbaseField>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void CanReadWrite()
        {
            var sut = _fixture.Create<DbaseField>();

            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    sut.Write(writer);
                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var result = DbaseField.Read(reader);

                    Assert.Equal(sut, result);
                }
            }
        }
    }
}
