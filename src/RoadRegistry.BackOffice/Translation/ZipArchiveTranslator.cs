namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
    using System.Text;

    public class ZipArchiveTranslator : IZipArchiveTranslator
    {
        private readonly Encoding _encoding;

        public ZipArchiveTranslator(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public TranslatedChanges Translate(ZipArchive archive)
        {
            //REMARK: Order will be more important here since we'll first want to add segments and only then enrich
            return TranslatedChanges.Empty;
        }
    }
}
