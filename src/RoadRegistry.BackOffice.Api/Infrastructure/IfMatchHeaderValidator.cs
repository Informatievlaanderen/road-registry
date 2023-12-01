namespace RoadRegistry.BackOffice.Api.Infrastructure;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using RoadSegments;
using System.Threading;
using System.Threading.Tasks;

public interface IIfMatchHeaderValidator
{
    public Task<bool> IsValid(string? ifMatchHeaderValue, RoadSegmentRecord roadSegment, CancellationToken ct);
}

public class IfMatchHeaderValidator : IIfMatchHeaderValidator
{
    public async Task<bool> IsValid(string? ifMatchHeaderValue, RoadSegmentRecord roadSegment, CancellationToken ct)
    {
        if (ifMatchHeaderValue is null)
        {
            return true;
        }

        if (roadSegment is null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var ifMatchTag = ifMatchHeaderValue.Trim();

        var lastHash = roadSegment.LastEventHash;
        var lastHashTag = new ETag(ETagType.Strong, lastHash);

        return ifMatchTag == lastHashTag.ToString();
    }
}
