namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using Microsoft.AspNetCore.Http;
using RoadRegistry.Extracts.Schema;

public static class ExtractUploadStatusExtensions
{
    // A delivery that was approved but whose changes the road network refused is one thing to Digitaal Vlaanderen and
    // another to the data-inwinner who uploaded it.
    //
    // Digitaal Vlaanderen is told the delivery was rejected, because they are the ones who correct it and have it
    // approved again - the same mail that reaches them says so. The data-inwinner is told what is true for them:
    // their delivery passed, nothing is expected of them, and the extract stays open until it is processed. Telling
    // them it was rejected would ask them to fix something that is not theirs to fix.
    public static string? AsSeenBy(this string? uploadStatus, HttpContext httpContext)
    {
        if (uploadStatus == nameof(ExtractUploadStatus.ProcessingFailed) && !httpContext.IsDigitaalVlaanderen())
        {
            return nameof(ExtractUploadStatus.Accepted);
        }

        return uploadStatus;
    }
}
