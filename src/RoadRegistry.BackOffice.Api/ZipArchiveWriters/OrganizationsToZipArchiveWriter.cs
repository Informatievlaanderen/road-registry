namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Schema;
    using Schema.Organizations;

    public class OrganizationsToZipArchiveWriter : IZipArchiveWriter
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

        public OrganizationsToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, BackOfficeContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var dbfEntry = archive.CreateEntry("LstOrg.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(await context.Organizations.CountAsync(cancellationToken) + Organization.PredefinedTranslations.All.Length),
                OrganizationDbaseRecord.Schema
            );
            using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new OrganizationDbaseRecord();
                foreach (var predefined in Organization.PredefinedTranslations.All)
                {
                    dbfRecord.ORG.Value = predefined.Identifier;
                    dbfRecord.LBLORG.Value = predefined.Name;
                    dbfWriter.Write(dbfRecord);
                }

                foreach (var data in context.Organizations.OrderBy(_ => _.SortableCode).Select(_ => _.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }
                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
