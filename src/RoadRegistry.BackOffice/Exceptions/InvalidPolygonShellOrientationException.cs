namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class InvalidPolygonShellOrientationException : Exception
{
    public InvalidPolygonShellOrientationException()
        : base("The shell of a polygon must have a clockwise orientation.")
    {
    }
}
