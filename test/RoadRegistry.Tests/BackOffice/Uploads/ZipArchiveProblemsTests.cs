namespace RoadRegistry.Tests.BackOffice.Uploads;

using FluentAssertions;
using RoadRegistry.Extracts.Uploads;

public class ZipArchiveProblemsTests
{
    // The readers drop the record problems of the extract and the integration data - the system generates those
    // files - and keep what this answers. A record that cannot be read belongs in there: it stops the reader where
    // it stands, so every record behind it is never seen, and what is left is not a file an upload can be judged
    // against. Dropped, the feature compare works from an extract it believes is complete.
    [Fact]
    public void RecordFormatErrorsAreKept()
    {
        var recordCannotBeRead = new FileError("EWEGSEGMENT.DBF", nameof(DbaseFileProblems.HasDbaseRecordFormatError));
        var recordIsIncomplete = new FileError("EWEGSEGMENT.DBF", nameof(DbaseFileProblems.RequiredFieldIsNull));

        var problems = ZipArchiveProblems.None
            .Add(recordCannotBeRead)
            .Add(recordIsIncomplete);

        problems.GetMissingOrInvalidFileProblems().Should().BeEquivalentTo([recordCannotBeRead]);
    }
}
