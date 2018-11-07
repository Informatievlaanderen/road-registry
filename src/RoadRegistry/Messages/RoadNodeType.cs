namespace RoadRegistry.Messages
{
    using System.Collections.Generic;

    public enum RoadNodeType
    {
        RealNode = 1,
        FakeNode = 2,
        EndNode = 3,
        MiniRoundabout = 4,
        TurnLoopNode = 5
    }

    public class RoadNodeTypeTranslator : EnumTranslator<RoadNodeType>
    {
        protected override IDictionary<RoadNodeType, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<RoadNodeType, string> DutchDescriptions => _dutchDescriptions;

        private static readonly Dictionary<RoadNodeType, string> _dutchTranslations =
            new Dictionary<RoadNodeType, string>
            {
                { RoadNodeType.RealNode, "echte knoop" },
                { RoadNodeType.FakeNode, "schijnknoop" },
                { RoadNodeType.EndNode, "eindknoop" },
                { RoadNodeType.MiniRoundabout, "minirotonde" },
                { RoadNodeType.TurnLoopNode, "keerlusknoop" },
            };

        private static readonly IDictionary<RoadNodeType, string> _dutchDescriptions =
            new Dictionary<RoadNodeType, string>
            {
                { RoadNodeType.RealNode, "Punt waar 2 wegsegmenten elkaar snijden; minstens drie aansluitende wegsegmenten." },
                { RoadNodeType.FakeNode, "Punt waar 2 wegsegmenten elkaar raken; slechts twee aansluitende wegsegmenten." },
                { RoadNodeType.EndNode, "Het einde van een doodlopende wegcorridor, slechts één aansluitend wegsegment." },
                { RoadNodeType.MiniRoundabout, "Kruispunt dat zich in de realiteit voordoet als een rotonde maar niet voldoet aan de geometrische specificaties om opgenomen te worden als een echte rotonde (ringvormige geometrie)." },
                { RoadNodeType.TurnLoopNode, "Juist twee aansluitende wegsegmenten; wegsegmenten die aan beide zijden begrensd worden door dezelfde wegknoop worden met behulp van een extra wegknoop (= keerlusknoop) opgesplitst." },
            };
    }
}
