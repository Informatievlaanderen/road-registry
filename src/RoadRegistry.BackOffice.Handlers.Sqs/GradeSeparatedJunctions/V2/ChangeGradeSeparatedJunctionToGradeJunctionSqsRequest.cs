namespace RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

// 'Wijzig ongelijkgrondse kruising naar gelijkgrondse kruising'. The junction is the whole request: a grade junction
// says nothing about which road passes over the other, so there is nothing left for the caller to decide.
[BlobRequest]
public sealed class ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest : SqsRequest
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
}
