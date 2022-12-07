namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using System.Threading.Tasks;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using SqlStreamStore.Streams;

public class RebuildSnapshotParametersValidator : AbstractValidator<RebuildSnapshotParameters>
{
    private static readonly BlobName SnapshotPrefix = new("roadnetworksnapshot-");

    public RebuildSnapshotParametersValidator(IBlobClient client)
    {
        RuleFor(x => x.StartFromVersion)
            .GreaterThanOrEqualTo(0)
            .MustAsync((version, ct) =>
            {
                if (version > StreamVersion.Start)
                {
                    var snapshotBlobName = SnapshotPrefix.Append(new BlobName(version.ToString()));
                    return client.BlobExistsAsync(snapshotBlobName, ct);
                }

                return Task.FromResult(true);
            })
            .WithErrorCode(ValidationErrors.Common.NotFound.Code)
            .WithMessage(item => $"No snapshot blob found for version '{item.StartFromVersion}'");
    }
}
