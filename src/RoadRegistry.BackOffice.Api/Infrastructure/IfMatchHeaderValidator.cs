namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Editor.Schema;

    public interface IIfMatchHeaderValidator
    {
        public Task<bool> IsValid(string? ifMatchHeaderValue, RoadSegmentId roadSegmentId, CancellationToken ct);
    }

    public class IfMatchHeaderValidator : IIfMatchHeaderValidator
    {
        private readonly EditorContext _editorContext;

        public IfMatchHeaderValidator(EditorContext editorContext)
        {
            _editorContext = editorContext;
        }

        public async Task<bool> IsValid(string? ifMatchHeaderValue, RoadSegmentId roadSegmentId, CancellationToken ct)
        {
            if (ifMatchHeaderValue is null)
            {
                return true;
            }

            var roadSegment = await _editorContext.RoadSegments.FindAsync(roadSegmentId);
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
}
