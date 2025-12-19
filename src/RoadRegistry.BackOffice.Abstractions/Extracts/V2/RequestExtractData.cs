namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

using NetTopologySuite.Geometries;

public sealed record RequestExtractData(ExtractRequestId ExtractRequestId, DownloadId DownloadId, MultiPolygon Contour, string Description, bool IsInformative, string? ExternalRequestId);
