namespace MartenPoc
{
    using System;

    public sealed record Wegsegment(
        Guid Id,
        string Geometry,
        Guid BeginknoopId,
        Guid EindknoopId,
        string Beheerder)
    {
        public static Wegsegment Create(WegsegmentWerdToegevoegd werdToegevoegd) =>
            new(
                werdToegevoegd.Id,
                werdToegevoegd.Geometry,
                werdToegevoegd.StartNodeId,
                werdToegevoegd.EndNodeId,
                werdToegevoegd.Beheerder);

        public Wegsegment Apply(WegsegmentWerdGewijzigd geometryWerdGewijzigd) =>
            this with
            {
                Geometry = geometryWerdGewijzigd.Geometry
            };
    }
}
