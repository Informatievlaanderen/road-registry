namespace RoadRegistry.Jobs.Abstractions
{
    using System;
    using System.Collections.Generic;

    public sealed record UploadPreSignedUrlResponse(Guid JobId, string UploadUrl, Dictionary<string, string> UploadUrlFormData, string TicketUrl);
}
