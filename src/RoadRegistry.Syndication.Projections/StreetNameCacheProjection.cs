namespace RoadRegistry.Syndication.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;
    using StreetNameEvents;

    public class StreetNameCacheProjection : ConnectedProjection<SyndicationContext>
    {
        public StreetNameCacheProjection()
        {
            When<Envelope<StreetNameWasRegistered>>(async (context, envelope, token) =>
            {
                await context.StreetNames.AddAsync(
                    new StreetNameRecord
                    {
                        StreetNameId = envelope.Message.StreetNameId,
                        PersistentLocalId = null,
                        MunicipalityId = envelope.Message.MunicipalityId,
                        NisCode = envelope.Message.NisCode,
                        Name = null,
                        DutchName = null,
                        FrenchName = null,
                        GermanName = null,
                        EnglishName = null,
                        StreetNameStatus = null,
                    }, token);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = StreetNameStatus.Current;
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, envelope, token) => { });

            When<Envelope<StreetNameNameWasCleared>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchName = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchName = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanName = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishName = null;
                        break;
                    case null:
                        streetNameRecord.Name = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchName = envelope.Message.Name;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchName = envelope.Message.Name;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanName = envelope.Message.Name;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishName = envelope.Message.Name;
                        break;
                    case null:
                        streetNameRecord.Name = envelope.Message.Name;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchName = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchName = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanName = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishName = null;
                        break;
                    case null:
                        streetNameRecord.Name = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            When<Envelope<StreetNamePersistentLocalIdentifierWasAssigned>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.PersistentLocalId = envelope.Message.PersistentLocalId;
            });

            When<Envelope<StreetNamePrimaryLanguageWasCleared>>(async (context, envelope, token) => { });
            When<Envelope<StreetNamePrimaryLanguageWasCorrected>>(async (context, envelope, token) => { });
            When<Envelope<StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (context, envelope, token) => { });
            When<Envelope<StreetNamePrimaryLanguageWasDefined>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameSecondaryLanguageWasCleared>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameSecondaryLanguageWasCorrected>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameSecondaryLanguageWasDefined>>(async (context, envelope, token) => { });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = null;
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = null;
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = StreetNameStatus.Current;
            });

            When<Envelope<StreetNameWasProposed>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed;
            });

            When<Envelope<StreetNameWasRetired>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = StreetNameStatus.Retired;
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed;
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                streetNameRecord.StreetNameStatus = StreetNameStatus.Retired;
            });

            When<Envelope<StreetNameWasNamed>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await context.StreetNames.FindAsync(envelope.Message.StreetNameId);
                if (streetNameRecord == null)
                    return;

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchName = envelope.Message.Name;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchName = envelope.Message.Name;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanName = envelope.Message.Name;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishName = envelope.Message.Name;
                        break;
                    case null:
                        streetNameRecord.Name = envelope.Message.Name;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameBecameIncomplete>>(async (context, envelope, token) => { });
        }
    }
}
