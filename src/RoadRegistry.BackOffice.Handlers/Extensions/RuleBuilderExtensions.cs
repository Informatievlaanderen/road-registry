namespace RoadRegistry.BackOffice.Handlers.Extensions;

using FluentValidation;

internal static class RuleBuilderExtensions
{
    private const string StreetNamePuriPrefix = "https://data.vlaanderen.be/id/straatnaam/";

    public static IRuleBuilderOptions<T, string> MustBeValidStreetNamePuri<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
        {
            if (value == null)
            {
                return true;
            }

            if (!value.StartsWith(StreetNamePuriPrefix))
            {
                return false;
            }

            var identifier = value.Substring(StreetNamePuriPrefix.Length);
            return int.TryParse(identifier, out _);
        });
    }
}
