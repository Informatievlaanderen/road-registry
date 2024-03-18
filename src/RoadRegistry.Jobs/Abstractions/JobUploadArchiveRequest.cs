namespace RoadRegistry.Jobs.Abstractions
{
    using System;
    using BackOffice.Abstractions.Uploads;
    using MediatR;

    public sealed record JobUploadArchiveRequest(Guid JobId, UploadExtractArchiveRequest Archive) : IRequest<JobUploadArchiveResponse>;
}
