using System;
using System.Collections.Generic;
using System.IO;
using JasperFx;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.MartenDb.MigrationGenerator;
using Weasel.Core;
using Weasel.Postgresql;

// Generation-only host: registers the complete Marten model (every document type) and writes migration SQL from it.
// It never runs a projection or applies anything itself; it only diffs the model against a live database.
//
//   patch <file> <connectionString>   -> delta between the model and the connected database
//
// The generate scripts point it at a scratch Postgres already brought to a known state:
//   - baseline: an *empty* scratch db  -> the delta is the full creation script
//   - delta:    a scratch db with the existing migrations applied -> the delta is only what changed
//
// We deliberately do NOT use Marten's WriteCreationScriptToFile: offline it triggers the hilo "reset sequences"
// feature, which opens a hardcoded default (localhost:5432) connection and fails. CreateMigrationAsync diffs
// against the connection we actually configured, so it always talks to the scratch db.
//
// The connection string is passed explicitly (not via an env var) so it reliably survives `dotnet run`.
if (args.Length < 3 || args[0] != "patch")
{
    Console.Error.WriteLine("usage: patch <file> <connectionString>");
    return 1;
}

var file = args[1];
var connectionString = args[2];

var builder = Host.CreateApplicationBuilder();
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    [$"ConnectionStrings:{WellKnownConnectionNames.Marten}"] = connectionString
});
builder.Services.AddMartenRoad(options => options.ConfigureAllRoadDocuments());

using var host = builder.Build();
var store = host.Services.GetRequiredService<IDocumentStore>();

var migration = await store.Storage.CreateMigrationAsync();
if (migration.Difference == SchemaPatchDifference.None)
{
    Console.WriteLine("no schema changes detected");
    return 0;
}

await using var writer = new StreamWriter(file);
migration.WriteAllUpdates(writer, new PostgresqlMigrator(), AutoCreate.CreateOrUpdate);
Console.WriteLine($"wrote migration delta to {file}");
return 0;
