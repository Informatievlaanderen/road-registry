//namespace RoadRegistry.BackOffice.Translation
//{
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.IO.Compression;
//    using System.Text;
//    using System.Threading.Tasks;
//    using Be.Vlaanderen.Basisregisters.Shaperon;
//    using Messages;
//    using Model;
//
//    public class ZipArchiveTranslator
//    {
//        private readonly Encoding _encoding;
//
//        public ZipArchiveTranslator(Encoding encoding)
//        {
//            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
//        }
//
//        public TranslatedChanges Translate(ZipArchive archive)
//        {
//            var roadNodeDbf = archive.GetEntry("wegknoop.dbf");
//            if (roadNodeDbf != null)
//            {
//                using (var roadNodeDbfStream = roadNodeDbf.Open())
//                using (var reader = new BinaryReader(roadNodeDbfStream, _encoding))
//                {
//                    var header = DbaseFileHeader.Read(reader);
//
//                }
//            }
//            return null;
//        }
//    }
//
//    //REMARK: Order will be more important here since we'll first want to add segments and only then enrich
//
//    public interface IZipArchiveEntryTranslator
//    {
//        TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes);
//    }
//
//    public interface IZipArchiveDbaseRecordsTranslator<TRecord> where TRecord : DbaseRecord
//    {
//        TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<TRecord> records, TranslatedChanges changes);
//    }
//
//    public class ZipArchiveDbaseEntryTranslator<TRecord> : IZipArchiveEntryTranslator
//        where TRecord : DbaseRecord
//    {
//        private readonly IZipArchiveDbaseRecordsTranslator<TRecord> _translator;
//
//        public ZipArchiveDbaseEntryTranslator(IZipArchiveDbaseRecordsTranslator<TRecord> translator)
//        {
//            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
//        }
//
//        public TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes)
//        {
//            if (entry == null) throw new ArgumentNullException(nameof(entry));
//            if (changes == null) throw new ArgumentNullException(nameof(changes));
//            // todo: read header and pass on to translator with record enumerator
//            return changes;
//        }
//    }
//
//    public class EuropeanRoadChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<EuropeanRoadChangeDbaseRecord>
//    {
//        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<EuropeanRoadChangeDbaseRecord> records, TranslatedChanges changes)
//        {
//            if (entry == null) throw new ArgumentNullException(nameof(entry));
//            if (records == null) throw new ArgumentNullException(nameof(records));
//            if (changes == null) throw new ArgumentNullException(nameof(changes));
//
//            while (records.MoveNext())
//            {
//                var record = records.Current;
//                if (record != null)
//                {
//                    switch (record.RECORDTYPE.Value)
//                    {
//                        case RecordTypes.Add:
//                            changes = changes.Append(
//                                new AddRoadSegmentToEuropeanRoad(
//                                    new AttributeId(record.EU_OIDN.Value.GetValueOrDefault()),
//                                    new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
//                                    EuropeanRoadNumber.Parse(record.EUNUMMER.Value)
//                                )
//                            );
//                            break;
//                    }
//                }
//            }
//
//            return changes;
//        }
//    }
//
//    public class NationalRoadChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NationalRoadChangeDbaseRecord>
//    {
//        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<NationalRoadChangeDbaseRecord> records, TranslatedChanges changes)
//        {
//            if (entry == null) throw new ArgumentNullException(nameof(entry));
//            if (records == null) throw new ArgumentNullException(nameof(records));
//            if (changes == null) throw new ArgumentNullException(nameof(changes));
//
//            while (records.MoveNext())
//            {
//                var record = records.Current;
//                if (record != null)
//                {
//                    switch (record.RECORDTYPE.Value)
//                    {
//                        case RecordTypes.Add:
//                            changes = changes.Append(
//                                new AddRoadSegmentToNationalRoad(
//                                    new AttributeId(record.NW_OIDN.Value.GetValueOrDefault()),
//                                    new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
//                                    NationalRoadNumber.Parse(record.IDENT2.Value)
//                                )
//                            );
//                            break;
//                    }
//                }
//            }
//
//            return changes;
//        }
//    }
//
//    public class NumberedRoadChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<NumberedRoadChangeDbaseRecord>
//    {
//        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<NumberedRoadChangeDbaseRecord> records, TranslatedChanges changes)
//        {
//            if (entry == null) throw new ArgumentNullException(nameof(entry));
//            if (records == null) throw new ArgumentNullException(nameof(records));
//            if (changes == null) throw new ArgumentNullException(nameof(changes));
//
//            while (records.MoveNext())
//            {
//                var record = records.Current;
//                if (record != null)
//                {
//                    switch (record.RECORDTYPE.Value)
//                    {
//                        case RecordTypes.Add:
//                            changes = changes.Append(
//                                new AddRoadSegmentToNumberedRoad(
//                                    new AttributeId(record.GW_OIDN.Value.GetValueOrDefault()),
//                                    new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
//                                    NumberedRoadNumber.Parse(record.IDENT8.Value),
//                                    RoadSegmentNumberedRoadDirection.ByIdentifier[record.RICHTING.Value.GetValueOrDefault()],
//                                    new RoadSegmentNumberedRoadOrdinal(record.VOLGNUMMER.Value.GetValueOrDefault())
//                                )
//                            );
//                            break;
//                    }
//                }
//            }
//
//            return changes;
//        }
//    }
//
//    public class RoadSegmentLaneChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadSegmentLaneChangeDbaseRecord>
//    {
//        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<RoadSegmentLaneChangeDbaseRecord> records, TranslatedChanges changes)
//        {
//            if (entry == null) throw new ArgumentNullException(nameof(entry));
//            if (records == null) throw new ArgumentNullException(nameof(records));
//            if (changes == null) throw new ArgumentNullException(nameof(changes));
//
//            while (records.MoveNext())
//            {
//                var record = records.Current;
//                if (record != null)
//                {
//                    switch (record.RECORDTYPE.Value)
//                    {
//                        case RecordTypes.Add:
//                            changes.Enrich(
//                                new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
//                                )
//                            changes = changes.Append(
//                                new AddRoadSegmentToNumberedRoad(
//                                    new AttributeId(record.GW_OIDN.Value.GetValueOrDefault()),
//                                    new RoadSegmentId(record.WS_OIDN.Value.GetValueOrDefault()),
//                                    NumberedRoadNumber.Parse(record.IDENT8.Value),
//                                    RoadSegmentNumberedRoadDirection.ByIdentifier[record.RICHTING.Value.GetValueOrDefault()],
//                                    new RoadSegmentNumberedRoadOrdinal(record.VOLGNUMMER.Value.GetValueOrDefault())
//                                )
//                            );
//                            break;
//                    }
//                }
//            }
//
//            return changes;
//        }
//    }
//
//    public class RoadNodeChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<RoadNodeChangeDbaseRecord>
//    {
//        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<RoadNodeChangeDbaseRecord> records, TranslatedChanges changes)
//        {
//            if (entry == null) throw new ArgumentNullException(nameof(entry));
//            if (records == null) throw new ArgumentNullException(nameof(records));
//            if (changes == null) throw new ArgumentNullException(nameof(changes));
//
//            while (records.MoveNext())
//            {
//                var record = records.Current;
//                if (record != null)
//                {
//                    switch (record.RECORDTYPE.Value)
//                    {
//                        case RecordTypes.Add:
//                            changes = changes.Append(
//                                new AddRoadNode(
//                                    new RoadNodeId(record.WEGKNOOPID.Value.GetValueOrDefault()),
//                                    RoadNodeType.ByIdentifier[record.TYPE.Value.GetValueOrDefault()]
//                                )
//                            );
//                            break;
//                    }
//                }
//            }
//
//            return changes;
//        }
//    }
//
//    public static class RecordTypes
//    {
//        public const short Add = 2;
//    }
//
//
//    //ZipArchiveTranslator: Task<ChangeRoadNetwork> Translate(ZipArchive);
//    //ZipArchiveEntryTranslator: Task Translate(ZipArchiveEntry, ChangeRoadNetwork)
//}
