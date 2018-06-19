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

    public class DbaseSingleTests
    {
        private readonly Fixture _fixture;

        public DbaseSingleTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseFieldName();
            _fixture.CustomizeDbaseFieldLength();
            _fixture.CustomizeDbaseDecimalCount();
            _fixture.CustomizeDbaseSingle();
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void CreateFailsIfFieldIsNotFloat()
        {
            var fieldType = new Generator<DbaseFieldType>(_fixture)
                .First(specimen => specimen != DbaseFieldType.Float);
            var length = _fixture.GenerateDbaseSingleLength();
            var decimalCount = _fixture.GenerateDbaseSingleDecimalCount(length);
            Assert.Throws<ArgumentException>(
                () =>
                    new DbaseSingle(
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
            Assert.IsAssignableFrom<DbaseFieldValue>(_fixture.Create<DbaseSingle>());
        }

        [Fact]
        public void ReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<DbaseSingle>().Select(instance => instance.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<DbaseSingle>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void LengthOfValueBeingSetCanNotExceedFieldLength()
        {
            var maxLength = new DbaseFieldLength(
                Single.MaxValue.ToString(CultureInfo.InvariantCulture).Length - 1
                // because it's impossible to create a value longer than this (we need the test to generate a longer value)
            );
            var length = _fixture.GenerateDbaseSingleLengthLessThan(maxLength);
            var decimalCount = _fixture.GenerateDbaseSingleDecimalCount(length);

            var sut =
                new DbaseSingle(
                    new DbaseField(
                        _fixture.Create<DbaseFieldName>(),
                        DbaseFieldType.Float,
                        _fixture.Create<ByteOffset>(),
                        length,
                        decimalCount
                    )
                );

            var value = Enumerable
                .Range(0, sut.Field.Length)
                .Aggregate(1f, (current, _) => current * 10f);

            Assert.Throws<ArgumentException>(() => sut.Value = value);
        }

        [Fact]
        public void LengthOfNegativeValueBeingSetCanNotExceedFieldLength()
        {
            var maxLength = new DbaseFieldLength(
                Single.MinValue.ToString(CultureInfo.InvariantCulture).Length - 1
                // because it's impossible to create a value longer than this (we need the test to generate a longer value)
            );
            var length = _fixture.GenerateDbaseSingleLengthLessThan(maxLength);
            var decimalCount = _fixture.GenerateDbaseSingleDecimalCount(length);

            var sut =
                new DbaseSingle(
                    new DbaseField(
                        _fixture.Create<DbaseFieldName>(),
                        DbaseFieldType.Float,
                        _fixture.Create<ByteOffset>(),
                        length,
                        decimalCount
                    )
                );

            var value = Enumerable
                .Range(0, sut.Field.Length)
                .Aggregate(-1f, (current, _) => current * 10f);

            Assert.Throws<ArgumentException>(() => sut.Value = value);
        }

        [Fact]
        public void CanReadWriteNull()
        {
            var sut = _fixture.Create<DbaseSingle>();
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
                    var result = new DbaseSingle(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }

        [Fact]
        public void CanReadWriteNegative()
        {
            var value = Math.Abs(_fixture.Create<float>()) * -1f;
            var sut = _fixture.Create<DbaseSingle>();
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
                    var result = new DbaseSingle(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }


        [Fact]
        public void CanReadWriteWithMaxDecimalCount()
        {
            var length = _fixture.GenerateDbaseSingleLength();
            var decimalCount = new DbaseDecimalCount(length - 2);
            var sut =
                new DbaseSingle(
                    new DbaseField(
                        _fixture.Create<DbaseFieldName>(),
                        DbaseFieldType.Float,
                        _fixture.Create<ByteOffset>(),
                        length,
                        decimalCount
                    )
                );
            sut.Value = Convert.ToSingle(_fixture.Create<int>()) + _fixture.Create<float>();

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
                    var result = new DbaseSingle(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }

        [Fact]
        public void CanReadWrite()
        {
            var sut = _fixture.Create<DbaseSingle>();

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
                    var result = new DbaseSingle(sut.Field);
                    result.Read(reader);

                    Assert.Equal(sut.Field, result.Field);
                    Assert.Equal(sut.Value, result.Value);
                }
            }
        }
    }
}
