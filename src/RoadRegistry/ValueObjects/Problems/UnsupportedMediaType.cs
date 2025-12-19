namespace RoadRegistry.ValueObjects.Problems;

using Be.Vlaanderen.Basisregisters.BlobStore;
using ProblemCodes;

public class UnsupportedMediaType : Error
{
    public UnsupportedMediaType()
        : base(ProblemCode.Upload.UnsupportedMediaType)
    {
    }

    public UnsupportedMediaType(ContentType contentType)
        : base(ProblemCode.Upload.UnsupportedMediaType,
            new ProblemParameter("ContentType", contentType))
    {
    }
}
