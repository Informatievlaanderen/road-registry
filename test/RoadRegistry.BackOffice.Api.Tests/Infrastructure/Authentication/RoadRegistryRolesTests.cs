namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Authentication;

using System.Reflection;
using Api.Infrastructure.Authentication;

public class RoadRegistryRolesTests
{
    [Fact]
    public void AllRolesHaveScopesMapped()
    {
        var roleFields = typeof(RoadRegistryRoles).GetMembers(BindingFlags.Public | BindingFlags.Static)
            .OfType<FieldInfo>()
            .ToArray();
        Assert.True(roleFields.Any());

        foreach (var roleField in roleFields)
        {
            var role = (string)roleField.GetValue(null);
            var scopes = RoadRegistryRoles.GetScopes(role);
            Assert.True(scopes.Any());
        }
    }
}
