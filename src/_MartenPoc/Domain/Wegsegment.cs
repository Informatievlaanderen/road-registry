namespace MartenPoc.Domain
{
    using System;

    public sealed record Wegsegment(
        Guid Id,
        int Version,
        string Geometry,
        Guid BeginknoopId,
        Guid EindknoopId,
        string Beheerder,
        string Attribuut1,
        string Attribuut2,
        string Attribuut3,
        string Attribuut4,
        string Attribuut5,
        string Attribuut6,
        string Attribuut7,
        string Attribuut8)
    {
        public static Wegsegment Create(WegsegmentWerdToegevoegd werdToegevoegd) =>
            new(
                werdToegevoegd.Id,
                1,
                werdToegevoegd.Geometry,
                werdToegevoegd.StartNodeId,
                werdToegevoegd.EndNodeId,
                werdToegevoegd.Beheerder,
                werdToegevoegd.Attribuut1,
                werdToegevoegd.Attribuut2,
                werdToegevoegd.Attribuut3,
                werdToegevoegd.Attribuut4,
                werdToegevoegd.Attribuut5,
                werdToegevoegd.Attribuut6,
                werdToegevoegd.Attribuut7,
                werdToegevoegd.Attribuut8);

        public Wegsegment Apply(WegsegmentWerdGewijzigd werdGewijzigd) =>
            this with
            {
                Version = Version + 1,
                Geometry = werdGewijzigd.Geometry,
                Beheerder = werdGewijzigd.Beheerder,
                Attribuut1 = werdGewijzigd.Attribuut1,
                Attribuut2 = werdGewijzigd.Attribuut2,
                Attribuut3 = werdGewijzigd.Attribuut3,
                Attribuut4 = werdGewijzigd.Attribuut4,
                Attribuut5 = werdGewijzigd.Attribuut5,
                Attribuut6 = werdGewijzigd.Attribuut6,
                Attribuut7 = werdGewijzigd.Attribuut7,
                Attribuut8 = werdGewijzigd.Attribuut8
            };
    }
}
