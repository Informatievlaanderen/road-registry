namespace RoadRegistry.BackOffice.Api.Downloads;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Downloads;
using Abstractions.Exceptions;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class DownloadController
{
    private const string GetForProductRoute = "for-product/{date}";

    /// <summary>
    ///     Gets for product.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpGet(GetForProductRoute, Name = nameof(GetForProduct))]
    [SwaggerOperation(OperationId = nameof(GetForProduct), Description = "")]
    public async Task<IActionResult> GetForProduct(string date, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadProductRequest(date);
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (DownloadProductNotFoundException)
        {
            return NotFound();
        }
    }
}