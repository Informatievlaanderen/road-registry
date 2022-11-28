namespace RoadRegistry.StreetNameConsumer.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Schema;

public class StreetNameConsumerProjection : ConnectedProjection<StreetNameConsumerContext>
{
    public StreetNameConsumerProjection()
    {
        When<StreetNameWasRegistered>(StreetNameWasRegistered);
        When<StreetNameBecameCurrent>(StreetNameBecameCurrent);
        When<StreetNameHomonymAdditionWasCleared>(StreetNameHomonymAdditionWasCleared);
        When<StreetNameHomonymAdditionWasCorrected>(StreetNameHomonymAdditionWasCorrected);
        When<StreetNameHomonymAdditionWasCorrectedToCleared>(StreetNameHomonymAdditionWasCorrectedToCleared);
        When<StreetNameHomonymAdditionWasDefined>(StreetNameHomonymAdditionWasDefined);
        When<StreetNameNameWasCleared>(StreetNameNameWasCleared);
        When<StreetNameNameWasCorrected>(StreetNameNameWasCorrected);
        When<StreetNameNameWasCorrectedToCleared>(StreetNameNameWasCorrectedToCleared);
        When<StreetNameStatusWasRemoved>(StreetNameStatusWasRemoved);
        When<StreetNameStatusWasCorrectedToRemoved>(StreetNameStatusWasCorrectedToRemoved);
        When<StreetNameWasCorrectedToCurrent>(StreetNameWasCorrectedToCurrent);
        When<StreetNameWasProposed>(StreetNameWasProposed);
        When<StreetNameWasRetired>(StreetNameWasRetired);
        When<StreetNameWasCorrectedToProposed>(StreetNameWasCorrectedToProposed);
        When<StreetNameWasCorrectedToRetired>(StreetNameWasCorrectedToRetired);
        When<StreetNameWasNamed>(StreetNameWasNamed);
    }

    private static async Task<StreetNameConsumerItem> FindOrThrow(StreetNameConsumerContext context, string streetNameId, CancellationToken token)
    {
        var streetNameConsumerItem = await context.StreetNames.FindAsync(streetNameId, cancellationToken: token);
        if (streetNameConsumerItem == null)
        {
            throw new InvalidOperationException($"No street name with id {streetNameId} was found.");
        }

        return streetNameConsumerItem;
    }

    private static StreetNameLanguage? ParseLanguage(string? language)
    {
        if (Enum.TryParse(language, true, out StreetNameLanguage streetNameLanguage))
        {
            return streetNameLanguage;
        }

        return null;
    }

    private async Task StreetNameBecameCurrent(StreetNameConsumerContext context, StreetNameBecameCurrent message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            record => { record.StreetNameStatus = StreetNameStatus.Current; },
            token);
    }

    private async Task StreetNameHomonymAdditionWasCleared(StreetNameConsumerContext context, StreetNameHomonymAdditionWasCleared message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanHomonymAddition = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishHomonymAddition = null;
                        break;
                    case null:
                        streetNameConsumerItem.HomonymAddition = null;
                        break;
                }
            },
            token).ConfigureAwait(false);
    }

    private async Task StreetNameHomonymAdditionWasCorrected(StreetNameConsumerContext context, StreetNameHomonymAdditionWasCorrected message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchHomonymAddition = message.HomonymAddition;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchHomonymAddition = message.HomonymAddition;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanHomonymAddition = message.HomonymAddition;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishHomonymAddition = message.HomonymAddition;
                        break;
                    case null:
                        streetNameConsumerItem.HomonymAddition = message.HomonymAddition;
                        break;
                }
            },
            token);
    }

    private async Task StreetNameHomonymAdditionWasCorrectedToCleared(StreetNameConsumerContext context, StreetNameHomonymAdditionWasCorrectedToCleared message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchHomonymAddition = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanHomonymAddition = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishHomonymAddition = null;
                        break;
                    case null:
                        streetNameConsumerItem.HomonymAddition = null;
                        break;
                }
            },
            token);
    }

    private async Task StreetNameHomonymAdditionWasDefined(StreetNameConsumerContext context, StreetNameHomonymAdditionWasDefined message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchHomonymAddition = message.HomonymAddition;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchHomonymAddition = message.HomonymAddition;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanHomonymAddition = message.HomonymAddition;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishHomonymAddition = message.HomonymAddition;
                        break;
                    case null:
                        streetNameConsumerItem.HomonymAddition = message.HomonymAddition;
                        break;
                }
            },
            token);
    }

    private async Task StreetNameNameWasCleared(StreetNameConsumerContext context, StreetNameNameWasCleared message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchName = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchName = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanName = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishName = null;
                        break;
                    case null:
                        streetNameConsumerItem.Name = null;
                        break;
                }
            },
            token);
    }

    private async Task StreetNameNameWasCorrected(StreetNameConsumerContext context, StreetNameNameWasCorrected message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchName = message.Name;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchName = message.Name;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanName = message.Name;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishName = message.Name;
                        break;
                    case null:
                        streetNameConsumerItem.Name = message.Name;
                        break;
                }
            },
            token);
    }

    private async Task StreetNameNameWasCorrectedToCleared(StreetNameConsumerContext context, StreetNameNameWasCorrectedToCleared message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchName = null;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchName = null;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanName = null;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishName = null;
                        break;
                    case null:
                        streetNameConsumerItem.Name = null;
                        break;
                }
            },
            token);
    }

    private async Task StreetNameStatusWasCorrectedToRemoved(StreetNameConsumerContext context, StreetNameStatusWasCorrectedToRemoved message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = null; },
            token);
    }

    private async Task StreetNameStatusWasRemoved(StreetNameConsumerContext context, StreetNameStatusWasRemoved message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = null; },
            token);
    }

    private async Task StreetNameWasCorrectedToCurrent(StreetNameConsumerContext context, StreetNameWasCorrectedToCurrent message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Current; },
            token);
    }

    private async Task StreetNameWasCorrectedToProposed(StreetNameConsumerContext context, StreetNameWasCorrectedToProposed message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Proposed; },
            token);
    }

    private async Task StreetNameWasCorrectedToRetired(StreetNameConsumerContext context, StreetNameWasCorrectedToRetired message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Retired; },
            token);
    }

    private async Task StreetNameWasNamed(StreetNameConsumerContext context, StreetNameWasNamed message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem =>
            {
                switch (ParseLanguage(message.Language))
                {
                    case StreetNameLanguage.Dutch:
                        streetNameConsumerItem.DutchName = message.Name;
                        break;
                    case StreetNameLanguage.French:
                        streetNameConsumerItem.FrenchName = message.Name;
                        break;
                    case StreetNameLanguage.German:
                        streetNameConsumerItem.GermanName = message.Name;
                        break;
                    case StreetNameLanguage.English:
                        streetNameConsumerItem.EnglishName = message.Name;
                        break;
                    case null:
                        streetNameConsumerItem.Name = message.Name;
                        break;
                }
            },
            token);
    }

    private async Task StreetNameWasProposed(StreetNameConsumerContext context, StreetNameWasProposed message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Proposed; },
            token);
    }

    private async Task StreetNameWasRegistered(StreetNameConsumerContext context, StreetNameWasRegistered message, CancellationToken token)
    {
        await context.StreetNames.AddAsync(
            new StreetNameConsumerItem
            {
                StreetNameId = message.StreetNameId,
                PersistentLocalId = null,
                MunicipalityId = message.MunicipalityId,
                NisCode = message.NisCode,
                Name = null,
                DutchName = null,
                FrenchName = null,
                GermanName = null,
                EnglishName = null,
                StreetNameStatus = null
            }, token);
    }

    private async Task StreetNameWasRetired(StreetNameConsumerContext context, StreetNameWasRetired message, CancellationToken token)
    {
        await UpdateStreetNameConsumerItem(
            context,
            message.StreetNameId,
            streetNameConsumerItem => { streetNameConsumerItem.StreetNameStatus = StreetNameStatus.Retired; },
            token);
    }

    private static async Task UpdateStreetNameConsumerItem(StreetNameConsumerContext context, string streetNameId, Action<StreetNameConsumerItem> update, CancellationToken token)
    {
        var streetNameConsumerItem = await FindOrThrow(context, streetNameId, token);

        update(streetNameConsumerItem);
    }
}
