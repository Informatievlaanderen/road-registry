namespace RoadRegistry.BackOffice.Api.Handlers
{
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Jobs;
    using Jobs.Abstractions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class JobUploadArchiveRequestHandler : IRequestHandler<JobUploadArchiveRequest, JobUploadArchiveResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly RoadNetworkJobsBlobClient _jobsBlobClient;

        public JobUploadArchiveRequestHandler(
            JobsContext jobsContext,
            RoadNetworkJobsBlobClient jobsBlobClient)
        {
            _jobsContext = jobsContext.ThrowIfNull();
            _jobsBlobClient = jobsBlobClient.ThrowIfNull();
        }

        public async Task<JobUploadArchiveResponse> Handle(
            JobUploadArchiveRequest request,
            CancellationToken cancellationToken)
        {
            var job = await _jobsContext.FindJob(request.JobId, cancellationToken);
            if (job is null)
            {
                throw new ApiException("Onbestaande upload job.", StatusCodes.Status404NotFound);
            }

            var metadata = Metadata.None.Add(
                new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                    string.IsNullOrEmpty(request.Archive.FileName)
                        ? job.Id + ".zip"
                        : request.Archive.FileName)
            );

            await _jobsBlobClient.CreateBlobAsync(
                new BlobName(job.ReceivedBlobName),
                metadata,
                ContentType.Parse("application/zip"),
                request.Archive.ReadStream,
                cancellationToken
            );
            
            return new JobUploadArchiveResponse();
        }
    }
}
