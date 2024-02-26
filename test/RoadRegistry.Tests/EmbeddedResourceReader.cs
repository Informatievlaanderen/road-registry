namespace RoadRegistry.Tests;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

public sealed class EmbeddedResourceReader
{
    public static MemoryStream Read(string fileName)
    {
        if (!TryFindEmbeddedResourceName(fileName, out var resourceType, out var resourceName))
        {
            throw new FileNotFoundException("Embedded resource not found!", fileName);
        }

        var sourceStream = new MemoryStream();
        var embeddedStream = resourceType.Assembly.GetManifestResourceStream(resourceName);
        embeddedStream!.CopyTo(sourceStream);
        sourceStream.Position = 0;

        return sourceStream;
    }

    public static async Task<MemoryStream> ReadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        if (!TryFindEmbeddedResourceName(fileName, out var resourceType, out var resourceName))
        {
            throw new FileNotFoundException("Embedded resource not found!", fileName);
        }

        var sourceStream = new MemoryStream();
        await using var embeddedStream = resourceType.Assembly.GetManifestResourceStream(resourceName);
        await embeddedStream!.CopyToAsync(sourceStream, cancellationToken);
        sourceStream.Position = 0;

        return sourceStream;
    }

    public static FormFile ReadFormFile(MemoryStream sourceStream, string fileName, string contentType)
    {
        var formFile = new FormFile(sourceStream, 0L, sourceStream.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, contentType) }
            })
        };
        return formFile;
    }

    public static async Task<FormFile> ReadFormFileAsync(string fileName, string contentType, CancellationToken cancellationToken)
    {
        var sourceStream = await ReadAsync(fileName, cancellationToken);
        return ReadFormFile(sourceStream, fileName, contentType);
    }

    public static bool TryFindEmbeddedResourceName(string fileName, out Type resourceType, out string resourceName)
    {
        var resourceTypes = new[]
        {
            new StackTrace()
                .GetFrames()
                .Where(frame =>
                {
                    var declaringType = frame.GetMethod()?.DeclaringType;
                    var fullName = declaringType?.FullName ?? string.Empty;
                    return fullName.StartsWith("RoadRegistry") && !fullName.Contains(nameof(EmbeddedResourceReader)) && !(declaringType?.IsNested ?? false);
                })
                .Select(frame => frame.GetMethod()!.DeclaringType)
                .LastOrDefault(),
            typeof(TestStartup)
        }.Where(x => x != null).Distinct().ToArray();

        foreach (var resourceTypeItem in resourceTypes)
        {
            resourceType = resourceTypeItem;

            var resourceNames = resourceType
                .Assembly
                .GetManifestResourceNames();

            resourceName = resourceNames
                .SingleOrDefault(embeddedResource => embeddedResource.EndsWith($".{fileName}", StringComparison.InvariantCultureIgnoreCase));

            if (resourceName is not null)
            {
                return true;
            }
        }

        resourceType = null;
        resourceName = null;
        return false;
    }
}
