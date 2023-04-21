namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.Collections.Generic;
using Framework;
using Messages;
using NetTopologySuite.Geometries;

public class RoadNetworkExtract : EventSourcedEntity
{
    public static readonly Func<RoadNetworkExtract> Factory = () => new RoadNetworkExtract();
    private readonly HashSet<DownloadId> _announcedDownloads;
    private readonly HashSet<UploadId> _knownUploads;
    private readonly List<DownloadId> _requestedDownloads;
    private ExternalExtractRequestId _externalExtractRequestId;

    private RoadNetworkExtract()
    {
        _requestedDownloads = new List<DownloadId>();
        _announcedDownloads = new HashSet<DownloadId>();
        _knownUploads = new HashSet<UploadId>();

        On<RoadNetworkExtractGotRequested>(e =>
        {
            Id = ExtractRequestId.FromString(e.RequestId);
            Description = new ExtractDescription(e.Description ?? string.Empty);
            _externalExtractRequestId = new ExternalExtractRequestId(e.ExternalRequestId);
            _requestedDownloads.Add(new DownloadId(e.DownloadId));
        });
        On<RoadNetworkExtractGotRequestedV2>(e =>
        {
            Id = ExtractRequestId.FromString(e.RequestId);
            Description = new ExtractDescription(e.Description);
            _externalExtractRequestId = new ExternalExtractRequestId(e.ExternalRequestId);
            _requestedDownloads.Add(new DownloadId(e.DownloadId));
        });
        On<RoadNetworkExtractDownloadTimeoutOccurred>(e => {
            Id = ExtractRequestId.FromString(e.RequestId);
            Description = new ExtractDescription(e.Description);
        });
        On<RoadNetworkExtractDownloadBecameAvailable>(e => { _announcedDownloads.Add(new DownloadId(e.DownloadId)); });
        On<RoadNetworkExtractChangesArchiveUploaded>(e =>
        {
            _knownUploads.Add(new UploadId(e.UploadId));
            FeatureCompareCompleted = false;
        });
        On<RoadNetworkExtractChangesArchiveFeatureCompareCompleted>(e =>
        {
            _knownUploads.Add(new UploadId(e.UploadId));
            FeatureCompareCompleted = true;
        });
    }

    public ExtractRequestId Id { get; private set; }
    public ExtractDescription Description { get; private set; }
    public bool FeatureCompareCompleted { get; private set; }

    public void AnnounceAvailable(DownloadId downloadId, ArchiveId archiveId)
    {
        if (_requestedDownloads.Contains(downloadId) && !_announcedDownloads.Contains(downloadId))
            Apply(new RoadNetworkExtractDownloadBecameAvailable
            {
                Description = Description,
                RequestId = Id.ToString(),
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId
            });
    }

    public void AnnounceTimeoutOccurred()
    {
        Apply(new RoadNetworkExtractDownloadTimeoutOccurred
        {
            Description = Description,
            RequestId = Id.ToString(),
            ExternalRequestId = _externalExtractRequestId,
        });
    }

    public static RoadNetworkExtract Request(
        ExternalExtractRequestId externalExtractRequestId,
        DownloadId downloadId,
        ExtractDescription extractDescription,
        IPolygonal contour)
    {
        var instance = Factory();
        instance.Apply(new RoadNetworkExtractGotRequestedV2
        {
            Description = extractDescription,
            RequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId).ToString(),
            ExternalRequestId = externalExtractRequestId,
            DownloadId = downloadId,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour)
        });
        return instance;
    }

    public void RequestAgain(DownloadId downloadId, IPolygonal contour)
    {
        if (!_requestedDownloads.Contains(downloadId))
            Apply(new RoadNetworkExtractGotRequestedV2
            {
                Description = Description,
                RequestId = Id.ToString(),
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour)
            });
    }

    public RoadNetworkExtractUpload Upload(DownloadId downloadId, UploadId uploadId, ArchiveId archiveId, bool featureCompareCompleted = false)
    {
        if (!_requestedDownloads.Contains(downloadId))
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException(
                _externalExtractRequestId, Id, downloadId, uploadId);

        if (_requestedDownloads[_requestedDownloads.Count - 1] != downloadId)
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException(
                _externalExtractRequestId, Id, downloadId, _requestedDownloads[_requestedDownloads.Count - 1], uploadId);

        if (_knownUploads.Count == 1)
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException(
                _externalExtractRequestId, Id, downloadId, uploadId);

        if (featureCompareCompleted)
        {
            Apply(new RoadNetworkExtractChangesArchiveFeatureCompareCompleted
            {
                Description = Description,
                RequestId = Id,
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId
            });
        }
        else
        {
            Apply(new RoadNetworkExtractChangesArchiveUploaded
            {
                Description = Description,
                RequestId = Id,
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId
            });
        }

        return new RoadNetworkExtractUpload(
            _externalExtractRequestId,
            Id,
            Description,
            downloadId,
            uploadId,
            archiveId,
            Apply);
    }
}
