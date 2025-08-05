namespace RoadRegistry.BackOffice.Handlers.Information;

using Abstractions;
using Abstractions.Information;
using Framework;
using Microsoft.Extensions.Logging;
using NetTopologySuite.IO;

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
            IsLargerThanMaximumArea = geometry.Area > (SquareKmMaximum * 1000 * 1000)
        };

        return Task.FromResult(response);
    }
}

//TODO-pr bovenstaande obsolete? te bekijken waar gebruikt, enkel voor portaal?
public interface IExtractContourValidator
{
    bool IsValid(string contour);
}

public class ExtractContourValidator: IExtractContourValidator
{
    private const int SquareKmMaximum = 100;

    public bool IsValid(string contour)
    {
        try
        {
            var reader = new WKTReader();
            var geometry = reader.Read(contour);
            return geometry.IsValid && geometry.Area <= (SquareKmMaximum * 1000 * 1000);
        }
        catch
        {
            return false;
        }
    }
}
