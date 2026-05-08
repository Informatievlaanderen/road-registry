namespace RoadRegistry.Tests;

using FluentAssertions;
using Moq;
using TicketingService.Abstractions;

public static class MockExtensions
{
    public static void VerifyThatTicketHasError(this Mock<ITicketing> ticketing, string code, string? message = null, Dictionary<string, object>? errorContext = null)
    {
        var ticketErrors = GetTicketErrors(ticketing).ToArray();
        ticketErrors.Should()
            .Contain(x => x.ErrorCode == code
                          && (message == null || x.ErrorMessage == message)
                          && (errorContext == null || (x.ErrorContext != null && x.ErrorContext.EqualsCollection(errorContext))));
    }

    public static void VerifyThatTicketHasCompleted(this Mock<ITicketing> ticketing, object response)
    {
        ticketing.Verify(x =>
            x.Complete(
                It.IsAny<Guid>(),
                new TicketResult(response),
                CancellationToken.None
            )
        );
    }

    private static IEnumerable<TicketError> GetTicketErrors(Mock<ITicketing> ticketing)
    {
        return ticketing.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Error))
            .Select(x => x.Arguments.OfType<TicketError>().Single())
            .SelectMany(ticketError => ticketError.Errors?.Count > 0 ? ticketError.Errors : [ticketError]);
    }
}
