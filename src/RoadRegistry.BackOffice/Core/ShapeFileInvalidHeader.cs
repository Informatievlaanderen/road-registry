namespace RoadRegistry.BackOffice.Core;

using System;

public class ShapeFileInvalidHeader : Error
{
    public ShapeFileInvalidHeader(Exception ex)
        : base(nameof(ShapeFileInvalidHeader), new ProblemParameter("ErrorMessage", ex.Message))
    {
    }
}
