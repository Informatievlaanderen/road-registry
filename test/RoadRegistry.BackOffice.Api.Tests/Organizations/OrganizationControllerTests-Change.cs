namespace RoadRegistry.BackOffice.Api.Tests.Organizations;

using Api.Organizations;
using Core;
using FluentAssertions;
using Messages;
using Microsoft.AspNetCore.Mvc;

public partial class OrganizationControllerTests
{
    [Fact]
    public async Task WhenOnlyChangingIsMaintainer_ThenChangeOrganizationCommand()
    {
        var result = await Controller.Change(new OrganizationChangeParameters
        {
            IsMaintainer = true
        }, "AGIV", new ChangeOrganizationValidator(), CancellationToken.None);

        Assert.IsType<AcceptedResult>(result);

        var changeOrganizationCommand = await _store.GetLastMessage<ChangeOrganization>();

        changeOrganizationCommand.Should().NotBeNull();
        changeOrganizationCommand.Code.Should().Be("AGIV");
        changeOrganizationCommand.IsMaintainer.Should().BeTrue();
        changeOrganizationCommand.Name.Should().BeNull();
        changeOrganizationCommand.OvoCode.Should().BeNull();
        changeOrganizationCommand.KboNumber.Should().BeNull();
    }
}
