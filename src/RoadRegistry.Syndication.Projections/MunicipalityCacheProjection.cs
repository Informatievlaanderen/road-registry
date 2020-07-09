namespace RoadRegistry.Syndication.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using MunicipalityEvents;
    using Schema;

    public class MunicipalityCacheProjection : ConnectedProjection<SyndicationContext>
    {
        public MunicipalityCacheProjection()
        {
            When<Envelope<MunicipalityWasRegistered>>(async (context, envelope, token) =>
            {
                await context.Municipalities.AddAsync(
                    new MunicipalityRecord
                    {
                        MunicipalityId = envelope.Message.MunicipalityId,
                        NisCode = envelope.Message.NisCode,
                        DutchName = null,
                        FrenchName = null,
                        GermanName = null,
                        EnglishName = null,
                        MunicipalityStatus = MunicipalityStatus.Registered
                    }, token);
            });

            When<Envelope<MunicipalityWasNamed>>(async (context, envelope, token) =>
            {
                var municipalityRecord = await context.Municipalities.FindAsync(envelope.Message.MunicipalityId);
                if (municipalityRecord == null)
                    return;

                switch (envelope.Message.Language)
                {
                    case Language.Dutch:
                        municipalityRecord.DutchName = envelope.Message.Name;
                        break;
                    case Language.French:
                        municipalityRecord.FrenchName = envelope.Message.Name;
                        break;
                    case Language.German:
                        municipalityRecord.GermanName = envelope.Message.Name;
                        break;
                    case Language.English:
                        municipalityRecord.EnglishName = envelope.Message.Name;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            // This method and called methods will be removed.
            When<AtomEntry<SyndicationContent<Gemeente>>>(async (context, envelope, token) =>
            {
                var title = envelope.FeedEntry.Title.Split('-')[0];
                var @event = Enum.Parse<MunicipalityEvent>(title);
                switch (@event)
                {
                    case MunicipalityEvent.MunicipalityWasRegistered:
                        await MunicipalityWasRegistered(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityWasNamed:
                        await MunicipalityWasNamed(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityNameWasCleared:
                        await MunicipalityNameWasCleared(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityNameWasCorrected:
                        await MunicipalityNameWasCorrected(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityNameWasCorrectedToCleared:
                        await MunicipalityNameWasCorrectedToCleared(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityNisCodeWasDefined:
                        await MunicipalityNisCodeWasDefined(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityNisCodeWasCorrected:
                        await MunicipalityNisCodeWasCorrected(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityOfficialLanguageWasAdded:
                        break;
                    case MunicipalityEvent.MunicipalityOfficialLanguageWasRemoved:
                        break;
                    case MunicipalityEvent.MunicipalityFacilityLanguageWasAdded:
                        break;
                    case MunicipalityEvent.MunicipalityFacilityLanguageWasRemoved:
                        break;
                    case MunicipalityEvent.MunicipalityBecameCurrent:
                        await MunicipalityBecameCurrent(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityWasCorrectedToCurrent:
                        await MunicipalityWasCorrectedToCurrent(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityWasRetired:
                        await MunicipalityWasRetired(context, envelope, token);
                        break;
                    case MunicipalityEvent.MunicipalityWasCorrectedToRetired:
                        await MunicipalityWasCorrectedToRetired(context, envelope, token);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private static async Task MunicipalityBecameCurrent(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Current;
        }

        private static async Task MunicipalityWasCorrectedToCurrent(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Current;
        }

        private static async Task MunicipalityWasCorrectedToRetired(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Retired;
        }

        private static async Task MunicipalityWasRetired(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Retired;
        }

        private static async Task MunicipalityNameWasCorrectedToCleared(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            foreach (var municipalityName in envelope.Content.Object.Gemeentenamen)
            {
                switch (municipalityName.Taal)
                {
                    case Taal.NL:
                        municipalityRecord.DutchName = null;
                        break;
                    case Taal.FR:
                        municipalityRecord.FrenchName = null;
                        break;
                    case Taal.DE:
                        municipalityRecord.GermanName = null;
                        break;
                    case Taal.EN:
                        municipalityRecord.EnglishName = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static async Task MunicipalityNameWasCleared(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            foreach (var municipalityName in envelope.Content.Object.Gemeentenamen)
            {
                switch (municipalityName.Taal)
                {
                    case Taal.NL:
                        municipalityRecord.DutchName = null;
                        break;
                    case Taal.FR:
                        municipalityRecord.FrenchName = null;
                        break;
                    case Taal.DE:
                        municipalityRecord.GermanName = null;
                        break;
                    case Taal.EN:
                        municipalityRecord.EnglishName = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static async Task MunicipalityWasNamed(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            foreach (var municipalityName in envelope.Content.Object.Gemeentenamen)
            {
                switch (municipalityName.Taal)
                {
                    case Taal.NL:
                        municipalityRecord.DutchName = municipalityName.Spelling;
                        break;
                    case Taal.FR:
                        municipalityRecord.FrenchName = municipalityName.Spelling;
                        break;
                    case Taal.DE:
                        municipalityRecord.GermanName = municipalityName.Spelling;
                        break;
                    case Taal.EN:
                        municipalityRecord.EnglishName = municipalityName.Spelling;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static async Task MunicipalityNameWasCorrected(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            foreach (var municipalityName in envelope.Content.Object.Gemeentenamen)
            {
                switch (municipalityName.Taal)
                {
                    case Taal.NL:
                        municipalityRecord.DutchName = municipalityName.Spelling;
                        break;
                    case Taal.FR:
                        municipalityRecord.FrenchName = municipalityName.Spelling;
                        break;
                    case Taal.DE:
                        municipalityRecord.GermanName = municipalityName.Spelling;
                        break;
                    case Taal.EN:
                        municipalityRecord.EnglishName = municipalityName.Spelling;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static async Task MunicipalityNisCodeWasCorrected(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            municipalityRecord.NisCode = envelope.Content.Object.Identificator.Id;
        }

        private static async Task MunicipalityNisCodeWasDefined(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            var municipalityRecord = await context.Municipalities.FindAsync(envelope.Content.Object.Id);
            if (municipalityRecord == null)
                return;

            municipalityRecord.NisCode = envelope.Content.Object.Identificator.Id;
        }

        private static async Task MunicipalityWasRegistered(SyndicationContext context, AtomEntry<SyndicationContent<Gemeente>> envelope, CancellationToken token)
        {
            await context.Municipalities.AddAsync(
                new MunicipalityRecord
                {
                    MunicipalityId = envelope.Content.Object.Id,
                    NisCode = envelope.Content.Object.Identificator.ObjectId,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    MunicipalityStatus = MunicipalityStatus.Registered
                }, token);
        }
    }

    public enum MunicipalityEvent
    {
        MunicipalityWasRegistered,
        MunicipalityWasNamed,
        MunicipalityNameWasCleared,
        MunicipalityNameWasCorrected,
        MunicipalityNameWasCorrectedToCleared,
        MunicipalityNisCodeWasDefined,
        MunicipalityNisCodeWasCorrected,
        MunicipalityOfficialLanguageWasAdded,
        MunicipalityOfficialLanguageWasRemoved,
        MunicipalityFacilityLanguageWasAdded,
        MunicipalityFacilityLanguageWasRemoved,
        MunicipalityBecameCurrent,
        MunicipalityWasCorrectedToCurrent,
        MunicipalityWasRetired,
        MunicipalityWasCorrectedToRetired,
    }

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent<T>
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public T Object { get; set; }
    }

    [DataContract(Name = "Gemeente", Namespace = "")]
    public class Gemeente
    {
        [DataMember(Name = "Id", Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Name = "Identificator", Order = 2)]
        public GemeenteIdentificator Identificator { get; set; }

        [DataMember(Name = "OfficieleTalen", Order = 3)]
        public List<Taal> OfficialLanguages { get; set; }

        [DataMember(Name = "FaciliteitenTalen", Order = 4)]
        public List<Taal> FacilitiesLanguages { get; set; }

        [DataMember(Name = "Gemeentenamen", Order = 5)]
        public List<GeografischeNaam> Gemeentenamen { get; set; }

        [DataMember(Name = "GemeenteStatus", Order = 6)]
        public GemeenteStatus? GemeenteStatus { get; set; }

        public Gemeente()
        {
            Gemeentenamen = new List<GeografischeNaam>();
            OfficialLanguages = new List<Taal>();
            FacilitiesLanguages = new List<Taal>();
        }
    }
}
