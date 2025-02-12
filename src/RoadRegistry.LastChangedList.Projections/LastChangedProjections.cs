namespace RoadRegistry.Editor.Projections
{
    using System;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;

    [ConnectedProjectionName(ProjectionName)]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel wegsegment de gecachte data nog geüpdated moeten worden.")]
    public class LastChangedProjections : LastChangedListConnectedProjection
    {
        public const string ProjectionName = "Cache markering wegsegmenten";
        private static readonly AcceptType[] SupportedAcceptTypes = [AcceptType.Json];

        public LastChangedProjections(ICacheValidator cacheValidator)
            : base(SupportedAcceptTypes, cacheValidator)
        {
            When<Envelope<ImportedRoadSegment>>(async (context, envelope, ct) => { await GetLastChangedRecordsAndUpdatePosition(envelope.Message.Id.ToString(), envelope.Position, context, ct); });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
            {
                foreach (var message in envelope.Message.Changes.Flatten())
                {
                    switch (message)
                    {
                        case RoadSegmentAdded roadSegmentAdded:
                            await GetLastChangedRecordsAndUpdatePosition(roadSegmentAdded.Id.ToString(), envelope.Position, context, ct);
                            break;

                        case RoadSegmentModified roadSegmentModified:
                            await GetLastChangedRecordsAndUpdatePosition(roadSegmentModified.Id.ToString(), envelope.Position, context, ct);
                            break;

                        case RoadSegmentAddedToEuropeanRoad change:
                            await GetLastChangedRecordsAndUpdatePosition(change.SegmentId.ToString(), envelope.Position, context, ct);
                            break;
                        case RoadSegmentRemovedFromEuropeanRoad change:
                            await GetLastChangedRecordsAndUpdatePosition(change.SegmentId.ToString(), envelope.Position, context, ct);
                            break;

                        case RoadSegmentAddedToNationalRoad change:
                            await GetLastChangedRecordsAndUpdatePosition(change.SegmentId.ToString(), envelope.Position, context, ct);
                            break;
                        case RoadSegmentRemovedFromNationalRoad change:
                            await GetLastChangedRecordsAndUpdatePosition(change.SegmentId.ToString(), envelope.Position, context, ct);
                            break;

                        case RoadSegmentAddedToNumberedRoad change:
                            await GetLastChangedRecordsAndUpdatePosition(change.SegmentId.ToString(), envelope.Position, context, ct);
                            break;
                        case RoadSegmentRemovedFromNumberedRoad change:
                            await GetLastChangedRecordsAndUpdatePosition(change.SegmentId.ToString(), envelope.Position, context, ct);
                            break;

                        case RoadSegmentAttributesModified roadSegmentAttributesModified:
                            await GetLastChangedRecordsAndUpdatePosition(roadSegmentAttributesModified.Id.ToString(), envelope.Position, context, ct);
                            break;

                        case RoadSegmentGeometryModified roadSegmentGeometryModified:
                            await GetLastChangedRecordsAndUpdatePosition(roadSegmentGeometryModified.Id.ToString(), envelope.Position, context, ct);
                            break;

                        case RoadSegmentRemoved roadSegmentRemoved:
                            await GetLastChangedRecordsAndUpdatePosition(roadSegmentRemoved.Id.ToString(), envelope.Position, context, ct);
                            break;
                    }
                }
            });

            When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, ct) =>
            {
                // todo-pr aan welke wegsegmenten is de organisatie gekoppeld?
            });

            When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, ct) =>
            {
                // todo-pr aan welke wegsegmenten is de organisatie gekoppeld?
            });
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.Json => $"v2/wegsegment:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => "/v2/wegsegmenten/{0}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }
    }
}
