namespace MartenPoc
{
    using System;

    public sealed record Wegsegment(
        Guid Id,
        int Version,
        string Geometry,
        Guid BeginknoopId,
        Guid EindknoopId,
        string Beheerder)
    {
        public static Wegsegment Create(WegsegmentWerdToegevoegd werdToegevoegd) =>
            new(
                werdToegevoegd.Id,
                1,
                werdToegevoegd.Geometry,
                werdToegevoegd.StartNodeId,
                werdToegevoegd.EndNodeId,
                werdToegevoegd.Beheerder);

        public Wegsegment Apply(WegsegmentWerdGewijzigd geometryWerdGewijzigd) =>
            this with
            {
                Version = Version + 1,
                Geometry = geometryWerdGewijzigd.Geometry
            };
    }
}
