namespace RoadRegistry.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class RoadSegmentDynamicHardeningAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly HardeningTypeTranslator _hardeningTypeTranslator;
        private readonly Encoding _encoding;

        public RoadSegmentDynamicHardeningAttributeRecordProjection(HardeningTypeTranslator hardeningTypeTranslator, Encoding encoding)
        {
            _hardeningTypeTranslator = hardeningTypeTranslator ?? throw new ArgumentNullException(nameof(hardeningTypeTranslator));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.Hardenings.Length == 0)
                return Task.CompletedTask;

            var hardenings = @event
                .Hardenings
                .Select(hardening => new RoadSegmentDynamicHardeningAttributeRecord
                {
                    Id = hardening.AttributeId,
                    RoadSegmentId = @event.Id,
                    DbaseRecord = new RoadSegmentDynamicHardeningAttributeDbaseRecord
                        {
                            WV_OIDN = { Value = hardening.AttributeId },
                            WS_OIDN = { Value = @event.Id },
                            WS_GIDN = { Value = $"{@event.Id}_{@event.GeometryVersion}" },
                            TYPE = { Value = _hardeningTypeTranslator.TranslateToIdentifier(hardening.Type) },
                            LBLTYPE = { Value = _hardeningTypeTranslator.TranslateToDutchName(hardening.Type) },
                            VANPOS = { Value = (double)hardening.FromPosition },
                            TOTPOS = { Value = (double)hardening.ToPosition },
                            BEGINTIJD = { Value = hardening.Origin.Since },
                            BEGINORG = { Value = hardening.Origin.OrganizationId },
                            LBLBGNORG = { Value = hardening.Origin.Organization },
                        }.ToBytes(_encoding)
                });

            return context.AddRangeAsync(hardenings, token);
        }
    }
}
