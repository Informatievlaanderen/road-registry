namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class RoadSegmentNotFoundInZipArchiveException : RoadRegistryException
{
    public RoadSegmentNotFoundInZipArchiveException(int id) : base($"Road segment with ID {id} could not be found.")
    {
    }

    public RoadSegmentNotFoundInZipArchiveException(string message) : base(message)
    {
    }

    protected RoadSegmentNotFoundInZipArchiveException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
