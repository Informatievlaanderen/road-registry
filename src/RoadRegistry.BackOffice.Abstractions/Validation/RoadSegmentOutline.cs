namespace RoadRegistry.BackOffice.Abstractions.Validation;

public static partial class ValidationErrors
{
    public static class RoadSegmentOutline
    {
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
