namespace RoadRegistry.Syndication.Schema
{
    using System;

    public class MunicipalityRecord
    {
        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }
        public string DutchName { get; set; }
        public string FrenchName { get; set; }
        public string GermanName { get; set; }
        public string EnglishName { get; set; }
        public MunicipalityStatus MunicipalityStatus { get; set; }
    }
}
