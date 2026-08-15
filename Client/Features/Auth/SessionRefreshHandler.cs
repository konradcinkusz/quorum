namespace Quorum.Client.Features.Auth;

/// <summary>
/// Turns an access-token expiry into a silent refresh instead of a visible sign-out.
/// <para>
/// The access cookie outlives nothing: when the API answers 401, this handler asks the BFF
/// to redeem the refresh cookie and replays the original request once. Replays are safe to
/// attempt for any method because the first attempt never reached a handler — 401 is
/// decided by authentication middleware before the endpoint runs. If the refresh itself
/// fails the session is over, and the provider flips the UI to anonymous.
/// </para>
/// </summary>
public sealed class SessionRefreshHandler : DelegatingHandler
{
    private readonly IBffAuthClient _auth;
    private readonly BffAuthenticationStateProvider _state;

    public SessionRefreshHandler(IBffAuthClient auth, BffAuthenticationStateProvider state)
    {
        _auth = auth;
        _state = state;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Buffer the content up front so the request can be cloned if it needs a replay.
        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync();
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        if (!await _auth.TryRefreshAsync())
        {
            await _state.ReloadSessionAsync();
            return response;
        }

        using var retry = await CloneAsync(request);
        response.Dispose();
        return await base.SendAsync(retry, cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
        };

        foreach (var (key, value) in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(key, value);
        }

        if (request.Content is not null)
        {
            var buffer = await request.Content.ReadAsByteArrayAsync();
            var content = new ByteArrayContent(buffer);
            foreach (var (key, value) in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(key, value);
            }

            clone.Content = content;
        }

        return clone;
    }
}
