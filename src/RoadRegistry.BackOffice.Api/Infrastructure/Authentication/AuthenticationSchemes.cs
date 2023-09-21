namespace RoadRegistry.BackOffice.Api.Infrastructure.Authentication;

public static class AuthenticationSchemes
{
    public const string ApiKey = "ApiKey";
    public const string Bearer = "Bearer";
    public const string JwtBearer = "JwtBearer";

    public const string AllBearerSchemes = $"{Bearer},{JwtBearer}";
    public const string AllSchemes = $"{ApiKey},{AllBearerSchemes}";
}
