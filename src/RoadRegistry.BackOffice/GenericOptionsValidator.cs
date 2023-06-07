namespace RoadRegistry.BackOffice;

using System;

public class GenericOptionsValidator<TOptions> : OptionsValidator<TOptions>
{
    private readonly Action<TOptions> _validate;

    public GenericOptionsValidator(Action<TOptions> validate)
    {
        _validate = validate;
    }
    protected override void ValidateAndThrowOptions(TOptions options)
    {
        _validate(options);
    }
}
