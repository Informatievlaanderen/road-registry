namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using Be.Vlaanderen.Basisregisters.BlobStore;

public readonly record struct DownloadExtractByFileRequestItem(string FileName, Stream ReadStream, ContentType ContentType) {}