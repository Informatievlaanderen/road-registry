namespace RoadRegistry.BackOffice.Abstractions.Uploads
{
    public sealed record UploadPreSignedUrlResponse(Guid JobId, string UploadUrl, Dictionary<string, string> UploadUrlFormData, string TicketUrl);
}
