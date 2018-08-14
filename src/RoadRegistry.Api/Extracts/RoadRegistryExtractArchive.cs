namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using ExtractFiles;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public class RoadRegistryExtractArchive : IEnumerable<ExtractFile>
    {
        private readonly string _name;
        private readonly IList<ExtractFile> _files = new List<ExtractFile>();

        public RoadRegistryExtractArchive(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            _name = name.EndsWith(".zip") ? name : name.TrimEnd('.') + ".zip";
        }

        public void Add(ExtractFile file)
        {
            _files.Add(file);
        }

        public void Add(IEnumerable<ExtractFile> files)
        {
            foreach (var file in files)
            {
                Add(file);
            }
        }

        public FileStreamResult CreateResponse()
        {
            var archiveStream = new MemoryStream();
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in _files)
                {
                    using (file)
                    {
                        var fileCompents = file.Flush();
                        using (var archiveItem = archive.CreateEntry(fileCompents.Name).Open())
                        {
                            fileCompents.Content.CopyTo(archiveItem);
                        }
                    }
                }
            }

            // argh! FileStreamResult does a Stream.CopyTo (which copies 0 bytes if you don't reset the position)
            archiveStream.Position = 0;

            return new FileStreamResult(archiveStream, new MediaTypeHeaderValue("application/zip"))
            {
                FileDownloadName = _name
            };
        }

        public IEnumerator<ExtractFile> GetEnumerator()
        {
            return _files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _files.GetEnumerator();
        }
    }
}
