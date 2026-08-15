namespace Quorum.Client.Services;

public interface ISignatureService
{
    Task<ApiResponse<bool>> SignIssue(Guid issueId);
    Task<ApiResponse<bool>> UnsignIssue(Guid issueId);
}

internal class SignatureService : DataServiceBase, ISignatureService
{
    public SignatureService(HttpClient httpclient) : base(httpclient)
    {
    }
    public async Task<ApiResponse<bool>> SignIssue(Guid issueId)
    {
        var endpoint = $"{_signatureControllerPath}/sign-issue/{issueId}";
        return await HandleResponse<bool>(async () => await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<ApiResponse<bool>> UnsignIssue(Guid issueId)
    {
        var endpoint = $"{_signatureControllerPath}/unsign-issue/{issueId}";
        return await HandleResponse<bool>(async () => await _httpClient.PutAsync(endpoint, null));
    }
}
