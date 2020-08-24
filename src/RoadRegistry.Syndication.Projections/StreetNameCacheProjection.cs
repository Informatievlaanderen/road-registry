namespace RoadRegistry.Syndication.Projections
{
    using System;
    using System.Threading.Tasks;
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
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = StreetNameStatus.Current;
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanHomonymAddition = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishHomonymAddition = null;
                        break;
                    case null:
                        streetNameRecord.HomonymAddition = null;
                        break;
                }
            });
            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case null:
                        streetNameRecord.HomonymAddition = envelope.Message.HomonymAddition;
                        break;
                }
            });
            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanHomonymAddition = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishHomonymAddition = null;
                        break;
                    case null:
                        streetNameRecord.HomonymAddition = null;
                        break;
                }
            });
            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                switch (envelope.Message.Language)
                {
                    case StreetNameLanguage.Dutch:
                        streetNameRecord.DutchHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case StreetNameLanguage.French:
                        streetNameRecord.FrenchHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case StreetNameLanguage.German:
                        streetNameRecord.GermanHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case StreetNameLanguage.English:
                        streetNameRecord.EnglishHomonymAddition = envelope.Message.HomonymAddition;
                        break;
                    case null:
                        streetNameRecord.HomonymAddition = envelope.Message.HomonymAddition;
                        break;
                }
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

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
                }
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

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
                }
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

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
                }
            });

            When<Envelope<StreetNamePersistentLocalIdentifierWasAssigned>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

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
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = null;
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = null;
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = StreetNameStatus.Current;
            });

            When<Envelope<StreetNameWasProposed>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed;
            });

            When<Envelope<StreetNameWasRetired>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = StreetNameStatus.Retired;
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed;
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

                streetNameRecord.StreetNameStatus = StreetNameStatus.Retired;
            });

            When<Envelope<StreetNameWasNamed>>(async (context, envelope, token) =>
            {
                var streetNameRecord = await FindOrThrow(context, envelope.Message.StreetNameId);

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
                }
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, envelope, token) => { });
            When<Envelope<StreetNameBecameIncomplete>>(async (context, envelope, token) => { });
        }

        private static async Task<StreetNameRecord> FindOrThrow(SyndicationContext context, Guid streetNameId)
        {
            var streetNameRecord = await context.StreetNames.FindAsync(streetNameId);
            if (streetNameRecord == null)
                throw new Exception($"No street name with id {streetNameId} was found.");

            return streetNameRecord;
        }
    }
}
