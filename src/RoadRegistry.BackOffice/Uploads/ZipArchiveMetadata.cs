namespace RoadRegistry.BackOffice.Uploads
{
    using System;

    public sealed class ZipArchiveMetadata
    {
        private readonly DownloadId? _downloadId;

        private ZipArchiveMetadata(DownloadId? downloadId)
        {
            _downloadId = downloadId;
        }

        public static readonly ZipArchiveMetadata Empty = new ZipArchiveMetadata(null);

        public DownloadId? DownloadId => _downloadId;

        public ZipArchiveMetadata WithDownloadId(DownloadId downloadId)
        {
            return new ZipArchiveMetadata(downloadId);
        }

        private bool Equals(ZipArchiveMetadata other)
        {
            return Nullable.Equals(_downloadId, other._downloadId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((ZipArchiveMetadata) obj);
        }

        public override int GetHashCode()
        {
            return _downloadId.GetHashCode();
        }
    }
}
