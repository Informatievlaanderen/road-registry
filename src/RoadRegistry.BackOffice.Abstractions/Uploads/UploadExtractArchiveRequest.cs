namespace RoadRegistry.BackOffice.Abstractions.Uploads;

using Be.Vlaanderen.Basisregisters.BlobStore;

public record UploadExtractArchiveRequest(string FileName, Stream ReadStream, ContentType ContentType) : EndpointRequest<UploadExtractArchiveResponse>;
