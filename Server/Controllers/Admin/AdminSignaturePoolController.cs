namespace MR.Server.Controllers.Admin;

[Authorize(Policy = Policies.RequireAdminRole)]
public class AdminSignaturePoolController : MRBaseController
{
    public AdminSignaturePoolController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-signature-pools-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SignaturePoolDTO>>>> GetSignaturePoolsBySearchParams([FromQuery] SignaturePoolsSearchParamsDTO searchParams)
    {
        var command = new GetSignaturePoolsBySearchParamsQuery();
        SearchParamsExtension.AddSignaturePoolsSearchParamsToCommand(command, searchParams);
        return await ProcessPagedRequest<GetSignaturePoolsBySearchParamsQuery, SignaturePoolDTO, SignaturePool>(command);
    }

    [HttpPost("add-signature-to-signature-pool")]
    public async Task<ActionResult<ApiResponse<bool>>> AddSignatureToSignaturePool([FromBody] Guid signaturePoolDTO)
        => await HandleErrors(async () => await Mediator.Send(new AddSignatureToSignaturePoolCommand { SignaturePoolId = signaturePoolDTO }), "Adding new signature to the pool");

    [HttpDelete("remove-signature")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveSignature([FromQuery] Guid signatureId)
        => await HandleErrors(async () => await Mediator.Send(new RemoveSignatureCommand { SignatureId = signatureId }), "Removing signature");

    [HttpPost("unpin-signature-from-issue")]
    public async Task<ActionResult<ApiResponse<bool>>> UnpinSignatureFromIssue([FromBody] Guid signatureId)
        => await HandleErrors(async () => await Mediator.Send(new UnpinSignatureFromIssueCommand { SignatureId = signatureId }), "Unpinning issue from signature");
}