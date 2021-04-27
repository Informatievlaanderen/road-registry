// namespace RoadRegistry.BackOffice.Uploads
// {
//     using System;
//     using System.Collections.Generic;
//     using System.IO;
//     using System.IO.Compression;
//     using System.Linq;
//     using AutoFixture;
//     using Be.Vlaanderen.Basisregisters.Shaperon;
//     using Schema;
//     using Xunit;
//
//     public class RoadSegmentSurfaceChangeDbaseRecordsValidatorTests : IDisposable
//     {
//         private readonly RoadSegmentSurfaceChangeDbaseRecordsValidator _sut;
//         private readonly ZipArchive _archive;
//         private readonly MemoryStream _stream;
//         private readonly ZipArchiveEntry _entry;
//         private readonly Fixture _fixture;
//         private readonly IDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord> _enumerator;
//
//         public RoadSegmentSurfaceChangeDbaseRecordsValidatorTests()
//         {
//             _fixture = new Fixture();
//             _fixture.CustomizeRecordType();
//             _fixture.CustomizeAttributeId();
//             _fixture.CustomizeRoadSegmentId();
//             _fixture.CustomizeRoadSegmentSurfaceType();
//             _fixture.CustomizeRoadSegmentPosition();
//             _fixture.Customize<RoadSegmentSurfaceChangeDbaseRecord>(
//                 composer => composer
//                     .FromFactory(random => new RoadSegmentSurfaceChangeDbaseRecord
//                     {
//                         RECORDTYPE = {Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
//                         TRANSACTID = {Value = (short)random.Next(1, 9999)},
//                         WV_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
//                         WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
//                         VANPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
//                         TOTPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
//                         TYPE = { Value = (short)_fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
//                     })
//                     .OmitAutoProperties());
//
//             _sut = new RoadSegmentSurfaceChangeDbaseRecordsValidator();
//             _enumerator = new List<RoadSegmentSurfaceChangeDbaseRecord>().ToDbaseRecordEnumerator();
//             _stream = new MemoryStream();
//             _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
//             _entry = _archive.CreateEntry("attwegverharding_all.dbf");
//         }
//
//         [Fact]
//         public void IsZipArchiveDbaseRecordsValidator()
//         {
//             Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentSurfaceChangeDbaseRecord>>(_sut);
//         }
//
//         [Fact]
//         public void ValidateEntryCanNotBeNull()
//         {
//             Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, _enumerator));
//         }
//
//         [Fact]
//         public void ValidateRecordsCanNotBeNull()
//         {
//             Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null));
//         }
//
//         [Fact]
//         public void ValidateWithoutRecordsReturnsExpectedResult()
//         {
//             var result = _sut.Validate(_entry, _enumerator);
//
//             Assert.Equal(
//                 ZipArchiveProblems.Single(_entry.HasNoDbaseRecords(false)),
//                 result);
//         }
//
//         [Fact]
//         public void ValidateWithValidRecordsReturnsExpectedResult()
//         {
//             var records = _fixture
//                 .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(new Random().Next(1, 5))
//                 .Select((record, index) =>
//                 {
//                     record.WV_OIDN.Value = index + 1;
//                     return record;
//                 })
//                 .ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Equal(
//                 ZipArchiveProblems.None,
//                 result);
//         }
//
//         [Fact]
//         public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
//         {
//             var records = _fixture
//                 .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
//                 .Select((record, index) =>
//                 {
//                     record.WV_OIDN.Value = index + 1;
//                     record.RECORDTYPE.Value = -1;
//                     return record;
//                 })
//                 .ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Equal(
//                 ZipArchiveProblems.Many(
//                     _entry
//                         .AtDbaseRecord(new RecordNumber(1))
//                         .RecordTypeMismatch(-1),
//                     _entry
//                         .AtDbaseRecord(new RecordNumber(2))
//                         .RecordTypeMismatch(-1)
//                 ),
//                 result);
//         }
//
//         [Fact]
//         public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierReturnsExpectedResult()
//         {
//             var records = _fixture
//                 .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
//                 .Select((record, index) =>
//                 {
//                     record.WV_OIDN.Value = 1;
//                     if (index == 0)
//                     {
//                         record.RECORDTYPE.Value = (short) RecordType.Identical.Translation.Identifier;
//                     }
//                     else if(index == 1)
//                     {
//                         record.RECORDTYPE.Value = (short) RecordType.Removed.Translation.Identifier;
//                     }
//                     return record;
//                 })
//                 .ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Equal(
//                 ZipArchiveProblems.Single(
//                     _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
//                         new AttributeId(1),
//                         new RecordNumber(1))
//                 ),
//                 result);
//         }
//
//         [Fact]
//         public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierAndHaveAddedAndRemovedAsRecordTypeReturnsExpectedResult()
//         {
//             var records = _fixture
//                 .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
//                 .Select((record, index) =>
//                 {
//                     record.WV_OIDN.Value = 1;
//                     if (index == 0)
//                     {
//                         record.RECORDTYPE.Value = (short) RecordType.Added.Translation.Identifier;
//                     }
//                     else if(index == 1)
//                     {
//                         record.RECORDTYPE.Value = (short) RecordType.Removed.Translation.Identifier;
//                     }
//
//                     return record;
//                 })
//                 .ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Equal(
//                 ZipArchiveProblems.None,
//                 result);
//         }
//
//         [Fact]
//         public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
//         {
//             var records = _fixture
//                 .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
//                 .Select(record =>
//                 {
//                     record.WV_OIDN.Value = 0;
//                     return record;
//                 })
//                 .ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Equal(
//                 ZipArchiveProblems.Many(
//                     _entry.AtDbaseRecord(new RecordNumber(1)).IdentifierZero(),
//                     _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierZero()
//                 ),
//                 result);
//         }
//
//         [Theory]
//         [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
//         public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
//             Action<RoadSegmentSurfaceChangeDbaseRecord> modifier, DbaseField field)
//         {
//             var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
//             modifier(record);
//             var records = new[] {record}.ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Contains(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(field), result);
//         }
//
//         public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
//         {
//             get
//             {
//                 yield return new object[]
//                 {
//                     new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
//                     RoadSegmentSurfaceChangeDbaseRecord.Schema.WS_OIDN
//                 };
//
//                 yield return new object[]
//                 {
//                     new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
//                     RoadSegmentSurfaceChangeDbaseRecord.Schema.RECORDTYPE
//                 };
//
//                 yield return new object[]
//                 {
//                     new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.WV_OIDN.Reset()),
//                     RoadSegmentSurfaceChangeDbaseRecord.Schema.WV_OIDN
//                 };
//
//                 yield return new object[]
//                 {
//                     new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.TYPE.Reset()),
//                     RoadSegmentSurfaceChangeDbaseRecord.Schema.TYPE
//                 };
//
//                 yield return new object[]
//                 {
//                     new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.VANPOSITIE.Reset()),
//                     RoadSegmentSurfaceChangeDbaseRecord.Schema.VANPOSITIE
//                 };
//
//                 yield return new object[]
//                 {
//                     new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.TOTPOSITIE.Reset()),
//                     RoadSegmentSurfaceChangeDbaseRecord.Schema.TOTPOSITIE
//                 };
//             }
//         }
//
//         [Fact]
//         public void ValidateWithProblematicRecordsReturnsExpectedResult()
//         {
//             var records = _fixture
//                 .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
//                 .ToArray();
//             var exception = new Exception("problem");
//             var enumerator = new ProblematicDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord>(records, 1, exception);
//
//             var result = _sut.Validate(_entry, enumerator);
//
//             Assert.Equal(
//                 ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
//                 result,
//                 new FileProblemComparer());
//         }
//
//         [Fact]
//         public void ValidateWithRecordThatHasInvalidTypeReturnsExpectedResult()
//         {
//             var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
//             record.TYPE.Value = -1;
//             var records = new [] { record }.ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Equal(
//                 ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).SurfaceTypeMismatch(-1)),
//                 result);
//         }
//
//         [Fact]
//         public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
//         {
//             var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
//             record.WS_OIDN.Value = -1;
//             var records = new [] { record }.ToDbaseRecordEnumerator();
//
//             var result = _sut.Validate(_entry, records);
//
//             Assert.Equal(
//                 ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentIdOutOfRange(-1)),
//                 result);
//         }
//
//         public void Dispose()
//         {
//             _archive?.Dispose();
//             _stream?.Dispose();
//         }
//     }
// }
