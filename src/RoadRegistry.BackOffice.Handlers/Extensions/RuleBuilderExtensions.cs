namespace RoadRegistry.BackOffice.Handlers.Extensions;

using FluentValidation;

public static class RuleBuilderExtensions
{
    private const string StreetNameIdPrefix = "https://data.vlaanderen.be/id/straatnaam/";

    public static IRuleBuilderOptions<T, string> MustBeValidStreetNameId<T>(this IRuleBuilder<T, string> ruleBuilder
        , bool allowNotApplicable = false)
    {
        return ruleBuilder.Must(value =>
        {
            if (value == null)
            {
                return true;
            }

            if (allowNotApplicable
                && StreetNameLocalId.TryParseUsingDutchName(value, out var streetNameLocalId)
                && streetNameLocalId == StreetNameLocalId.NotApplicable)
            {
                return true;
            }

            if (!value.StartsWith(StreetNameIdPrefix))
            {
                return false;
            }

            var identifier = value[StreetNameIdPrefix.Length..];
            return int.TryParse(identifier, out _);
        });
    }
}
