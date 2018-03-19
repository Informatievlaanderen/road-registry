namespace RoadRegistry.Api.Oslo.Road.Responses
{
    using System.Collections.Generic;
    using Swashbuckle.AspNetCore.Examples;

    public class RoadListResponse
    {
        /// <summary>Naam van de weg.</summary>
        public string Naam { get; }

        public RoadListResponse(
            string defaultName,
            string nameDutch,
            string nameFrench,
            string nameGerman,
            string nameEnglish,
            Taal? taal)
        {
            if (!taal.HasValue)
            {
                Naam = defaultName;
            }
            else
            {
                switch (taal)
                {
                    case Taal.Nl:
                        Naam = nameDutch;
                        break;
                    case Taal.Fr:
                        Naam = nameFrench;
                        break;
                    case Taal.De:
                        Naam = nameGerman;
                        break;
                    case Taal.En:
                        Naam = nameEnglish;
                        break;
                }
            }
        }
    }

    public class RoadListResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new List<RoadListResponse>
            {
                new RoadListResponse(
                    "Brugge",
                    "Brugge",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Taal.Nl),

                new RoadListResponse(
                    "Quévy",
                    string.Empty,
                    "Quévy",
                    string.Empty,
                    string.Empty,
                    Taal.Fr)
            };
        }
    }
}
