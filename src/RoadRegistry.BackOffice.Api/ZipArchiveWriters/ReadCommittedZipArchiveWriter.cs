namespace RoadRegistry.Api.ZipArchiveWriters
{
    using System;
    using System.Data;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Schema;
    using Microsoft.EntityFrameworkCore;

    public class ReadCommittedZipArchiveWriter : IZipArchiveWriter
    {
        private readonly IZipArchiveWriter _writer;

        public ReadCommittedZipArchiveWriter(IZipArchiveWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public async Task WriteAsync(ZipArchive archive, ShapeContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            using (await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                await _writer.WriteAsync(archive, context, cancellationToken);
            }
        }
    }
}
