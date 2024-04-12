namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.BlobStore;
using ProblemCodes;

public class UnsupportedMediaType : Error
{
    public UnsupportedMediaType(ContentType contentType)
        : base(ProblemCode.Upload.UnsupportedMediaType,
            new ProblemParameter("ContentType", contentType))
    {
    }
}
