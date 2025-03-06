namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentsRemoved: IHaveHash
{
    public const string EventName = "RoadSegmentsRemoved";

    public int[] Ids { get; set; }
    public string GeometryDrawMethod { get; set; }

    // todo-pr add removed roadnodes
    // todo-pr add roadnodes with changed type

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
