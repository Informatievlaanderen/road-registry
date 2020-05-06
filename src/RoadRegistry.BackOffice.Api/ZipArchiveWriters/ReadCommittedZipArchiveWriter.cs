namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.Data;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Microsoft.EntityFrameworkCore;

    public class ReadCommittedZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext: DbContext
    {
        private readonly IZipArchiveWriter<TContext> _writer;

        public ReadCommittedZipArchiveWriter(IZipArchiveWriter<TContext> writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public async Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken)
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
