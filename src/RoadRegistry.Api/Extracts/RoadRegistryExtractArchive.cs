namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using Responses;

    public class RoadRegistryExtractArchive
    {
        private readonly string _name;
        private readonly IList<RoadRegistryExtractFile> _files = new List<RoadRegistryExtractFile>();

        public RoadRegistryExtractArchive(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            _name = name.EndsWith(".zip") ? name : name.TrimEnd('.') + ".zip";
        }

        public void Add(RoadRegistryExtractFile file)
        {
            if (file?.Content == null)
                return;

            if (_files.Any(f => f.Name == file.Name))
                throw new InvalidDataException($"File {file.Name} already exists in {_name}");

            _files.Add(file);
        }

        public void Add(IEnumerable<RoadRegistryExtractFile> files)
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
                    using (var archiveItem = archive.CreateEntry(file.Name).Open())
                    {
                        file.Content.Position = 0;
                        file.Content.CopyTo(archiveItem);
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

    }
}
