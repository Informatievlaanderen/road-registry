namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveTranslator
    {
        private readonly Encoding _encoding;

        public ZipArchiveTranslator(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public Task Translate(ZipArchive archive)
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
            return Task.CompletedTask;
        }
    }

    //ZipArchiveTranslator: Task<ChangeRoadNetwork> Translate(ZipArchive);
    //ZipArchiveEntryTranslator: Task Translate(ZipArchiveEntry, ChangeRoadNetwork)
}
