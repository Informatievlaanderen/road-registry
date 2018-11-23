namespace RoadRegistry.Model
{
    using FluentValidation;

    internal static class ArrayValidators {
        public static IRuleBuilderOptions<T, TElement[]> MaximumLength<T, TElement>(this IRuleBuilder<T, TElement[]> ruleBuilder, int max) {
            return ruleBuilder
                .Must(array => array.Length <= max)
                .WithMessage($"The array contains more items than the maximum of {max} allowed.");
        }
    }
}
