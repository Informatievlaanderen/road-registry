namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.Collections.Generic;
using System.Linq;
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
            _requestedDownloads.Add(new DownloadId(e.DownloadId));

            Id = ExtractRequestId.FromString(e.RequestId);
            Description = new ExtractDescription(e.Description ?? string.Empty);
            IsInformative = e.IsInformative;
            DateRequested = DateTime.UtcNow;
            _externalExtractRequestId = new ExternalExtractRequestId(e.ExternalRequestId);
        });
        On<RoadNetworkExtractGotRequestedV2>(e =>
        {
            _requestedDownloads.Add(new DownloadId(e.DownloadId));

            Id = ExtractRequestId.FromString(e.RequestId);
            Description = new ExtractDescription(e.Description);
            IsInformative = e.IsInformative;
            DateRequested = DateTime.UtcNow;
            _externalExtractRequestId = new ExternalExtractRequestId(e.ExternalRequestId);
        });
        On<RoadNetworkExtractDownloadTimeoutOccurred>(e =>
        {
            Id = ExtractRequestId.FromString(e.RequestId);
            Description = new ExtractDescription(e.Description);
        });
        On<RoadNetworkExtractDownloadBecameAvailable>(e =>
        {
            _announcedDownloads.Add(new DownloadId(e.DownloadId));
        });
        On<RoadNetworkExtractChangesArchiveUploaded>(e =>
        {
            _knownUploads.Add(new UploadId(e.UploadId));
        });
        On<RoadNetworkExtractChangesArchiveFeatureCompareCompleted>(e =>
        {
            _knownUploads.Add(new UploadId(e.UploadId));
        });
        On<RoadNetworkExtractClosed>(e =>
        {
            IsInformative = true;
        });
    }

    public ExtractRequestId Id { get; private set; }
    public ExtractDescription Description { get; private set; }
    public DateTime DateRequested { get; private set; }
    public bool IsInformative { get; private set; }

    public void AnnounceAvailable(DownloadId downloadId, ArchiveId archiveId, ICollection<DownloadId> overlapsWithDownloadIds)
    {
        if (_requestedDownloads.Contains(downloadId) && !_announcedDownloads.Contains(downloadId))
            Apply(new RoadNetworkExtractDownloadBecameAvailable
            {
                Description = Description,
                RequestId = Id.ToString(),
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                IsInformative = IsInformative,
                OverlapsWithDownloadIds = overlapsWithDownloadIds?.Select(x => x.ToGuid()).ToList() ?? []
            });
    }

    public void AnnounceTimeoutOccurred(DownloadId? downloadId)
    {
        Apply(new RoadNetworkExtractDownloadTimeoutOccurred
        {
            Description = Description,
            RequestId = Id.ToString(),
            ExternalRequestId = _externalExtractRequestId,
            DownloadId = downloadId,
            IsInformative = IsInformative
        });
    }

    public void Download(DownloadId downloadId)
    {
        Apply(new RoadNetworkExtractDownloaded
        {
            DownloadId = downloadId,
            Description = Description,
            RequestId = Id.ToString(),
            ExternalRequestId = _externalExtractRequestId,
            IsInformative = IsInformative
        });
    }

    public static RoadNetworkExtract Request(
        ExternalExtractRequestId externalExtractRequestId,
        DownloadId downloadId,
        ExtractDescription extractDescription,
        IPolygonal contour,
        bool isInformative)
    {
        var instance = Factory();
        instance.Apply(new RoadNetworkExtractGotRequestedV2
        {
            Description = extractDescription,
            RequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId).ToString(),
            ExternalRequestId = externalExtractRequestId,
            DownloadId = downloadId,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour),
            IsInformative = isInformative
        });
        return instance;
    }

    public void RequestAgain(DownloadId downloadId, IPolygonal contour, bool isInformative)
    {
        if (!_requestedDownloads.Contains(downloadId))
            Apply(new RoadNetworkExtractGotRequestedV2
            {
                Description = Description,
                RequestId = Id.ToString(),
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour),
                IsInformative = isInformative
            });
    }

    public RoadNetworkExtractUpload Upload(DownloadId downloadId, UploadId uploadId, ArchiveId archiveId)
    {
        if (!_requestedDownloads.Contains(downloadId))
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException(
                _externalExtractRequestId, Id, downloadId, uploadId);

        if (_requestedDownloads[^1] != downloadId)
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException(
                _externalExtractRequestId, Id, downloadId, _requestedDownloads[^1], uploadId);

        if (_knownUploads.Count == 1)
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException();

        Apply(new RoadNetworkExtractChangesArchiveUploaded
        {
            Description = Description,
            RequestId = Id,
            ExternalRequestId = _externalExtractRequestId,
            DownloadId = downloadId,
            UploadId = uploadId,
            ArchiveId = archiveId
        });

        return new RoadNetworkExtractUpload(
            _externalExtractRequestId,
            Id,
            Description,
            downloadId,
            uploadId,
            archiveId,
            Apply);
    }

    public void Close(RoadNetworkExtractCloseReason reason, DownloadId? downloadId = null)
    {
        var closeDownloadIds = downloadId is not null
            ? new[] { downloadId.Value }
            : _requestedDownloads.ToArray();

        Apply(new RoadNetworkExtractClosed
        {
            RequestId = Id,
            ExternalRequestId = _externalExtractRequestId,
            DownloadIds = closeDownloadIds.Select(x => x.ToString()).ToArray(),
            DateRequested = DateRequested,
            Reason = reason
        });
    }
}
