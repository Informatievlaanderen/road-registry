namespace Microsoft.Extensions.DependencyInjection;

using Duende.IdentityServer.Test;

public static class IdentityServerBuilderExtensions
{
    /// <summary>
    /// Adds the in memory clients.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="clients">The clients.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddInMemoryUsers(this IIdentityServerBuilder builder, IEnumerable<TestUser> testUsers)
    {
        if (testUsers is not null)
        {
            builder.AddTestUsers(testUsers.ToList());
        }

        return builder;
    }
}
