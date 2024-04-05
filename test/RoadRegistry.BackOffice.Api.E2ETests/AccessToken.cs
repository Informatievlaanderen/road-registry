namespace RoadRegistry.BackOffice.Api.E2ETests;

using System;

public class AccessToken
{
    private readonly DateTime _expiresAt;

    public string Token { get; }

    // Let's regard it as expired 10 seconds before it actually expires.
    public bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

    public AccessToken(string token, int expiresIn)
    {
        _expiresAt = DateTime.Now.AddSeconds(expiresIn);
        Token = token;
    }
}
