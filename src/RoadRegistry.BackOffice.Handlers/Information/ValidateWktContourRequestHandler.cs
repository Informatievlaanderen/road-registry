namespace RoadRegistry.BackOffice.Handlers.Information;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Information;
using FluentValidation;
using Framework;
using Microsoft.Extensions.Logging;
using NetTopologySuite.IO;

/// <summary>
///     Upload controller, get upload
/// </summary>
/// <exception cref="ExtractDownloadNotFoundException"></exception>
/// <exception cref="ValidationException"></exception>
public class ValidateWktContourRequestHandler : EndpointRequestHandler<ValidateWktContourRequest, ValidateWktContourResponse>
{
    private readonly WKTReader _reader;
    private const int SquareKmMaximum = 100;

    public ValidateWktContourRequestHandler(
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        ILogger<ValidateWktContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public override Task<ValidateWktContourResponse> HandleAsync(ValidateWktContourRequest request, CancellationToken cancellationToken)
    {
        var geometry = _reader.Read(request.Contour);

        var response = new ValidateWktContourResponse
        {
            IsValid = geometry.IsValid,
            Area = geometry.Area
        };

        return Task.FromResult(response);
    }
}
