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
        private readonly Stopwatch _watch;

        public RoadRegistryExtractArchive(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            _fileName = name.EndsWith(".zip") ? name : name.TrimEnd('.') + ".zip";
            _watch = Stopwatch.StartNew();
            Console.WriteLine($"-- Create {_fileName}");
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
            Console.WriteLine($"-- Start writing to {_fileName} ({_watch.ElapsedMilliseconds}ms)");
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            {
                foreach (var file in _files.Where(writer => null != writer))
                {
                    if (token.IsCancellationRequested)
                        break;

                    Console.WriteLine($"Writing {file.Name}");
                    _watch.Restart();
                    using (var dbfFileStream = archive.CreateEntry(file.Name).Open())
                    {
                        file.WriteTo(dbfFileStream, token);
                    }
                    Console.WriteLine($"--> finished in {_watch.ElapsedMilliseconds}ms");
                }

                Console.WriteLine($"-- Finished writing {_fileName}");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _files.GetEnumerator();
        }
    }
}
