namespace RoadRegistry.BackOffice.Api.Infrastructure.Options;

using System.Collections.Generic;

public class InwinningOrganizationNisCodesOptions : Dictionary<string, IReadOnlyCollection<string>>
{
    public const string ConfigKey = "InwinningOrganizationNisCodes";
}
