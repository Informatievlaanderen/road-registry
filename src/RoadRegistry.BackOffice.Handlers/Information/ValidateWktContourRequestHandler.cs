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
    public const int SquareKmMaximum = 100;

    private readonly WKTReader _reader;

    public ValidateWktContourRequestHandler(
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        ILogger<ValidateWktContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    protected override Task<ValidateWktContourResponse> InnerHandleAsync(ValidateWktContourRequest request, CancellationToken cancellationToken)
    {
        var geometry = _reader.Read(request.Contour);

        var response = new ValidateWktContourResponse
        {
            Area = geometry.Area,
            AreaMaximumSquareKilometers = SquareKmMaximum,
            IsValid = geometry.IsValid,
            IsLargerThanMaximumArea =  geometry.Area > (SquareKmMaximum * 1000 * 1000)
        };

        return Task.FromResult(response);
    }
}
