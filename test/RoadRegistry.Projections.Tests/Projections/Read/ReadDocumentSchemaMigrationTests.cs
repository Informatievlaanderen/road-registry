namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Marten;
using Marten.Schema;
using Marten.Storage;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections.Setup;
using Weasel.Core;
using Weasel.Postgresql;

// Marten runs with AutoCreate.None, so the storage for the read documents comes from the hand-written SQL
// migrations and nothing at runtime reconciles the two. A duplicated column the model has but the migration does
// not create, or a storage function whose argument list differs by one argument from the one Marten calls, shows up
// only as a failing projection in production - and the argument list is not something a reviewer can check by eye:
// Marten sorts the arguments itself.
//
// So derive the storage objects from the model, offline, and require that what they declare is in the migrations.
public class ReadDocumentSchemaMigrationTests
{
    [Fact]
    public void DuplicatedColumnsAreCreatedByAMigration()
    {
        // Per statement, so a column of the same name on another table does not count as covered.
        var statements = AllMigrationsSql().Split(';');

        var missing = ReadDocumentMappings()
            .SelectMany(mapping => mapping.DuplicatedFields
                .Select(field => (Table: Normalize(mapping.TableName.QualifiedName), Column: Normalize($"{field.ColumnName} {field.PgType}"))))
            // The column name has to stand on its own: the storage functions take it as arg_<column> of the same
            // type, which a plain substring match would accept as a column declaration.
            .Where(column => !statements.Any(statement =>
                statement.Contains(column.Table)
                && Regex.IsMatch(statement, $@"(?<![a-z0-9_]){Regex.Escape(column.Column)}")))
            .Select(column => $"{column.Table}.{column.Column}")
            .ToArray();

        missing.Should().BeEmpty("every duplicated column of a read document must be created by a migration");
    }

    [Fact]
    public void StorageFunctionsAreCreatedByAMigrationWithTheArgumentsMartenCalls()
    {
        var migrations = AllMigrationsSql();

        var missing = GeneratedStatements()
            .Where(line => line.StartsWith("create or replace function"))
            .Select(line => line[line.IndexOf("projections.", StringComparison.Ordinal)..])
            .Where(signature => !migrations.Contains(signature))
            .ToArray();

        missing.Should().BeEmpty("every read document's upsert/insert/update function must be created by a migration with exactly the arguments Marten passes");
    }

    [Fact]
    public void DuplicatedColumnIndexesAreCreatedByAMigration()
    {
        var migrations = AllMigrationsSql();

        var missing = GeneratedStatements()
            .Where(line => line.StartsWith("create index"))
            .Select(line => line[line.IndexOf("ix_", StringComparison.Ordinal)..])
            .Where(index => !migrations.Contains(index))
            .ToArray();

        missing.Should().BeEmpty("every index on a duplicated column of a read document must be created by a migration");
    }

    // The read documents as the model declares them. Folding the pending Schema.For<T>() registrations into the
    // storage model and enumerating them is internal in Marten 8 (as in MartenProjectionDocuments), so reflect -
    // and fail loudly when a Marten upgrade moves it.
    private static IEnumerable<DocumentMapping> ReadDocumentMappings()
    {
        var options = new StoreOptions();
        options.ConfigureRoad();
        options.ConfigureReadDocuments();

        var applyConfiguration = typeof(StoreOptions)
                .GetMethod("ApplyConfiguration", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                $"Marten's {nameof(StoreOptions)} no longer has ApplyConfiguration; adjust {nameof(ReadDocumentSchemaMigrationTests)} to the new Marten internals.");
        applyConfiguration.Invoke(options, []);

        var allDocumentMappings = typeof(StorageFeatures)
                .GetProperty("AllDocumentMappings", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(options.Storage) as IEnumerable<DocumentMapping>
            ?? throw new InvalidOperationException(
                $"Marten's {nameof(StorageFeatures)} no longer exposes AllDocumentMappings; adjust {nameof(ReadDocumentSchemaMigrationTests)} to the new Marten internals.");

        var mappings = allDocumentMappings.Where(x => x.Alias.StartsWith("read_")).ToArray();
        if (mappings.Length == 0)
        {
            throw new InvalidOperationException("No read document mappings found; refusing to pass a schema check that compares nothing.");
        }

        return mappings;
    }

    // The table, index and function statements Marten would create for those documents. Built from the mapping
    // rather than from IMartenStorage.AllObjects(), which opens a connection to reset the hilo sequences.
    private static string[] GeneratedStatements()
    {
        var writer = new StringWriter();
        var migrator = new PostgresqlMigrator();

        foreach (var mapping in ReadDocumentMappings())
        {
            foreach (var schemaObject in StorageObjectsFor(mapping))
            {
                schemaObject.WriteCreateStatement(migrator, writer);
            }
        }

        return writer.ToString().Split('\n').Select(Normalize).ToArray();
    }

    private static IEnumerable<ISchemaObject> StorageObjectsFor(DocumentMapping mapping)
    {
        foreach (var typeName in new[]
                 {
                     "Marten.Storage.DocumentTable",
                     "Marten.Storage.UpsertFunction",
                     "Marten.Storage.InsertFunction",
                     "Marten.Storage.UpdateFunction"
                 })
        {
            var type = typeof(IDocumentStore).Assembly.GetType(typeName)
                ?? throw new InvalidOperationException($"Marten no longer has {typeName}; adjust {nameof(ReadDocumentSchemaMigrationTests)} to the new Marten internals.");

            var constructor = type.GetConstructors()
                                  .SingleOrDefault(x => x.GetParameters().Length >= 1 && x.GetParameters()[0].ParameterType == typeof(DocumentMapping))
                              ?? throw new InvalidOperationException($"Marten's {typeName} is no longer built from a DocumentMapping; adjust {nameof(ReadDocumentSchemaMigrationTests)} to the new Marten internals.");

            var parameters = constructor.GetParameters();
            var arguments = new object?[parameters.Length];
            arguments[0] = mapping;
            for (var i = 1; i < arguments.Length; i++)
            {
                arguments[i] = parameters[i].DefaultValue;
            }

            yield return (ISchemaObject)constructor.Invoke(arguments);
        }
    }

    private static string AllMigrationsSql()
    {
        var assembly = typeof(RoadRegistry.Infrastructure.MartenDb.Setup.SetupExtensions).Assembly;
        var names = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".sql")).Order().ToArray();
        if (names.Length == 0)
        {
            throw new InvalidOperationException("No migration resources found; refusing to pass a schema check that compares against nothing.");
        }

        return string.Join('\n', names.Select(name =>
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            return string.Join('\n', reader.ReadToEnd().Split('\n').Select(Normalize));
        }));
    }

    // Marten's writer and a hand-edited migration differ in whitespace and casing, nothing else.
    private static string Normalize(string line)
    {
        return string.Join(' ', line.ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
