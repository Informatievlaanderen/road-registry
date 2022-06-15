namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using FluentValidation;

public class RebuildSnapshotParametersValidator : AbstractValidator<RebuildSnapshotParameters>
{
    private static readonly BlobName SnapshotPrefix = new BlobName("roadnetworksnapshot-");

    public RebuildSnapshotParametersValidator(IBlobClient client)
    {
        RuleFor(x => x.StartFromVersion)
            .GreaterThan(0)
            .MustAsync((version, ct) =>
            {
                var snapshotBlobName = SnapshotPrefix.Append(new BlobName(version.ToString()));
                return client.BlobExistsAsync(snapshotBlobName, ct);
            });
    }
}
