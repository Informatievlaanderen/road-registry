namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Requests;

using Abstractions;
using RoadRegistry.BackOffice.MessagingHost.Sqs.Responses;

public record FeatureCompareMessageRequest(string ArchiveId) : SqsMessageRequest<FeatureCompareMessageResponse>
{
}
