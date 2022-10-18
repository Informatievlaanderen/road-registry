namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;

public class RebuildSnapshotParametersValidator : AbstractValidator<RebuildSnapshotParameters>
{
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

    private static readonly BlobName SnapshotPrefix = new("roadnetworksnapshot-");
}
