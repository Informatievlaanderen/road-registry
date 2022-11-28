namespace System;

using Runtime.CompilerServices;

public static class ObjectExtensions
{
    public static T ThrowIfNull<T>(this T argument, [CallerArgumentExpression("argument")] string argumentName = null)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(argumentName);
        }

        return argument;
    }
}
