namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Xunit;

    public class ZipArchiveDbaseEntryTranslatorTests
    {
        [Fact]
        public void EncodingCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipArchiveDbaseEntryTranslator<FakeDbaseRecord>(
                    null,
                    new FakeDbaseRecordTranslator()));
        }

        [Fact]
        public void TranslatorCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipArchiveDbaseEntryTranslator<FakeDbaseRecord>(
                    Encoding.Default,
                    null));
        }

        [Fact]
        public void TranslateEntryCanNotBeNull()
        {
            var sut = new ZipArchiveDbaseEntryTranslator<FakeDbaseRecord>(
                Encoding.Default,
                new FakeDbaseRecordTranslator());

            Assert.Throws<ArgumentNullException>(() => sut.Translate(null, TranslatedChanges.Empty));
        }

        [Fact]
        public void TranslateChangesCanNotBeNull()
        {
            var sut = new ZipArchiveDbaseEntryTranslator<FakeDbaseRecord>(
                Encoding.Default,
                new FakeDbaseRecordTranslator());

            using (var stream = new MemoryStream())
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("entry");
                Assert.Throws<ArgumentNullException>(() => sut.Translate(entry, null));
            }
        }

        [Fact]
        public void TranslateReturnsExpectedResultWhenDbaseRecordTranslatorReturnsChanges()
        {
            var changes = TranslatedChanges.Empty
                .Append(new Uploads.AddRoadNode(new RecordNumber(1), new RoadNodeId(1), RoadNodeType.FakeNode))
                .Append(new Uploads.AddRoadNode(new RecordNumber(2), new RoadNodeId(2), RoadNodeType.FakeNode));
            var sut = new ZipArchiveDbaseEntryTranslator<FakeDbaseRecord>(
                Encoding.UTF8,
                new FakeDbaseRecordTranslator(ignored => changes));
            var date = DateTime.Today;
            var header = new DbaseFileHeader(
                date,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(0),
                new FakeDbaseSchema());

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using(var entryStream = entry.Open())
                    using(var writer = new BinaryWriter(entryStream, Encoding.UTF8))
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

                    var result = sut.Translate(entry, TranslatedChanges.Empty);

                    Assert.Equal(
                        changes,
                        result,
                        new TranslatedChangeEqualityComparer());
                }
            }
        }

        [Fact]
        public void TranslatePassesExpectedDbaseRecordsToDbaseRecordValidator()
        {
            var schema = new FakeDbaseSchema();
            var translator = new CollectDbaseRecordTranslator();
            var sut = new ZipArchiveDbaseEntryTranslator<FakeDbaseRecord>(
                Encoding.UTF8,
                translator);
            var records = new []
            {
                new FakeDbaseRecord {Field = {Value = 1}},
                new FakeDbaseRecord {Field = {Value = 2}}
            };
            var date = DateTime.Today;
            var header = new DbaseFileHeader(
                date,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(records.Length),
                schema);

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using(var entryStream = entry.Open())
                    using(var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                    {
                        header.Write(writer);
                        foreach (var record in records)
                        {
                            record.Write(writer);
                        }
                        writer.Write(DbaseRecord.EndOfFile);
                        entryStream.Flush();
                    }
                }
                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Translate(entry, TranslatedChanges.Empty);

                    Assert.Equal(TranslatedChanges.Empty, result, new TranslatedChangeEqualityComparer());
                    Assert.Equal(records, translator.Collected);
                }
            }
        }

        private class FakeDbaseRecord : DbaseRecord
        {
            private static readonly FakeDbaseSchema Schema = new FakeDbaseSchema();

            public FakeDbaseRecord()
            {
                Field = new DbaseNumber(Schema.Field);
                Values = new DbaseFieldValue[] { Field };
            }

            public DbaseNumber Field { get; }

            public bool Equals(FakeDbaseRecord other) => other != null && Field.Field.Equals(other.Field.Field) &&
                                                         Field.Value.Equals(other.Field.Value);
            public override bool Equals(object obj) => obj is FakeDbaseRecord other && Equals(other);
            public override int GetHashCode() => Field.GetHashCode();
        }

        private class FakeDbaseSchema : DbaseSchema
        {
            public FakeDbaseSchema()
            {
                Fields = new []
                {
                    DbaseField.CreateNumberField(
                        new DbaseFieldName(nameof(Field)),
                        new DbaseFieldLength(10))
                };
            }

            public DbaseField Field => Fields[0];
        }

        private class FakeDbaseRecordTranslator : IZipArchiveDbaseRecordsTranslator<FakeDbaseRecord>
        {
            private readonly Func<TranslatedChanges, TranslatedChanges> _translation;

            public FakeDbaseRecordTranslator(Func<TranslatedChanges, TranslatedChanges> translation = null)
            {
                _translation = translation ?? (changes => changes);
            }

            public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<FakeDbaseRecord> records, TranslatedChanges changes)
            {
                return _translation(changes);
            }
        }

        private class CollectDbaseRecordTranslator : IZipArchiveDbaseRecordsTranslator<FakeDbaseRecord>
        {
            public FakeDbaseRecord[] Collected { get; private set; }

            public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<FakeDbaseRecord> records, TranslatedChanges changes)
            {
                var collected = new List<FakeDbaseRecord>();
                while (records.MoveNext())
                {
                    collected.Add(records.Current);
                }
                Collected = collected.ToArray();

                return changes;
            }
        }
    }
}
