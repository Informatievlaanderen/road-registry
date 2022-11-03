namespace RoadRegistry.BackOffice.Handlers.Kafka.Tests.Projections;

using Amazon.DynamoDBv2.Model;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Kafka.Projections;
using Polly;

public class ConnectedProjectionFixture<TProjection, TConnection>
    where TProjection : ConnectedProjection<TConnection>, new()
{
    private readonly TConnection _connection;
    private readonly ConnectedProjector<TConnection> _projector;

    public ConnectedProjectionFixture(ConnectedProjectionHandlerResolver<TConnection> resolver)
    {
        _projector = new ConnectedProjector<TConnection>(resolver);
    }

    public TProjection Projection { get; init; }

    public Task ProjectAsync(object message) => _projector.ProjectAsync(_connection, message);
    public Task ProjectAsync(object message, CancellationToken cancellationToken) => _projector.ProjectAsync(_connection, message, cancellationToken);
    public Task ProjectAsync(IEnumerable<object> messages) => _projector.ProjectAsync(_connection, messages);
    public Task ProjectAsync(IEnumerable<object> messages, CancellationToken cancellationToken) => _projector.ProjectAsync(_connection, messages, cancellationToken);
}


public class StreetNameConsumerProjectionFixture : ConnectedProjectionFixture<StreetNameConsumerProjection, StreetNameConsumerContext>
{
    public StreetNameConsumerProjectionFixture() : base(Resolve.WhenEqualToHandlerMessageType(new StreetNameConsumerProjection().Handlers))
    {
    }
}



public class StreetNameConsumerProjectionTests : IClassFixture<StreetNameConsumerProjectionFixture>
{
    private readonly StreetNameConsumerProjectionFixture _fixture;

    public StreetNameConsumerProjectionTests(StreetNameConsumerProjectionFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ItShouldProject_StreetNameWasRegisteredAsync()
    {
        await _fixture.ProjectAsync(new StreetNameWasRegistered(
            "",
            "",
            "",
            new Provenance(
                "",
                "",
                "",
                "",
                "")
            )
        );
    }
}

//When<StreetNameWasRegistered>(async (context, message, token) =>
//        {
//            await context.StreetNames.AddAsync(
//                new StreetNameConsumerItem
//                {
//                    StreetNameId = message.StreetNameId,
//                    PersistentLocalId = null,
//                    MunicipalityId = message.MunicipalityId,
//                    NisCode = message.NisCode,
//                    Name = null,
//                    DutchName = null,
//                    FrenchName = null,
//                    GermanName = null,
//                    EnglishName = null,
//                    StreetNameStatus = null
//                    //Position = message.Position
//                }, token);
//        });

//        When<StreetNameBecameCurrent>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                record => { record.StreetNameStatus = StreetNameStatus.Current; },
//                token);
//        });

//        When<StreetNameHomonymAdditionWasCleared>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchHomonymAddition = null;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchHomonymAddition = null;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanHomonymAddition = null;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishHomonymAddition = null;
//                            break;
//                        case null:
//                            streetNameConsumerItem.HomonymAddition = null;
//                            break;
//                    }
//                },
//                token);
//        });

//        When<StreetNameHomonymAdditionWasCorrected>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchHomonymAddition = message.HomonymAddition;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchHomonymAddition = message.HomonymAddition;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanHomonymAddition = message.HomonymAddition;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishHomonymAddition = message.HomonymAddition;
//                            break;
//                        case null:
//                            streetNameConsumerItem.HomonymAddition = message.HomonymAddition;
//                            break;
//                    }
//                },
//                token);
//        });

//        When<StreetNameHomonymAdditionWasCorrectedToCleared>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchHomonymAddition = null;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchHomonymAddition = null;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanHomonymAddition = null;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishHomonymAddition = null;
//                            break;
//                        case null:
//                            streetNameConsumerItem.HomonymAddition = null;
//                            break;
//                    }
//                },
//                token);
//        });

//        When<StreetNameHomonymAdditionWasDefined>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchHomonymAddition = message.HomonymAddition;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchHomonymAddition = message.HomonymAddition;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanHomonymAddition = message.HomonymAddition;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishHomonymAddition = message.HomonymAddition;
//                            break;
//                        case null:
//                            streetNameConsumerItem.HomonymAddition = message.HomonymAddition;
//                            break;
//                    }
//                },
//                token);
//        });

//        When<StreetNameNameWasCleared>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchName = null;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchName = null;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanName = null;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishName = null;
//                            break;
//                        case null:
//                            streetNameConsumerItem.Name = null;
//                            break;
//                    }
//                },
//                token);
//        });

//        When<StreetNameNameWasCorrected>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchName = message.Name;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchName = message.Name;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanName = message.Name;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishName = message.Name;
//                            break;
//                        case null:
//                            streetNameConsumerItem.Name = message.Name;
//                            break;
//                    }
//                },
//                token);
//        });

//        When<StreetNameNameWasCorrectedToCleared>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchName = null;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchName = null;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanName = null;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishName = null;
//                            break;
//                        case null:
//                            streetNameConsumerItem.Name = null;
//                            break;
//                    }
//                },
//                token);
//        });

//        //When<StreetNamePersistentLocalIdentifierWasAssigned>(async (context, message, token) =>
//        //{
//        //    await UpdateStreetNameConsumerItem(
//        //        context,
//        //        message,
//        //        message.StreetNameId,
//        //        StreetNameConsumerItem => { StreetNameConsumerItem.PersistentLocalId = message.PersistentLocalId; },
//        //        token);
//        //});

//        When<StreetNameStatusWasRemoved>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = null; },
//                token);
//        });

//        When<StreetNameStatusWasCorrectedToRemoved>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = null; },
//                token);
//        });

//        When<StreetNameWasCorrectedToCurrent>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Current; },
//                token);
//        });

//        When<StreetNameWasProposed>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Proposed; },
//                token);
//        });

//        When<StreetNameWasRetired>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Retired; },
//                token);
//        });

//        When<StreetNameWasCorrectedToProposed>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Proposed; },
//                token);
//        });

//        When<StreetNameWasCorrectedToRetired>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Retired; },
//                token);
//        });

//        When<StreetNameWasNamed>(async (context, message, token) =>
//        {
//            await UpdateStreetNameConsumerItem(
//                context,
//                message,
//                message.StreetNameId,
//                streetNameConsumerItem =>
//                {
//                    switch (ParseLanguage(message.Language))
//                    {
//                        case StreetNameLanguage.Dutch:
//                            streetNameConsumerItem.DutchName = message.Name;
//                            break;
//                        case StreetNameLanguage.French:
//                            streetNameConsumerItem.FrenchName = message.Name;
//                            break;
//                        case StreetNameLanguage.German:
//                            streetNameConsumerItem.GermanName = message.Name;
//                            break;
//                        case StreetNameLanguage.English:
//                            streetNameConsumerItem.EnglishName = message.Name;
//                            break;
//                        case null:
//                            streetNameConsumerItem.Name = message.Name;
//                            break;
//                    }
//                },
//                token);
//        });
//    }

