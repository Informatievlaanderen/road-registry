namespace Shaperon
{
    using System.IO;
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

        [Fact]
        public void CreateCharacterFieldValueReturnsExpectedResult()
        {
            var sut = new DbaseField(
                _fixture.Create<DbaseFieldName>(),
                DbaseFieldType.Character,
                _fixture.Create<ByteOffset>(),
                _fixture.Create<DbaseFieldLength>(),
                _fixture.Create<DbaseDecimalCount>());

            var result = sut.CreateFieldValue();

            Assert.Equal(sut, result.Field);
            Assert.IsType<DbaseString>(result);
        }

        [Fact]
        public void CreateNumberFieldValueReturnsExpectedResult()
        {
            var sut = new DbaseField(
                _fixture.Create<DbaseFieldName>(),
                DbaseFieldType.Number,
                _fixture.Create<ByteOffset>(),
                _fixture.Create<DbaseFieldLength>(),
                _fixture.Create<DbaseDecimalCount>());

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
                _fixture.Create<DbaseFieldLength>(),
                _fixture.Create<DbaseDecimalCount>());

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
            var sut = new DbaseField(
                _fixture.Create<DbaseFieldName>(),
                _fixture.Create<DbaseFieldType>(),
                _fixture.Create<ByteOffset>(),
                _fixture.Create<DbaseFieldLength>(),
                _fixture.Create<DbaseDecimalCount>());

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

                    Assert.Equal(sut.Name, result.Name);
                    Assert.Equal(sut.Length, result.Length);
                    Assert.Equal(sut.FieldType, result.FieldType);
                    Assert.Equal(sut.DecimalCount, result.DecimalCount);
                }
            }
        }
    }
}
