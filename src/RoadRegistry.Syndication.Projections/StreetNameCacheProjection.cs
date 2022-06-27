namespace RoadRegistry.Syndication.Projections
{
    using System;
    using System.Threading;
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
                        Position = envelope.Position,
                    }, token);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    record =>
                    {
                        record.StreetNameStatus = StreetNameStatus.Current;
                    },
                    token);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });

            When<Envelope<StreetNamePersistentLocalIdentifierWasAssigned>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.PersistentLocalId = envelope.Message.PersistentLocalId;
                    },
                    token);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.StreetNameStatus = null;
                    },
                    token);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.StreetNameStatus = null;
                    },
                    token);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.StreetNameStatus = StreetNameStatus.Current;
                    },
                    token);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed;
                    },
                    token);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.StreetNameStatus = StreetNameStatus.Retired;
                    },
                    token);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed;
                    },
                    token);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
                        streetNameRecord.StreetNameStatus = StreetNameStatus.Retired;
                    },
                    token);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord =>
                    {
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
                    },
                    token);
            });
        }

        private static async Task UpdateStreetNameRecord<T>(SyndicationContext context, Envelope<T> envelope, Guid streetNameId, Action<StreetNameRecord> update, CancellationToken token)
        {
            var streetNameRecord = await FindOrThrow(context, streetNameId, token);

            update(streetNameRecord);

            streetNameRecord.Position = envelope.Position;
        }

        private static async Task<StreetNameRecord> FindOrThrow(SyndicationContext context, Guid streetNameId, CancellationToken token)
        {
            var streetNameRecord = await context.StreetNames.FindAsync(streetNameId, cancellationToken: token);
            if (streetNameRecord == null)
            {
                throw new InvalidOperationException($"No street name with id {streetNameId} was found.");
            }

            return streetNameRecord;
        }
    }
}
