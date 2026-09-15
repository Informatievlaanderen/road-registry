using System;
using System.Collections.Generic;
using System.IO;
using JasperFx;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda;
using RoadRegistry.Infrastructure.MartenDb.Setup;

// Pre-generates the Marten code of the backoffice Lambda (GAWR-7236):
//
//   dotnet run --project src/RoadRegistry.BackOffice.Handlers.Sqs.Lambda.CodeGen -- codegen write
//
// The Lambda cannot do this itself. It is a class library the Lambda runtime starts, so there is no Main to hand
// `codegen write` to. This host builds the same store - from FunctionMartenConfiguration, which the function builds its
// own store from - and writes the code into the Lambda's project folder, where `dotnet lambda package` compiles it into
// the function.
//
// Nothing is connected to: the host is built, never started, and the connection string only has to be present.

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    [$"ConnectionStrings:{WellKnownConnectionNames.Marten}"] = "Host=codegen;Database=codegen"
});

builder.Services.AddMartenRoad(options =>
{
    FunctionMartenConfiguration.Configure(options);

    // Not this project's folder, which AddMartenRoad would pick: the code belongs to the function.
    options.GeneratedCodeOutputPath = Path.Combine(FindLambdaProjectDirectory(), "Internal", "Generated");
});

using var host = builder.Build();
return await host.RunJasperFxCommands(args);

static string FindLambdaProjectDirectory()
{
    const string lambdaProject = "RoadRegistry.BackOffice.Handlers.Sqs.Lambda";

    for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
    {
        var candidate = Path.Combine(directory.FullName, lambdaProject);
        if (File.Exists(Path.Combine(candidate, $"{lambdaProject}.csproj")))
        {
            return candidate;
        }
    }

    throw new InvalidOperationException($"Could not find the {lambdaProject} project folder above {AppContext.BaseDirectory}.");
}
