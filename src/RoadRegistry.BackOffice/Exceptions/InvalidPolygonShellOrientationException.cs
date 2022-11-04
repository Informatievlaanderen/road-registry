namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class InvalidPolygonShellOrientationException : ApplicationException
{
    public InvalidPolygonShellOrientationException()
        : base("The shell of a polygon must have a clockwise orientation.")
    {
    }

    protected InvalidPolygonShellOrientationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
