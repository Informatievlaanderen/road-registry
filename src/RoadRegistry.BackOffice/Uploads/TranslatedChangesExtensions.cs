namespace RoadRegistry.BackOffice.Uploads;

using System.Collections.Generic;
using System.Linq;

internal static class TranslatedChangesExtensions
{
    public static ITranslatedChange Flatten(this IEnumerable<ITranslatedChange> changes)
    {
        return changes.SingleOrDefault(_ => !ReferenceEquals(_, null));
    }
}
