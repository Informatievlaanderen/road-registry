namespace RoadRegistry.BackOffice;

using System;

public abstract class OptionsValidator<TOptions> : IOptionsValidator
{
    public void ValidateAndThrow(object options)
    {
        ValidateAndThrowOptions((TOptions)options);
    }

    public Type GetOptionsType()
    {
        return typeof(TOptions);
    }

    protected abstract void ValidateAndThrowOptions(TOptions options);
}
