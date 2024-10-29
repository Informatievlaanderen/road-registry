namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;

public class Municipality : EventSourcedEntity
{
    public static readonly Func<Municipality> Factory = () => new Municipality();

    private Municipality()
    {
        On<ImportedMunicipality>(e =>
        {
            DutchName = e.DutchName;
            Geometry = e.Geometry;
        });
    }

    public string DutchName { get; private set; }
    public MunicipalityGeometry Geometry { get; set; }

    public bool IsRemoved { get; private set; }
}
