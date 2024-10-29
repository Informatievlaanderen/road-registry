namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;

public interface IMunicipalities
{
    Task<Municipality> FindAsync(MunicipalityNisCode id, CancellationToken ct = default);
}
