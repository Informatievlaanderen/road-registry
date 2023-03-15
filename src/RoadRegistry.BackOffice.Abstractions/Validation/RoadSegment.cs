namespace RoadRegistry.BackOffice.Abstractions.Validation;

public static partial class ValidationErrors
{
    public static class RoadSegment
    {
        public static class NotFound
        {
            public const string Code = Common.NotFound.Code;
            public const string Message = "Onbestaand wegsegment.";
            public static string FormattedMessage(IEnumerable<int> value) => $"Onbestaande of verwijderde wegsegmenten gevonden '{string.Join(", ", value)}'.";
        }

        public static class ChangeAttributesRequestNull
        {
            public const string Code = Common.NotFound.Code;
            public const string Message = "Ten minste één attribuut moet opgegeven worden.";
        }

        public static class StreetName
        {
            public static class Left
            {
                public static class NotLinked
                {
                    public const string Code = "LinkerstraatnaamNietGekoppeld";
                    public static string Message(int roadSegmentId, string linkerstraatnaamPuri) => $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de linkerstraatnaam '{linkerstraatnaamPuri}'";
                }

                public static class NotUnlinked
                {
                    public const string Code = "LinkerstraatnaamNietOntkoppeld";
                    public static string Message(int roadSegmentId) => $"Het wegsegment '{roadSegmentId}' heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.";
                }
            }

            public static class Right
            {
                public static class NotUnlinked
                {
                    public const string Code = "RechterstraatnaamNietOntkoppeld";
                    public static string Message(int roadSegmentId) => $"Het wegsegment '{roadSegmentId}' heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.";
                }

                public static class NotLinked
                {
                    public const string Code = "RechterstraatnaamNietGekoppeld";
                    public static string Message(int roadSegmentId, string rechterstraatnaamPuri) => $"Het wegsegment '{roadSegmentId}' is niet gekoppeld aan de rechterstraatnaam '{rechterstraatnaamPuri}'";
                }
            }

            public static class NotProposedOrCurrent
            {
                public const string Code = "WegsegmentStraatnaamNietVoorgesteldOfInGebruik";
                public const string Message = "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.";
            }
        }

        public static class Status
        {
            public static class NotParsed
            {
                public const string Code = "WegsegmentStatusNietCorrect";
                public static string Message(string value) => $"Wegsegment status is foutief. '{value}' is geen geldige waarde.";
            }
            public static class IsRequired
            {
                public const string Code = "WegsegmentStatusVerplicht";
                public const string Message = "Wegsegment status is verplicht.";
            }
        }

        public static class Morphology
        {
            public static class NotParsed
            {
                public const string Code = "MorfologischeWegklasseNietCorrect";
                public static string Message(string value) => $"Morfologische wegklasse is foutief. '{value}' is geen geldige waarde.";
            }
            public static class IsRequired
            {
                public const string Code = "MorfologischeWegklasseVerplicht";
                public const string Message = "Morfologische wegklasse is verplicht.";
            }
        }

        public static class AccessRestriction
        {
            public static class NotParsed
            {
                public const string Code = "ToegangsbeperkingNietCorrect";
                public static string Message(string value) => $"Toegangsbeperking is foutief. '{value}' is geen geldige waarde.";
            }
            public static class IsRequired
            {
                public const string Code = "ToegangsbeperkingVerplicht";
                public const string Message = "Toegangsbeperking is verplicht.";
            }
        }

        public static class Organization
        {
            public static class NotFound
            {
                public const string Code = "WegbeheerderNietCorrect";
                public static string Message(string value) => $"Wegbeheerder is foutief. '{value}' is geen geldige waarde.";
            }
            public static class IsRequired
            {
                public const string Code = "WegbeheerderVerplicht";
                public const string Message = "Wegbeheerder is verplicht.";
            }
        }

        public static class SurfaceType
        {
            public static class NotParsed
            {
                public const string Code = "WegverhardingNietCorrect";
                public static string Message(string value) => $"Wegverharding is foutief. '{value}' is geen geldige waarde.";
            }
            public static class IsRequired
            {
                public const string Code = "WegverhardingVerplicht";
                public const string Message = "Wegverharding is verplicht.";
            }
        }

        public static class Category
        {
            public static class NotParsed
            {
                public const string Code = "WegcategorieNietCorrect";
                public static string Message(string value) => $"Wegcategorie is foutief. '{value}' is geen geldige waarde.";
            }
            public static class IsRequired
            {
                public const string Code = "WegcategorieVerplicht";
                public const string Message = "Wegcategorie is verplicht.";
            }
        }

        public static class Width
        {
            public static class NotAccepted
            {
                public const string Code = "WegbreedteNietCorrect";
                public static string Message(int value) => $"Wegbreedte is foutief. '{value}' is geen geldige waarde.";
            }
            public static class GreaterThanZero
            {
                public const string Code = "WegbreedteVerplicht";
                public const string Message = "Wegbreedte moet groter dan nul zijn.";
            }
        }
        public static class Lane
        {
            public static class GreaterThanZero
            {
                public const string Code = "AantalRijstrokenVerplicht";
                public const string Message = "Aantal rijstroken moet groter dan nul zijn.";
            }
        }
        
        public static class LaneDirection
        {
            public static class NotParsed
            {
                public const string Code = "AantalRijstrokenRichtingNietCorrect";
                public static string Message(string value) => $"Aantal rijstroken richting is foutief. '{value}' is geen geldige waarde.";
            }

            public static class IsRequired
            {
                public const string Code = "AantalRijstrokenRichtingVerplicht";
                public const string Message = "Aantal rijstroken richting is verplicht.";
            }
        }
    }
}
