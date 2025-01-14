namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public record DownloadExtractRequestBody(string Contour, string RequestId, bool? IsInformative);
