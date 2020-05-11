namespace RoadRegistry.Wms.Schema.RoadSegmentDenorm
{
    using System;

    public class RoadSegmentDenormRecord
    {
        public int Id { get; set; }
        public int Method { get; set; }
        public string Maintainer { get; set; }
        public DateTime BeginTime { get; set; }
        public string BeginOperator { get; set; }
        public string BeginOrganization { get; set; }
    }
}
