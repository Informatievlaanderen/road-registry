namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using Xunit;

    public class ZipArchiveShapeEntryValidatorTests
    {
        private readonly Fixture _fixture;

        public ZipArchiveShapeEntryValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<NetTopologySuite.Geometries.Point>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.Point(
                        _fixture.Create<double>(),
                        _fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<RecordNumber>(customizer =>
                customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));
            _fixture.Customize<ShapeRecord>(customization =>
                customization.FromFactory(random =>
                    new PointShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(_fixture.Create<NetTopologySuite.Geometries.Point>())).RecordAs(_fixture.Create<RecordNumber>())
                ).OmitAutoProperties()
            );
        }

        [Fact]
        public void EncodingCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipArchiveShapeEntryValidator(
                    null,
                    new FakeShapeRecordValidator()));
        }

        [Fact]
        public void ValidatorCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipArchiveShapeEntryValidator(
                    Encoding.Default,
                    null));
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            var sut = new ZipArchiveShapeEntryValidator(
                Encoding.Default,
                new FakeShapeRecordValidator());

            Assert.Throws<ArgumentNullException>(() => sut.Validate(null));
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenEntryStreamIsEmpty()
        {
            var sut = new ZipArchiveShapeEntryValidator(
                Encoding.Default,
                new FakeShapeRecordValidator());

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry("entry");
                }

                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(
                        ZipArchiveProblems.Single(entry.HasShapeHeaderFormatError(
                            new EndOfStreamException("Unable to read beyond the end of the stream."))
                        ),
                        result,
                        new FileProblemComparer());
                }
            }
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenEntryStreamContainsMalformedShapeHeader()
        {
            var sut = new ZipArchiveShapeEntryValidator(
                Encoding.Default,
                new FakeShapeRecordValidator());

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using (var entryStream = entry.Open())
                    {
                        entryStream.WriteByte(0);
                        entryStream.WriteByte(0);
                        entryStream.WriteByte(0);
                        entryStream.WriteByte(0);
                        entryStream.Flush();
                    }
                }

                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(
                        ZipArchiveProblems.Single(entry.HasShapeHeaderFormatError(
                            new ShapeFileHeaderException("The File Code field does not match 9994."))
                        ),
                        result,
                        new FileProblemComparer());
                }
            }
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenShapeRecordValidatorReturnsErrors()
        {
            var problems = new FileProblem[]
            {
                new FileError("file1", "error1", new ProblemParameter("parameter1", "value1")),
                new FileWarning("file2", "error2", new ProblemParameter("parameter2", "value2"))
            };
            var sut = new ZipArchiveShapeEntryValidator(
                Encoding.UTF8,
                new FakeShapeRecordValidator(problems));
            var header = new ShapeFileHeader(
                ShapeFileHeader.Length,
                ShapeType.Point,
                BoundingBox3D.Empty);

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using (var entryStream = entry.Open())
                    using (var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                    {
                        header.Write(writer);
                        entryStream.Flush();
                    }
                }

                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(
                        ZipArchiveProblems.None.AddRange(problems),
                        result);
                }
            }
        }

        [Fact]
        public void ValidatePassesExpectedShapeRecordsToShapeRecordValidator()
        {
            var validator = new CollectShapeRecordValidator();
            var sut = new ZipArchiveShapeEntryValidator(Encoding.UTF8, validator);
            var records = _fixture.CreateMany<ShapeRecord>(2).ToArray();
            var fileSize = records.Aggregate(ShapeFileHeader.Length, (length, record) => length.Plus(record.Length));
            var header = new ShapeFileHeader(
                fileSize,
                ShapeType.Point,
                BoundingBox3D.Empty);

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using (var entryStream = entry.Open())
                    using (var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                    {
                        header.Write(writer);
                        foreach (var record in records)
                        {
                            record.Write(writer);
                        }

                        entryStream.Flush();
                    }
                }

                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(ZipArchiveProblems.None, result);
                    Assert.Equal(records, validator.Collected, new ShapeRecordEqualityComparer());
                }
            }
        }

        private class FakeShapeRecordValidator : IZipArchiveShapeRecordsValidator
        {
            private readonly FileProblem[] _problems;

            public FakeShapeRecordValidator(params FileProblem[] problems)
            {
                _problems = problems ?? throw new ArgumentNullException(nameof(problems));
            }

            public ZipArchiveProblems Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records)
            {
                return ZipArchiveProblems.None.AddRange(_problems);
            }
        }

        private class CollectShapeRecordValidator : IZipArchiveShapeRecordsValidator
        {
            public ZipArchiveProblems Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records)
            {
                var collected = new List<ShapeRecord>();
                while (records.MoveNext())
                {
                    collected.Add(records.Current);
                }

                Collected = collected.ToArray();

                return ZipArchiveProblems.None;
            }

            public ShapeRecord[] Collected { get; private set; }
        }
    }
}
