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
            _fixture.Customize<PointM>(customization =>
                customization.FromFactory(generator =>
                    new PointM(
                        _fixture.Create<double>(),
                        _fixture.Create<double>(),
                        _fixture.Create<double>(),
                        _fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<RecordNumber>(customizer =>
                customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));
            _fixture.Customize<ShapeRecord>(customization =>
                customization.FromFactory(random =>
                    new PointShapeContent(_fixture.Create<PointM>()).RecordAs(_fixture.Create<RecordNumber>())
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

        [Fact(Skip = "Waiting for a newer version of Shaperon.")]
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
                        ZipArchiveErrors.None.ShapeHeaderFormatError(
                            entry.Name,
                            new EndOfStreamException("Unable to read beyond the end of the stream.")),
                        result,
                        new ErrorComparer());
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
                        ZipArchiveErrors.None.ShapeHeaderFormatError(
                            entry.Name,
                            new ShapeFileHeaderException("The File Code field does not match 9994.")),
                        result,
                        new ErrorComparer());
                }
            }
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenShapeRecordValidatorReturnsErrors()
        {
            var errors = new[]
            {
                new Error("error1", new ProblemParameter("parameter1", "value1")),
                new Error("error2", new ProblemParameter("parameter2", "value2"))
            };
            var sut = new ZipArchiveShapeEntryValidator(
                Encoding.UTF8,
                new FakeShapeRecordValidator(errors));
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
                        ZipArchiveErrors.None.CombineWith(errors),
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

                    Assert.Equal(ZipArchiveErrors.None, result);
                    Assert.Equal(records, validator.Collected, new ShapeRecordEqualityComparer());
                }
            }
        }

        private class FakeShapeRecordValidator : IZipArchiveShapeRecordsValidator
        {
            private readonly Error[] _errors;

            public FakeShapeRecordValidator(params Error[] errors)
            {
                _errors = errors ?? throw new ArgumentNullException(nameof(errors));
            }

            public ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records)
            {
                return ZipArchiveErrors.None.CombineWith(_errors);
            }
        }

        private class CollectShapeRecordValidator : IZipArchiveShapeRecordsValidator
        {
            public ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records)
            {
                var collected = new List<ShapeRecord>();
                while (records.MoveNext())
                {
                    collected.Add(records.Current);
                }

                Collected = collected.ToArray();

                return ZipArchiveErrors.None;
            }

            public ShapeRecord[] Collected { get; private set; }
        }

        private class ShapeRecordEqualityComparer : IEqualityComparer<ShapeRecord>
        {
            private readonly IEqualityComparer<ShapeContent> _comparer;

            public ShapeRecordEqualityComparer()
            {
                _comparer = new ShapeContentEqualityComparer();
            }

            public bool Equals(ShapeRecord left, ShapeRecord right)
            {
                if (left == null && right == null) return true;
                if (left == null || right == null) return false;
                var sameHeader = left.Header.Equals(right.Header);
                var sameLength = left.Length.Equals(right.Length);
                var sameContent = _comparer.Equals(left.Content, right.Content);
                return sameHeader && sameLength && sameContent;
            }

            public int GetHashCode(ShapeRecord instance)
            {
                return instance.Header.GetHashCode()
                       ^ instance.Length.GetHashCode()
                       ^ _comparer.GetHashCode(instance.Content);
            }
        }

        private class ShapeContentEqualityComparer : IEqualityComparer<ShapeContent>
        {
            public bool Equals(ShapeContent left, ShapeContent right)
            {
                if (left == null && right == null)
                    return true;
                if (left == null || right == null)
                    return false;
                if (left is NullShapeContent && right is NullShapeContent)
                    return true;
                if (left is PointShapeContent leftPointContent && right is PointShapeContent rightPointContent)
                    return Equals(leftPointContent, rightPointContent);
                if (left is PolyLineMShapeContent leftLineContent && right is PolyLineMShapeContent rightLineContent)
                    return Equals(leftLineContent, rightLineContent);
                return false;
            }

            private bool Equals(PointShapeContent left, PointShapeContent right)
            {
                var sameContentLength = left.ContentLength.Equals(right.ContentLength);
                var sameShapeType = left.ShapeType.Equals(right.ShapeType);
                var sameShape = left.Shape.Equals(right.Shape);
                return sameContentLength && sameShapeType && sameShape;
            }

            private bool Equals(PolyLineMShapeContent left, PolyLineMShapeContent right)
            {
                var sameContentLength = left.ContentLength.Equals(right.ContentLength);
                var sameShapeType = left.ShapeType.Equals(right.ShapeType);
                var sameShape = left.Shape.Equals(right.Shape);
                return sameContentLength && sameShapeType && sameShape;
            }

            public int GetHashCode(ShapeContent instance)
            {
                if (instance is NullShapeContent)
                    return 0;
                if (instance is PointShapeContent pointContent)
                    return pointContent.ContentLength.GetHashCode() ^ pointContent.ShapeType.GetHashCode() ^
                           pointContent.Shape.GetHashCode();
                if (instance is PolyLineMShapeContent lineContent)
                    return lineContent.ContentLength.GetHashCode() ^ lineContent.ShapeType.GetHashCode() ^
                           lineContent.Shape.GetHashCode();
                return -1;
            }
        }
    }
}
