namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;
    using System;
    using System.Linq;

    public class ShapeFileHeaderTests
    {
        private readonly Fixture _fixture;

        public ShapeFileHeaderTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeWordLength();
            _fixture.Register(() => new BinaryReader(new MemoryStream()));
            _fixture.Register(() => new BinaryWriter(new MemoryStream()));
        }

        [Fact]
        public void ReaderCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(Methods.Select(() => ShapeFileHeader.Read(null)));
        }

        [Fact]
        public void WriterCanNotBeNull()
        {
            new GuardClauseAssertion(_fixture)
                .Verify(new Methods<ShapeFileHeader>().Select(instance => instance.Write(null)));
        }

        [Fact]
        public void CanReadWrite()
        {
            var sut = new ShapeFileHeader(
                _fixture.Create<WordLength>(),
                _fixture.Create<ShapeType>(),
                _fixture.Create<BoundingBox3D>());

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
                    var result = ShapeFileHeader.Read(reader);

                    Assert.Equal(sut.FileLength, result.FileLength);
                    Assert.Equal(sut.ShapeType, result.ShapeType);
                    Assert.Equal(sut.BoundingBox, result.BoundingBox);
                }
            }
        }

        [Fact]
        public void ReadExpectsHeaderToStartWithFileCode9994()
        {
            var start = _fixture.Create<Generator<int>>().Where(_ => _ != ShapeFileHeader.ExpectedFileCode).First();

            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.WriteInt32BigEndian(start);

                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    Assert.Throws<ShapeFileHeaderException>(
                        () => ShapeFileHeader.Read(reader)
                    );
                }
            }
        }

        [Fact]
        public void ReadExpectsHeaderVersionToBe1000()
        {
            var version = _fixture.Create<Generator<int>>().Where(_ => _ != ShapeFileHeader.ExpectedVersion).First();

            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.WriteInt32BigEndian(ShapeFileHeader.ExpectedFileCode);
                    for(var index = 0; index < 20; index++)
                    {
                        writer.Write((byte)0x0);
                    }
                    writer.WriteInt32BigEndian(_fixture.Create<int>());
                    writer.WriteInt32LittleEndian(version);
                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    Assert.Throws<ShapeFileHeaderException>(
                        () => ShapeFileHeader.Read(reader)
                    );
                }
            }
        }

        [Fact]
        public void ReadExpectsHeaderShapeTypeToBeValid()
        {
            var shapeType = _fixture.Create<Generator<int>>().Where(_ => !Enum.IsDefined(typeof(ShapeType), _)).First();

            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.WriteInt32BigEndian(ShapeFileHeader.ExpectedFileCode);
                    for(var index = 0; index < 20; index++)
                    {
                        writer.Write((byte)0x0);
                    }
                    writer.WriteInt32BigEndian(_fixture.Create<int>());
                    writer.WriteInt32LittleEndian(ShapeFileHeader.ExpectedVersion);
                    writer.WriteInt32LittleEndian(shapeType);
                    writer.Flush();
                }

                stream.Position = 0;

                using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    Assert.Throws<ShapeFileHeaderException>(
                        () => ShapeFileHeader.Read(reader)
                    );
                }
            }
        }
    }
}
