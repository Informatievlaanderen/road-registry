namespace RoadRegistry.LegacyStreamLoader
{
    using System;

    public static class ArrayExtensions
    {
        public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] input, Func<Int32, TInput, TOutput> converter)
        {
            var output = new TOutput[input.Length];
            for(var index = 0;index < input.Length; index++)
            {
                output[index] = converter(index, input[index]);
            }
            return output;
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] input, Func<TInput, TOutput> converter)
        {
            return Array.ConvertAll<TInput, TOutput>(input, value => converter(value));
        }
    }
}
