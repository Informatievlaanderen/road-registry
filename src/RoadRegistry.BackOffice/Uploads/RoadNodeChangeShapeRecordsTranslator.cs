namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

    public class RoadNodeChangeShapeRecordsTranslator : IZipArchiveShapeRecordsTranslator
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, TranslatedChanges changes)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            while (records.MoveNext())
            {
                var record = records.Current;
                if (record?.Content is PointShapeContent content
                    && changes.TryFindRoadNodeChangeOfShapeRecord(record.Header.RecordNumber, out var change))
                {
                    switch (change)
                    {
                        case AddRoadNode addition:
                            changes = changes.ReplaceChange(addition, addition.WithGeometry(GeometryTranslator.ToGeometryPoint(content.Shape)));
                            break;
                        case ModifyRoadNode modification:
                            changes = changes.ReplaceChange(modification, modification.WithGeometry(GeometryTranslator.ToGeometryPoint(content.Shape)));
                            break;
                    }
                }
            }
            return changes;
        }
    }
}
