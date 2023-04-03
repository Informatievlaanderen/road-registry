namespace RoadRegistry.BackOffice.Core;

using System;
using ProblemCodes;

public class ShapeFileInvalidHeader : Error
{
    public ShapeFileInvalidHeader(Exception ex)
        : base(ProblemCode.ShapeFile.InvalidHeader,
            new ProblemParameter("ErrorMessage", ex.Message))
    {
    }
}
