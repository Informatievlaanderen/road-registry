namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using ExtractFiles;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public class RoadRegistryExtractArchive : IEnumerable
    {
        private readonly string _fileName;
        private readonly IList<Func<Task<IEnumerable<ExtractFile>>>> _createFileBatches = new List<Func<Task<IEnumerable<ExtractFile>>>>();

        public RoadRegistryExtractArchive(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            _fileName = name.EndsWith(".zip") ? name : name.TrimEnd('.') + ".zip";
        }

        public void Add(Func<Task<ExtractFile>> createFile)
        {
            if (null != createFile)
                _createFileBatches.Add(async () => new []{ await createFile() });
        }

        public void Add(Func<Task<IEnumerable<ExtractFile>>> createFiles)
        {
            if(null != createFiles)
                _createFileBatches.Add(createFiles);
        }

        public FileResult CreateCallbackFileStreamResult()
        {
            return new FileCallbackResult(
                new MediaTypeHeaderValue("application/octet-stream"),
                WriteArchiveContentAsync
            )
            {
                FileDownloadName = _fileName
            };
        }

        private async Task WriteArchiveContentAsync(Stream archiveStream, ActionContext _)
        {
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            {
                foreach (var createFileBatch in _createFileBatches)
                {
                    var extractFiles = (await createFileBatch())?.Where(file => file != null) ?? new ExtractFile[0];
                    foreach (var file in extractFiles)
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
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _createFileBatches.GetEnumerator();
        }
    }
}
