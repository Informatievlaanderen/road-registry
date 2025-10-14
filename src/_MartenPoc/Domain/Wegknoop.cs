namespace MartenPoc.Domain
{
    using System;

    public sealed record Wegknoop(Guid Id, int Version, string Geometry, string Type)
    {
        public static Wegknoop Create(WegknoopWerdToegevoegd werdToegevoegd) =>
            new(
                werdToegevoegd.Id,
                1,
                werdToegevoegd.Geometry,
                werdToegevoegd.Type);

        public Wegknoop Apply(WegknoopWerdGewijzigd werdGewijzigd) =>
            this with
            {
                Version = Version + 1,
                Geometry = werdGewijzigd.Geometry,
                Type = werdGewijzigd.Type
            };
    }
}
