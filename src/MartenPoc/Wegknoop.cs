namespace MartenPoc
{
    using System;

    public sealed record Wegknoop(Guid Id, string Type)
    {
        public static Wegknoop Create(WegknoopWerdToegevoegd werdToegevoegd) =>
            new(
                werdToegevoegd.Id,
                werdToegevoegd.Type);

        public Wegknoop Apply(WegknoopWerdGewijzigd werdGewijzigd) =>
            this with
            {
                Type = werdGewijzigd.Type
            };
    }
}
