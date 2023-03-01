namespace RoadRegistry.BackOffice;

using System;

public interface IOptionsValidator
{
    void ValidateAndThrow(object options);
    Type GetOptionsType();
}
