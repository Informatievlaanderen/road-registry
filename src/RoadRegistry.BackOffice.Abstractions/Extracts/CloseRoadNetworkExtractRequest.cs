namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using Messages;

public sealed record CloseRoadNetworkExtractRequest(DownloadId DownloadId, RoadNetworkExtractCloseReason Reason) : EndpointRequest<CloseRoadNetworkExtractResponse> {}
