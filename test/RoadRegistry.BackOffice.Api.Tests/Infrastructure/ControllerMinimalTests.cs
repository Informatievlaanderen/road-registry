namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public abstract class ControllerMinimalTests<TController> where TController : ControllerBase
{
    protected ControllerMinimalTests(TController controller)
    {
        Controller = controller;
        Controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    protected TController Controller { get; }
}
