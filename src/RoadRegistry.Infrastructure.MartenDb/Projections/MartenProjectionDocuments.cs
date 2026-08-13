namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Reflection;
using Marten;
using Marten.Schema;
using Marten.Storage;

public static class MartenProjectionDocuments
{
    // Runs a projection's schema configuration against a scratch StoreOptions and returns the document types it
    // registers, so a rebuild that has to wipe those documents can never fall out of sync with the projection:
    // a document type added to the Configure method is picked up automatically.
    public static Type[] GetDocumentTypes(Action<StoreOptions> configureSchema)
    {
        var options = new StoreOptions();
        configureSchema(options);

        // Schema.For<T>() registrations stay pending on the registry until a store is built; folding them into
        // the storage model is an internal step. Both it and the mapping enumeration are no longer public in
        // Marten 8, so reflect - and fail loudly when a Marten upgrade moves them, which
        // MartenProjectionDocumentsTests pins.
        var applyConfiguration = typeof(StoreOptions)
                .GetMethod("ApplyConfiguration", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                $"Marten's {nameof(StoreOptions)} no longer has ApplyConfiguration; adjust {nameof(MartenProjectionDocuments)} to the new Marten internals.");
        applyConfiguration.Invoke(options, []);

        var allDocumentMappings = typeof(StorageFeatures)
                .GetProperty("AllDocumentMappings", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(options.Storage) as IEnumerable<DocumentMapping>
            ?? throw new InvalidOperationException(
                $"Marten's {nameof(StorageFeatures)} no longer exposes AllDocumentMappings; adjust {nameof(MartenProjectionDocuments)} to the new Marten internals.");

        var documentTypes = allDocumentMappings
            .Select(x => x.DocumentType)
            .Where(x => x != typeof(RoadNetworkChangesProjectionProgression))
            .ToArray();

        if (documentTypes.Length == 0)
        {
            throw new InvalidOperationException("The schema configuration registered no document types; refusing to treat that as an empty read model.");
        }

        return documentTypes;
    }
}
