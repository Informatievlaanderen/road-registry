namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System.Threading.Tasks;

public interface IApiTokenReader
{
    Task<ApiToken> ReadAsync(string apiKey);
}
