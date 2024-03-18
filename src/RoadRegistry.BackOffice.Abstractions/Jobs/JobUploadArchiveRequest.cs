namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using System;
    using MediatR;
    using RoadRegistry.BackOffice.Abstractions.Uploads;

    public sealed record JobUploadArchiveRequest(Guid JobId, UploadExtractArchiveRequest Archive) : IRequest<JobUploadArchiveResponse>;
}
