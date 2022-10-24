namespace RoadRegistry.BackOffice.Handlers.Information;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Information;
using FluentValidation;
using Framework;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

/// <summary>
///     Upload controller, get upload
/// </summary>
/// <exception cref="UploadExtractBlobClientNotFoundException"></exception>
/// <exception cref="ExtractDownloadNotFoundException"></exception>
/// <exception cref="ValidationException"></exception>
public class ValidateWktContourRequestHandler : EndpointRequestHandler<ValidateWktContourRequest, ValidateWktContourResponse>
{
    private readonly WKTReader _reader;

    public ValidateWktContourRequestHandler(
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        ILogger<ValidateWktContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public override Task<ValidateWktContourResponse> HandleAsync(ValidateWktContourRequest request, CancellationToken cancellationToken)
    {
        try
        {
            GeometryTranslator.TranslateToRoadNetworkExtractGeometry(_reader.Read(request.Contour) as IPolygonal, 0);
            return Task.FromResult(new ValidateWktContourResponse(request.Contour));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ValidateWktContourResponse(request.Contour)
            {
                Exception = new ValidationException(ex.Message)
            });
        }
    }
}
