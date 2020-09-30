namespace RoadRegistry.BackOffice.Uploads
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class TranslatedChangesExtensions
    {
        public static object Flatten(this IEnumerable<ITranslatedChange> changes) =>
            changes.SingleOrDefault(_ => !ReferenceEquals(_, null));
    }
}
