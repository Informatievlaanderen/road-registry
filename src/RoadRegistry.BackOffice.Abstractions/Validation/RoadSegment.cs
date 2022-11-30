namespace RoadRegistry.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class RoadSegment
        {
            public static class NotFound
            {
                public const string Code = "NotFound";
                public const string Message = "Onbestaand wegsegment.";
            }

            public static class LeftStreetNameIsNotAvailable
            {
                public const string Code = "LinkerstraatnaamNietOntkoppeldValidatie";
                public static string Message(int roadSegmentId) => $"Het wegsegment '{roadSegmentId}' heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.";
            }

            public static class RightStreetNameIsNotAvailable
            {
                public const string Code = "RechterstraatnaamNietOntkoppeldValidatie";
                public static string Message(int roadSegmentId) => $"Het wegsegment '{roadSegmentId}' heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.";
            }

            public static class StreetNameIsNotProposedOrCurrent
            {
                public const string Code = "WegsegmentStraatnaamNietVoorgesteldOfInGebruik";
                public const string Message = "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.";
            }
        }
    }
}
