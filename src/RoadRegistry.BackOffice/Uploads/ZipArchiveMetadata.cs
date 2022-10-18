namespace RoadRegistry.BackOffice.Uploads;

using System;

public sealed class ZipArchiveMetadata
{
    public static readonly ZipArchiveMetadata Empty = new(null);

    private ZipArchiveMetadata(DownloadId? downloadId)
    {
        DownloadId = downloadId;
    }

    public DownloadId? DownloadId { get; }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;

        if (ReferenceEquals(this, obj)) return true;

        return obj.GetType() == GetType() && Equals((ZipArchiveMetadata)obj);
    }

    private bool Equals(ZipArchiveMetadata other)
    {
        return Nullable.Equals(DownloadId, other.DownloadId);
    }

    public override int GetHashCode()
    {
        return DownloadId.GetHashCode();
    }

    public ZipArchiveMetadata WithDownloadId(DownloadId downloadId)
    {
        return new ZipArchiveMetadata(downloadId);
    }
}