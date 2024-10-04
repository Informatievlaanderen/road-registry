using RoadRegistry.BackOffice;

namespace RoadRegistry.Sync.OrganizationRegistry;

public class OrganizationConsumerOptions: IHasConfigurationKey
{
    public string OrganizationRegistrySyncUrl { get; set; }
    public int ConsumerDelaySeconds { get; set; } = 30;
    public bool DisableWaitForEditorContextProjection { get; set; }

    public string GetConfigurationKey()
    {
        return "OrganizationConsumerOptions";
    }
}
