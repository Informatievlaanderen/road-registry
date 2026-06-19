namespace RoadRegistry.BackOffice.Api.V2;

using RoadRegistry.BackOffice.Api.Infrastructure.Options;

internal static class PublicApi
{
    public const string ApiVersion = "v3";

    public static string GetWegknoopDetailUrlFormat(this ApiOptions apiOptions)
    {
        return $"{apiOptions.BaseUrl}/{ApiVersion}/wegknopen/{{0}}";
    }

    public static string GetWegsegmentDetailUrlFormat(this ApiOptions apiOptions)
    {
        return $"{apiOptions.BaseUrl}/{ApiVersion}/wegsegmenten/{{0}}";
    }

    public static string GetGelijkgrondseKruisingDetailUrlFormat(this ApiOptions apiOptions)
    {
        return $"{apiOptions.BaseUrl}/{ApiVersion}/gelijkgrondsekruisingen/{{0}}";
    }

    public static string GetOngelijkgrondseKruisingDetailUrlFormat(this ApiOptions apiOptions)
    {
        return $"{apiOptions.BaseUrl}/{ApiVersion}/ongelijkgrondsekruisingen/{{0}}";
    }

    public static string GetStraatnaamDetailUrlFormat(this ApiOptions apiOptions)
    {
        return $"{apiOptions.BaseUrl}/{ApiVersion}/straatnamen/{{0}}";
    }
}
