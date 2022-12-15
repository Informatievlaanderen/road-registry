namespace RoadRegistry.Dbase.GradeSeparatedJuntions;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.GradeSeparatedJunctionDbaseRecord
{
    public new static readonly GradeSeparatedJunctionDbaseSchema Schema = new();

    public GradeSeparatedJunctionDbaseRecord()
    {
        LBLTYPE = new DbaseString(Schema.LBLTYPE);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            LBLTYPE,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }
    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseString LBLTYPE { get; }
}
