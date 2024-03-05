namespace RoadRegistry.BackOffice.Handlers.Extensions;

using FluentValidation;

internal static class RuleBuilderExtensions
{
    private const string StreetNameIdPrefix = "https://data.vlaanderen.be/id/straatnaam/";

    public static IRuleBuilderOptions<T, string> MustBeValidStreetNameId<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
        {
            if (value == null)
            {
                return true;
            }

            if (!value.StartsWith(StreetNameIdPrefix))
            {
                return false;
            }

            var identifier = value.Substring(StreetNameIdPrefix.Length);
            return int.TryParse(identifier, out _);
        });
    }
}
