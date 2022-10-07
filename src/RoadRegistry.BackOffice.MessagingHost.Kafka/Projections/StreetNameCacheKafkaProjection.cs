namespace RoadRegistry.BackOffice.MessagingHost.Kafka.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Confluent.Kafka;
    using RoadRegistry.Syndication.Schema;

    public class StreetNameCacheKafkaProjection : ConnectedProjection<ConsumerContext>
    {
        public StreetNameCacheKafkaProjection()
        {
            When<StreetNameWasRegistered>(async (context, envelope, token) =>
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
                        Position = envelope.Position
                    }, token);
            });

            When<StreetNameBecameCurrent>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    record => { record.StreetNameStatus = StreetNameStatus.Current; },
                    token);
            });

            When<StreetNameHomonymAdditionWasCleared>(async (context, envelope, token) =>
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

            When<StreetNameHomonymAdditionWasCorrected>(async (context, envelope, token) =>
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

            When<StreetNameHomonymAdditionWasCorrectedToCleared>(async (context, envelope, token) =>
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

            When<StreetNameHomonymAdditionWasDefined>(async (context, envelope, token) =>
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

            When<StreetNameNameWasCleared>(async (context, envelope, token) =>
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

            When<StreetNameNameWasCorrected>(async (context, envelope, token) =>
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

            When<StreetNameNameWasCorrectedToCleared>(async (context, envelope, token) =>
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

            //When<StreetNamePersistentLocalIdentifierWasAssigned>(async (context, envelope, token) =>
            //{
            //    await UpdateStreetNameRecord(
            //        context,
            //        envelope,
            //        envelope.Message.StreetNameId,
            //        streetNameRecord => { streetNameRecord.PersistentLocalId = envelope.Message.PersistentLocalId; },
            //        token);
            //});

            When<StreetNameStatusWasRemoved>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord => { streetNameRecord.StreetNameStatus = null; },
                    token);
            });

            When<StreetNameStatusWasCorrectedToRemoved>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord => { streetNameRecord.StreetNameStatus = null; },
                    token);
            });

            When<StreetNameWasCorrectedToCurrent>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord => { streetNameRecord.StreetNameStatus = StreetNameStatus.Current; },
                    token);
            });

            When<StreetNameWasProposed>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord => { streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed; },
                    token);
            });

            When<StreetNameWasRetired>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord => { streetNameRecord.StreetNameStatus = StreetNameStatus.Retired; },
                    token);
            });

            When<StreetNameWasCorrectedToProposed>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord => { streetNameRecord.StreetNameStatus = StreetNameStatus.Proposed; },
                    token);
            });

            When<StreetNameWasCorrectedToRetired>(async (context, envelope, token) =>
            {
                await UpdateStreetNameRecord(
                    context,
                    envelope,
                    envelope.Message.StreetNameId,
                    streetNameRecord => { streetNameRecord.StreetNameStatus = StreetNameStatus.Retired; },
                    token);
            });

            When<StreetNameWasNamed>(async (context, envelope, token) =>
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

        private static async Task UpdateStreetNameRecord<T>(SyndicationContext context, Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<T> envelope, Guid streetNameId, Action<StreetNameRecord> update, CancellationToken token)
            where T: IMessage
        {
            var streetNameRecord = await FindOrThrow(context, streetNameId, token);

            update(streetNameRecord);

            streetNameRecord.Position = envelope.Position;
        }

        private void HandlerFor<T>() where T : IQueueMessage => When<T>(async (commandHandler, message, ct) =>
        {
            var command = GetCommand(message);
            await commandHandler.Handle(command, ct);
        });

        private static async Task<StreetNameRecord> FindOrThrow(SyndicationContext context, Guid streetNameId, CancellationToken token)
        {
            var streetNameRecord = await context.StreetNames.FindAsync(streetNameId, cancellationToken: token);
            if (streetNameRecord == null) throw new InvalidOperationException($"No street name with id {streetNameId} was found.");

            return streetNameRecord;
        }
    }
}
