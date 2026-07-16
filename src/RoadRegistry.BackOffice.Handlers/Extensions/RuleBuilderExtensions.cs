namespace RoadRegistry.BackOffice.Handlers.Extensions;

using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
using FluentValidation;

public static class RuleBuilderExtensions
{
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

            if (!value.StartsWith(OsloNamespaces.StraatNaam.Value))
            {
                return false;
            }

            var identifier = value[OsloNamespaces.StraatNaam.Value.Length..].TrimStart('/');
            return int.TryParse(identifier, out _);
        });
    }
}
