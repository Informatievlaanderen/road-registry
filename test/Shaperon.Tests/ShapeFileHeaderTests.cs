namespace Shaperon
{
    using AutoFixture;
    using Albedo;
    using AutoFixture.Idioms;
    using Xunit;
    using System.IO;
    using System.Text;
    using System;

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
        public void ReadExpectsHeaderToStartWith9994()
        {
            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.WriteInt32BigEndian(1234); // start

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
            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.WriteInt32BigEndian(9994);
                    for(var index = 0; index < 20; index++)
                    {
                        writer.Write((byte)0x0);
                    }
                    writer.WriteInt32BigEndian(_fixture.Create<int>());
                    writer.WriteInt32LittleEndian(999); // version
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
            using(var stream = new MemoryStream())
            {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    writer.WriteInt32BigEndian(9994);
                    for(var index = 0; index < 20; index++)
                    {
                        writer.Write((byte)0x0);
                    }
                    writer.WriteInt32BigEndian(_fixture.Create<int>());
                    writer.WriteInt32LittleEndian(1000);
                    writer.WriteInt32LittleEndian(-1); // shape type
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
