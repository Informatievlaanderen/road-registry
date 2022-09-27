namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Responses;

using Abstractions;

public record FeatureCompareMessageResponse(ArchiveId ArchiveId) : SqsMessageResponse
{
}
