namespace GiphyIntegration.Api.Users;

/// <summary>
/// Identifies the user behind the current request.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Stable key identifying the user, used to scope their cached searches.</summary>
    string Key { get; }
}
