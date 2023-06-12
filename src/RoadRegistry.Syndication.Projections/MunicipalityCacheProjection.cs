namespace RoadRegistry.Syndication.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using MunicipalityEvents;
using RTools_NTS.Util;
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
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            switch (envelope.Message.Language)
            {
                case MunicipalityLanguage.Dutch:
                    municipalityRecord.DutchName = envelope.Message.Name;
                    break;
                case MunicipalityLanguage.French:
                    municipalityRecord.FrenchName = envelope.Message.Name;
                    break;
                case MunicipalityLanguage.German:
                    municipalityRecord.GermanName = envelope.Message.Name;
                    break;
                case MunicipalityLanguage.English:
                    municipalityRecord.EnglishName = envelope.Message.Name;
                    break;
            }
        });

        When<Envelope<MunicipalityNameWasCleared>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            switch (envelope.Message.Language)
            {
                case MunicipalityLanguage.Dutch:
                    municipalityRecord.DutchName = null;
                    break;
                case MunicipalityLanguage.French:
                    municipalityRecord.FrenchName = null;
                    break;
                case MunicipalityLanguage.German:
                    municipalityRecord.GermanName = null;
                    break;
                case MunicipalityLanguage.English:
                    municipalityRecord.EnglishName = null;
                    break;
            }
        });

        When<Envelope<MunicipalityNameWasCorrected>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            switch (envelope.Message.Language)
            {
                case MunicipalityLanguage.Dutch:
                    municipalityRecord.DutchName = envelope.Message.Name;
                    break;
                case MunicipalityLanguage.French:
                    municipalityRecord.FrenchName = envelope.Message.Name;
                    break;
                case MunicipalityLanguage.German:
                    municipalityRecord.GermanName = envelope.Message.Name;
                    break;
                case MunicipalityLanguage.English:
                    municipalityRecord.EnglishName = envelope.Message.Name;
                    break;
            }
        });

        When<Envelope<MunicipalityNameWasCorrectedToCleared>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            switch (envelope.Message.Language)
            {
                case MunicipalityLanguage.Dutch:
                    municipalityRecord.DutchName = null;
                    break;
                case MunicipalityLanguage.French:
                    municipalityRecord.FrenchName = null;
                    break;
                case MunicipalityLanguage.German:
                    municipalityRecord.GermanName = null;
                    break;
                case MunicipalityLanguage.English:
                    municipalityRecord.EnglishName = null;
                    break;
            }
        });

        When<Envelope<MunicipalityNisCodeWasDefined>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            municipalityRecord.NisCode = envelope.Message.NisCode;
        });

        When<Envelope<MunicipalityNisCodeWasCorrected>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            municipalityRecord.NisCode = envelope.Message.NisCode;
        });

        When<Envelope<MunicipalityBecameCurrent>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Current;
        });

        When<Envelope<MunicipalityWasCorrectedToCurrent>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Current;
        });

        When<Envelope<MunicipalityWasRetired>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId);

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Retired;
        });

        When<Envelope<MunicipalityWasCorrectedToRetired>>(async (context, envelope, token) =>
        {
            var municipalityRecord = await FindOrThrow(context, envelope.Message.MunicipalityId, token);

            municipalityRecord.MunicipalityStatus = MunicipalityStatus.Retired;
        });
    }

    private static async Task<MunicipalityRecord> FindOrThrow(SyndicationContext context, Guid municipalityId, CancellationToken token)
    {
        var municipalityRecord = await context.Municipalities.FindAsync(municipalityId, cancellationToken: token).ConfigureAwait(false);
        if (municipalityRecord == null) throw new InvalidOperationException($"No municipality with id {municipalityId} was found.");

        return municipalityRecord;
    }
}
