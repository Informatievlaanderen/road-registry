namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using Api.Uploads;
using Infrastructure;

public partial class UploadControllerTests : ControllerMinimalTests<UploadController>
{
    public UploadControllerTests(
        UploadController controller)
        : base(controller)
    {
    }
}
