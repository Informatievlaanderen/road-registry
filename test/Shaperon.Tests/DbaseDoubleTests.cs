namespace Shaperon
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class DbaseDoubleTests
    {
        private readonly Fixture _fixture;

        public DbaseDoubleTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseFieldName();
            _fixture.CustomizeDbaseFieldLength();
            _fixture.CustomizeDbaseDecimalCount();
            _fixture.CustomizeDbaseDouble();
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void CreateFailsIfFieldIsNotNumber()
        {
            var fieldType = new Generator<DbaseFieldType>(_fixture)
                .First(specimen => specimen != DbaseFieldType.Number);
            var length = _fixture.GenerateDbaseDoubleLength();
            var decimalCount = _fixture.GenerateDbaseDoubleDecimalCount(length);
            Assert.Throws<ArgumentException>(
                () =>
                    new DbaseDouble(
                        new DbaseField(
                            _fixture.Create<DbaseFieldName>(),
                            fieldType,
                            _fixture.Create<ByteOffset>(),
                            length,
                            decimalCount
                        )
                    )
            );
        }

        [Fact]
        public void IsDbaseFieldValue()
        {
            Assert.IsAssignableFrom<DbaseFieldValue>(_fixture.Create<DbaseDouble>());
        }

        [Fact]
        public void ReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<DbaseDouble>().Select(instance => instance.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<DbaseDouble>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void LengthOfValueBeingSetCanNotExceedFieldLength()
        {
            var maxLength =
                Math.Max(
                    Double.MaxValue.ToString(CultureInfo.InvariantCulture).Length,
                    Double.MinValue.ToString(CultureInfo.InvariantCulture).Length
                );
            var length = new Generator<int>(_fixture)
                .Where(specimen => specimen < maxLength)
                .Select(_ => new DbaseFieldLength(_))
                .First();
            var decimalCount = _fixture.GenerateDbaseDoubleDecimalCount(length);

            var sut =
                new DbaseDouble(
                    new DbaseField(
                        _fixture.Create<DbaseFieldName>(),
                        DbaseFieldType.Number,
                        _fixture.Create<ByteOffset>(),
                        length,
                        decimalCount
                    )
                );

            var value = Enumerable
                .Range(0, sut.Field.Length)
                .Aggregate(1d, (current, _) => current * 10d);

            Assert.Throws<ArgumentException>(() => sut.Value = value);
        }

        [Fact]
        public void LengthOfNegativeValueBeingSetCanNotExceedFieldLength()
        {
            var maxLength =
                Math.Max(
                    Double.MaxValue.ToString(CultureInfo.InvariantCulture).Length,
                    Double.MinValue.ToString(CultureInfo.InvariantCulture).Length
                );
            var length = new Generator<int>(_fixture)
                .Where(specimen => specimen < maxLength)
                .Select(_ => new DbaseFieldLength(_))
                .First();
            var decimalCount = _fixture.GenerateDbaseDoubleDecimalCount(length);

            var sut =
                new DbaseDouble(
                    new DbaseField(
                        _fixture.Create<DbaseFieldName>(),
                        DbaseFieldType.Number,
                        _fixture.Create<ByteOffset>(),
                        length,
                        decimalCount
                    )
                );

            var value = Enumerable
                .Range(0, sut.Field.Length)
                .Aggregate(-1d, (current, _) => current * 10d);

            Assert.Throws<ArgumentException>(() => sut.Value = value);
        }

        [Fact]
        public void CanReadWriteNull()
        {
            var sut = _fixture.Create<DbaseDouble>();
            sut.Value = null;

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    sut.Write(writer);
                    writer.Flush();
                }

                stream.Position = 0;

                using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var result = new DbaseDouble(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }

        [Fact]
        public void CanReadWriteNegative()
        {
            var value = Math.Abs(_fixture.Create<double>()) * -1d;
            var sut = _fixture.Create<DbaseDouble>();
            sut.Value = value;

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    sut.Write(writer);
                    writer.Flush();
                }

                stream.Position = 0;

                using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var result = new DbaseDouble(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }


        [Fact(Skip = "Generated value exceeds field length")]
        public void CanReadWriteWithMaxDecimalCount()
        {
            var length = _fixture.GenerateDbaseDoubleLength();
            // var generator = new Generator<double>(_fixture)
            //     .First(value => value.ToString())
            var decimalCount = new DbaseDecimalCount(length - 2);
            var sut =
                new DbaseDouble(
                    new DbaseField(
                        _fixture.Create<DbaseFieldName>(),
                        DbaseFieldType.Number,
                        _fixture.Create<ByteOffset>(),
                        length,
                        decimalCount
                    ),
                    _fixture.Create<double>()
                );

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    sut.Write(writer);
                    writer.Flush();
                }

                stream.Position = 0;

                using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var result = new DbaseDouble(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }

        [Fact]
        public void CanReadWrite()
        {
            // ToDo: fix the flickering test!
            // fails
            // when (0 < value < 1) && Field.DecimalCount >= Field.Length
            // or
            // when (value > 0) && (Field.DecimalCount + InterPartLengthOf(value)) >= Field.Length
            // or
            // negative value ...

            var sut = _fixture.Create<DbaseDouble>();

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    sut.Write(writer);
                    writer.Flush();
                }

                stream.Position = 0;

                using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var result = new DbaseDouble(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }
    }
}
