namespace RoadRegistry.BackOffice.Dbase;

using Be.Vlaanderen.Basisregisters.Shaperon;

public interface IRoadSegmentDbaseRecord
{
    DbaseString BEHEERDER { get; }
    DbaseInt32 METHODE { get; }
    DbaseInt32 MORFOLOGIE { get; }
    DbaseString CATEGORIE { get; }
    DbaseInt32 STATUS { get; }
    DbaseInt32 TGBEP { get; }
    DbaseNullableInt32 LSTRNMID { get; }
    DbaseNullableInt32 RSTRNMID { get; }
    DbaseInt32 B_WK_OIDN { get; }
    DbaseInt32 E_WK_OIDN { get; }
}
