namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ExtractFiles;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public class RoadRegistryExtractArchive : IEnumerable
    {
        private readonly string _fileName;
        private readonly List<ExtractFile> _files = new List<ExtractFile>();

        public RoadRegistryExtractArchive(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            _fileName = name.EndsWith(".zip") ? name : name.TrimEnd('.') + ".zip";
        }

        public void Add(ExtractFile fileWriter)
        {
            if (null != fileWriter)
                _files.Add(fileWriter);
        }

        public void Add(IEnumerable<ExtractFile> files)
        {
            if (null != files)
                _files.AddRange(files);
        }

        public FileResult CreateCallbackFileStreamResult(CancellationToken token)
        {
            return new FileCallbackResult(
                new MediaTypeHeaderValue("application/octet-stream"),
                (stream, _) => Task.Run(() => WriteArchiveContent(stream, token), token)
            )
            {
                FileDownloadName = _fileName
            };
        }

        private void WriteArchiveContent(Stream archiveStream, CancellationToken token)
        {
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            {
                foreach (var file in _files.Where(file => null != file))
                {
                    if (token.IsCancellationRequested)
                        break;

                    using (var dbfFileStream = archive.CreateEntry(file.Name).Open())
                    {
                        file.WriteTo(dbfFileStream, token);
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _files.GetEnumerator();
        }
    }
}
