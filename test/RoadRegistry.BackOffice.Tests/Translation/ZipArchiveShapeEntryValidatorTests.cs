namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using Xunit;

    public class ZipArchiveShapeEntryValidatorTests
    {
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
                            new ShapeFileHeaderException("The database file type must be 3 (dBase III).")),
                        result,
                        new ErrorComparer());
                }
            }
        }

//        [Fact]
//        public void ValidateReturnsExpectedResultWhenShapeFileHeaderDoesNotMatch()
//        {
//            var sut = new ZipArchiveShapeEntryValidator<FakeShapeRecord>(
//                Encoding.UTF8,
//                new FakeShapeSchema(),
//                new FakeShapeRecordValidator());
//            var date = DateTime.Today;
//            var header = new ShapeFileHeader(
//                date,
//                ShapeCodePage.Western_European_ANSI,
//                new ShapeRecordCount(0),
//                new AnonymousShapeSchema(new ShapeField[0]));
//
//            using (var stream = new MemoryStream())
//            {
//                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
//                {
//                    var entry = archive.CreateEntry("entry");
//                    using(var entryStream = entry.Open())
//                    using(var writer = new BinaryWriter(entryStream, Encoding.UTF8))
//                    {
//                        header.Write(writer);
//                        entryStream.Flush();
//                    }
//                }
//                stream.Flush();
//                stream.Position = 0;
//
//                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
//                {
//                    var entry = archive.GetEntry("entry");
//
//                    var result = sut.Validate(entry);
//
//                    Assert.Equal(
//                        ZipArchiveErrors.None.ShapeSchemaMismatch(
//                            entry.Name,
//                            new FakeShapeSchema(),
//                            new AnonymousShapeSchema(new ShapeField[0])),
//                        result);
//                }
//            }
//        }
//
//        [Fact]
//        public void ValidateReturnsExpectedResultWhenShapeRecordValidatorReturnsErrors()
//        {
//            var errors = new[]
//            {
//                new Error("error1", new ProblemParameter("parameter1", "value1")),
//                new Error("error2", new ProblemParameter("parameter2", "value2"))
//            };
//            var schema = new FakeShapeSchema();
//            var sut = new ZipArchiveShapeEntryValidator<FakeShapeRecord>(
//                Encoding.UTF8,
//                schema,
//                new FakeShapeRecordValidator(errors));
//            var date = DateTime.Today;
//            var header = new ShapeFileHeader(
//                date,
//                ShapeCodePage.Western_European_ANSI,
//                new ShapeRecordCount(0),
//                schema);
//
//            using (var stream = new MemoryStream())
//            {
//                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
//                {
//                    var entry = archive.CreateEntry("entry");
//                    using(var entryStream = entry.Open())
//                    using(var writer = new BinaryWriter(entryStream, Encoding.UTF8))
//                    {
//                        header.Write(writer);
//                        entryStream.Flush();
//                    }
//                }
//                stream.Flush();
//                stream.Position = 0;
//
//                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
//                {
//                    var entry = archive.GetEntry("entry");
//
//                    var result = sut.Validate(entry);
//
//                    Assert.Equal(
//                        ZipArchiveErrors.None.CombineWith(errors),
//                        result);
//                }
//            }
//        }
//
//        [Fact]
//        public void ValidatePassesExpectedShapeRecordsToShapeRecordValidator()
//        {
//            var schema = new FakeShapeSchema();
//            var validator = new CollectShapeRecordValidator();
//            var sut = new ZipArchiveShapeEntryValidator<FakeShapeRecord>(
//                Encoding.UTF8,
//                schema,
//                validator);
//            var records = new []
//            {
//                new FakeShapeRecord {Field = {Value = 1}},
//                new FakeShapeRecord {Field = {Value = 2}}
//            };
//            var date = DateTime.Today;
//            var header = new ShapeFileHeader(
//                date,
//                ShapeCodePage.Western_European_ANSI,
//                new ShapeRecordCount(records.Length),
//                schema);
//
//            using (var stream = new MemoryStream())
//            {
//                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
//                {
//                    var entry = archive.CreateEntry("entry");
//                    using(var entryStream = entry.Open())
//                    using(var writer = new BinaryWriter(entryStream, Encoding.UTF8))
//                    {
//                        header.Write(writer);
//                        foreach (var record in records)
//                        {
//                            record.Write(writer);
//                        }
//                        writer.Write(ShapeRecord.EndOfFile);
//                        entryStream.Flush();
//                    }
//                }
//                stream.Flush();
//                stream.Position = 0;
//
//                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
//                {
//                    var entry = archive.GetEntry("entry");
//
//                    var result = sut.Validate(entry);
//
//                    Assert.Equal(ZipArchiveErrors.None, result);
//                    Assert.Equal(records, validator.Collected);
//                }
//            }
//        }

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
    }
}