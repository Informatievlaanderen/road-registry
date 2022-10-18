namespace RoadRegistry.Legacy.Import;

using System;

public static class ArrayExtensions
{
    public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] input, Func<int, TInput, TOutput> converter)
    {
        var output = new TOutput[input.Length];

        for (var index = 0; index < input.Length; index++)
            output[index] = converter(index, input[index]);

        return output;
    }

    public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] input, Func<TInput, TOutput> converter)
    {
        return Array.ConvertAll(input, value => converter(value));
    }
}