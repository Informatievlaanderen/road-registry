using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Configuration
{
    public class EmailClientOptions : IHasConfigurationKey
    {
        public string ExtractUploadFailed { get; set; }

        public string GetConfigurationKey() => "EmailClientOptions";
    }
}
