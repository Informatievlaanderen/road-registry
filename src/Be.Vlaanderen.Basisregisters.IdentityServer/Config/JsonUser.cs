namespace IdentityServer.Config;

using System.Security.Claims;
using Duende.IdentityServer.Test;

public class JsonUser
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; }
    public List<JsonClaim> Claims { get; set; }
    public string SubjectId { get; set; }

    public static TestUser Export(JsonUser jsonUser)
    {
        return new TestUser
        {
            Username = jsonUser.Username,
            Password = jsonUser.Password,
            IsActive = jsonUser.IsActive,
            SubjectId = jsonUser.SubjectId,
            Claims = jsonUser.Claims.Select(c => new Claim(c.Type, c.Value)).ToList()
        };
    }
}