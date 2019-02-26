namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Messages;

    public class ZipArchiveTranslator
    {
        private readonly Encoding _encoding;

        public ZipArchiveTranslator(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public ChangeRoadNetwork Translate(ZipArchive archive)
        {
            var roadNodeDbf = archive.GetEntry("wegknoop.dbf");
            if (roadNodeDbf != null)
            {
                using (var roadNodeDbfStream = roadNodeDbf.Open())
                using (var reader = new BinaryReader(roadNodeDbfStream, _encoding))
                {
                    var header = DbaseFileHeader.Read(reader);

                }
            }
            return null;
        }
    }

    //REMARK: Order will be more important here since we'll first want to add segments and only then enrich

    public interface IZipArchiveEntryTranslator
    {
        void Translate(ZipArchiveEntry entry, ChangeRoadNetwork message);
    }

    public interface IZipArchiveDbaseRecordsTranslator<TRecord> where TRecord : DbaseRecord
    {
        void Translate(ZipArchiveEntry entry, IEnumerator<TRecord> records, ChangeRoadNetwork message);
    }

    public class ZipArchiveDbaseEntryTranslator<TRecord> : IZipArchiveEntryTranslator
        where TRecord : DbaseRecord
    {
        private readonly IZipArchiveDbaseRecordsTranslator<TRecord> _translator;

        public ZipArchiveDbaseEntryTranslator(IZipArchiveDbaseRecordsTranslator<TRecord> translator)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public void Translate(ZipArchiveEntry entry, ChangeRoadNetwork message)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (message == null) throw new ArgumentNullException(nameof(message));
            // todo: read header and pass on to translator with record enumerator
        }
    }

    public class EuropeanRoadChangeRecordsTranslator : IZipArchiveDbaseRecordsTranslator<EuropeanRoadChangeDbaseRecord>
    {
        public void Translate(ZipArchiveEntry entry, IEnumerator<EuropeanRoadChangeDbaseRecord> records, ChangeRoadNetwork message)
        {
            //fast lookup of corresponding segment command
            //enrich the command with the record
        }
    }

    public interface IRoadSegmentOperation
    {
        void EnrichWith(RoadSegmentLaneChangeDbaseRecord record);
        void EnrichWith(RoadSegmentWidthChangeDbaseRecord record);
        void EnrichWith(RoadSegmentSurfaceChangeDbaseRecord record);
    }

    public class AddRoadSegmentOperation //: IRoadSegmentOperation
    {
    }

    public class RemoveRoadSegmentOperation //: IRoadSegmentOperation
    {
    }

    public class ModifyRoadSegmentOperation //: IRoadSegmentOperation
    {
    }

    //ZipArchiveTranslator: Task<ChangeRoadNetwork> Translate(ZipArchive);
    //ZipArchiveEntryTranslator: Task Translate(ZipArchiveEntry, ChangeRoadNetwork)
}
