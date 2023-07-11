namespace MR.Client.Services;

public interface ISignaturePoolService
{
    Task<ApiResponse<PagedListDto<UserSignaturePool>>> GetMySignaturePoolsBySearchParams(SignaturePoolSearchParamsDTO searchParams);
}

internal class SignaturePoolService : DataServiceBase, ISignaturePoolService
{
    public SignaturePoolService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<PagedListDto<UserSignaturePool>>> GetMySignaturePoolsBySearchParams(SignaturePoolSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_signaturePoolControllerPath}/get-my-signature-pools?{q}";
        return await HandleResponse<PagedListDto<UserSignaturePool>>(async () => await _httpClient.GetAsync(endpoint));
    }
}
