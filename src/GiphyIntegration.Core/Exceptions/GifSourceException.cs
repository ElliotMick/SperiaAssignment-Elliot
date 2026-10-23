namespace GiphyIntegration.Core.Exceptions;

/// <summary>
/// Raised when the upstream GIF provider cannot be reached or returns an unusable response.
/// Provider agnostic, so callers don't depend on a specific provider's error types.
/// </summary>
public sealed class GifSourceException(string message, Exception? innerException = null)
    : Exception(message, innerException);
