namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Core.ProblemCodes;
using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;
using System.IO.Compression;
using Uploads;

public static class ZipArchiveCleanerExtensions
{
    public static bool UpdateRoadSegmentAttributeMissingFromOrToPositions<TDbaseRecord>(this ZipArchive archive,
        IReadOnlyCollection<TDbaseRecord> dbfRecords,
        FileEncoding encoding,
        Func<TDbaseRecord, int?> getRoadSegmentId = null,
        Func<TDbaseRecord, double?> getFromPosition = null,
        Action<TDbaseRecord, double> setFromPosition = null,
        Func<TDbaseRecord, double?> getToPosition = null,
        Action<TDbaseRecord, double> setToPosition = null
    )
        where TDbaseRecord : DbaseRecord, new()
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(dbfRecords);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(getRoadSegmentId);
        ArgumentNullException.ThrowIfNull(getFromPosition);
        ArgumentNullException.ThrowIfNull(setFromPosition);
        ArgumentNullException.ThrowIfNull(getToPosition);
        ArgumentNullException.ThrowIfNull(setToPosition);

        var dataChanged = false;

        var (roadSegmentFeatures, problems) = new RoadSegmentFeatureCompareFeatureReader(encoding)
            .Read(archive, FeatureType.Change, ExtractFileName.Wegsegment, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

        var hasRoadSegmentError = problems
            .Where(x => x.Reason != ProblemCode.RoadSegment.StartNode.Missing && x.Reason != ProblemCode.RoadSegment.EndNode.Missing)
            .OfType<FileError>()
            .Any();

        if (!hasRoadSegmentError)
        {
            var featuresGroupedByRoadSegment = dbfRecords
                .Select(record => new
                {
                    RoadSegmentId = getRoadSegmentId(record),
                    Record = record
                })
                .Where(x => x.RoadSegmentId is not null && RoadSegmentId.Accepts(x.RoadSegmentId.Value))
                .GroupBy(x => x.RoadSegmentId.Value, x => x.Record)
                .ToDictionary(x => x.Key, x => x.ToArray());

            foreach (var group in featuresGroupedByRoadSegment)
            {
                var nullFromPosition = group.Value
                    .Where(x => getFromPosition(x) is null || getFromPosition(x) == 0)
                    .ToArray();
                var nullToPosition = group.Value
                    .Where(x => getToPosition(x) is null)
                    .ToArray();

                if (nullFromPosition.Length == 1 && getFromPosition(nullFromPosition.Single()) is null)
                {
                    setFromPosition(nullFromPosition.Single(), 0);
                    dataChanged = true;
                }

                if (nullToPosition.Length == 1 && getToPosition(nullToPosition.Single()) is null)
                {
                    var roadSegmentGeometries = roadSegmentFeatures
                        .Where(x => x.Attributes.Id == group.Key)
                        .Select(x => x.Attributes.Geometry)
                        .ToArray();
                    if (roadSegmentGeometries.Length == 1)
                    {
                        setToPosition(nullToPosition.Single(), roadSegmentGeometries.Single().Length);
                        dataChanged = true;
                    }
                }
            }
        }

        return dataChanged;
    }
}
