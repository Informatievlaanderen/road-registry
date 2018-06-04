namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum RoadNodeType
    {
        RealNode = 1, // 1	echte knoop	Punt waar 2 wegsegmenten elkaar snijden; minstens drie aansluitende wegsegmenten.
        FakeNode = 2, // 2	schijnknoop	Punt waar 2 wegsegmenten elkaar raken; slechts twee aansluitende wegsegmenten.
        EndNode = 3, // 3	eindknoop	Het einde van een doodlopende wegcorridor, slechts één aansluitend wegsegment.
        MiniRoundabout = 4, // 4	minirotonde	Kruispunt dat zich in de realiteit voordoet als een rotonde maar niet voldoet aan de geometrische specificaties om opgenomen te worden als een echte rotonde (ringvormige geometrie).
        TurnLoopNode = 5 // 5	keerlusknoop	Juist twee aansluitende wegsegmenten; wegsegmenten die aan beide zijden begrensd worden door dezelfde wegknoop worden met behulp van een extra wegknoop (= keerlusknoop) opgesplitst.
    }

    public class RoadNodeTypeTranslator : EnumTranslator<RoadNodeType>
    {
        protected override IDictionary<RoadNodeType, string> DutchTranslations => _dutchTranslations;
        private static readonly Dictionary<RoadNodeType, string> _dutchTranslations = new Dictionary<RoadNodeType, string>
        {
            { RoadNodeType.RealNode, "echte knoop" },
            { RoadNodeType.FakeNode, "schijnknoop" },
            { RoadNodeType.EndNode, "eindknoop" },
            { RoadNodeType.MiniRoundabout, "minirotonde" },
            { RoadNodeType.TurnLoopNode, "keerlusknoop" },
        };
    }
}
