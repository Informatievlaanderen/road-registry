namespace RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;

/// <summary>
/// Marks a <see cref="Be.Vlaanderen.Basisregisters.Sqs.Requests.SqsRequest"/> as a <b>system-internal</b> request.
/// <para>
/// A system request is published by an internal/system flow (e.g. the Marten migration) via a direct SQS copy and is
/// handled by a lambda with ticketing disabled (<c>TicketingBehavior.None</c>). It deliberately has <b>no</b>
/// mediator/<c>SqsHandler</c> entry point, must not be exposed through the API, and must not be reused by user-facing
/// flows. This is guarded by a test that asserts no <c>SqsHandler&lt;T&gt;</c> exists for an <see cref="ISystemSqsRequest"/>.
/// </para>
/// </summary>
public interface ISystemSqsRequest
{
}
