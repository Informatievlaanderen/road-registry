//namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

//using BackOffice.Framework;
//using Messages;
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;
//using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

//public partial class RoadRegistrySystemController
//{
//    [HttpPost("correctroadnodeversions")]
//    [ApiExplorerSettings(IgnoreApi = true)]
//    public async Task<IActionResult> PostCorrectRoadNodeVersions()
//    {
//        var command = new CorrectRoadNodeVersions
//        {
//            Provenance = new RoadRegistryProvenanceData(Modification.Update).ToProvenance()
//        };

//        await RoadNetworkCommandQueue
//            .Write(new Command(command), HttpContext.RequestAborted);

//        return Accepted();
//    }
//}
