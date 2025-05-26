namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using FluentAssertions;
using Moq;
using TicketingService.Abstractions;

public static class MockExtensions
{
    public static void VerifyThatTicketHasError(this Mock<ITicketing> ticketing, string code, string message)
    {
        var ticketError = GetTicketErrors(ticketing).FirstOrDefault();

        ticketError.Should().NotBeNull();
        ticketError!.ErrorCode.Should().Be(code);
        ticketError.ErrorMessage.Should().Be(message);
    }

    public static void VerifyThatTicketHasError(this Mock<ITicketing> ticketing, string code, string message, Dictionary<string, object> errorContext)
    {
        var ticketError = GetTicketErrors(ticketing).FirstOrDefault();

        ticketError.Should().NotBeNull();
        ticketError!.ErrorCode.Should().Be(code);
        ticketError.ErrorMessage.Should().Be(message);
        ticketError.ErrorContext.Should().BeEquivalentTo(errorContext);
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
