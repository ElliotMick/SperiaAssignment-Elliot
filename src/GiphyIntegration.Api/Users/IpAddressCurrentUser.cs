namespace GiphyIntegration.Api.Users;

/// <summary>
/// Identifies users by IP address, as the application has no account system.
/// </summary>
/// <remarks>
/// Users sharing a public IP (NAT, office network) share their cached searches.
/// Improvements:
/// <list type="bullet">
/// <item>Swap this class for a claims-based implementation once authentication exists; nothing else changes.</item>
/// <item>Behind a reverse proxy, every request comes from the proxy's IP. Enable <c>UseForwardedHeaders</c> with
/// <c>KnownProxies</c>/<c>KnownNetworks</c> set, never blindly: X-Forwarded-For is client controlled and could be
/// spoofed to read someone else's searches.</item>
/// <item>IP addresses are personal data (GDPR): store a keyed hash (HMAC) of them rather than the raw value.</item>
/// </list>
/// </remarks>
internal sealed class IpAddressCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string UnknownUser = "unknown";

    public string Key
    {
        get
        {
            var address = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            if (address is null)
            {
                return UnknownUser;
            }

            // A dual stack socket reports IPv4 clients as "::ffff:a.b.c.d"; normalize so they map to one user.
            return (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();
        }
    }
}
