namespace RoadRegistry.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class RoadSegment
        {
            public static class NotFound
            {
                public const string Code = Common.NotFound.Code;
                public const string Message = "Onbestaand wegsegment.";
            }

            public static class LeftStreetNameIsNotUnlinked
            {
                public const string Code = "LinkerstraatnaamNietOntkoppeld";
                public static string Message(int roadSegmentId) => $"Het wegsegment '{roadSegmentId}' heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.";
            }

            public static class LeftStreetNameIsNotLinked
            {
                public const string Code = "LinkerstraatnaamNietGekoppeld";
                public static string Message(int roadSegmentId, string linkerstraatnaamPuri) => $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de linkerstraatnaam '{linkerstraatnaamPuri}'";
            }

            public static class RightStreetNameIsNotUnlinked
            {
                public const string Code = "RechterstraatnaamNietOntkoppeld";
                public static string Message(int roadSegmentId) => $"Het wegsegment '{roadSegmentId}' heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.";
            }

            public static class RightStreetNameIsNotLinked
            {
                public const string Code = "RechterstraatnaamNietGekoppeld";
                public static string Message(int roadSegmentId, string rechterstraatnaamPuri) => $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de rechterstraatnaam '{rechterstraatnaamPuri}'";
            }

            public static class StreetNameIsNotProposedOrCurrent
            {
                public const string Code = "WegsegmentStraatnaamNietVoorgesteldOfInGebruik";
                public const string Message = "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.";
            }
        }
    }
}
