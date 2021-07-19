namespace RoadRegistry.BackOffice.ExtractHost
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Extracts;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;
    using ZipArchiveWriters;

    public class RoadNetworkExtractArchiveAssembler : IRoadNetworkExtractArchiveAssembler
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Func<EditorContext> _contextFactory;
        private readonly IZipArchiveWriter<EditorContext> _writer;

        public RoadNetworkExtractArchiveAssembler(
            RecyclableMemoryStreamManager manager,
            Func<EditorContext> contextFactory,
            IZipArchiveWriter<EditorContext> writer)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public async Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var stream = _manager.GetStream();
            using (var context = _contextFactory())
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                await _writer.WriteAsync(archive, request, context, cancellationToken);
            }

            return stream;
        }
    }
}
