namespace MR.Server.Controllers.Admin;

[Authorize(Policy = Constants.Policies.RequireAdminRole)]
public class AdminSignaturePoolController : MRBaseController
{
    public AdminSignaturePoolController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-signature-pools-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SignaturePoolAdminDTO>>>> GetSignaturePoolsBySearchParams([FromQuery] SignaturePoolAdminSearchParamsDTO searchParams)
    {
        var command = new GetSignaturePoolsBySearchParamsQuery();
        SearchParamsExtension.AddSignaturePoolAdminSearchParamsToCommand(command, searchParams);
        return await ProcessPagedRequest<GetSignaturePoolsBySearchParamsQuery, SignaturePoolAdminDTO, SignaturePool>(command);
    }

    [HttpPost("add-signature-to-signature-pool")]
    public async Task<ActionResult<ApiResponse<bool>>> AddSignatureToSignaturePool([FromBody] Guid signaturePoolDTO)
        => await HandleErrors(async () => await Mediator.Send(new AddSignatureToSignaturePoolCommand { SignaturePoolId = signaturePoolDTO }), "Adding new signature to the pool");

    [HttpDelete("remove-signature")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveSignature([FromQuery] Guid signatureId)
        => await HandleErrors(async () => await Mediator.Send(new RemoveSignatureCommand { SignatureId = signatureId }), "Removing signature");

    [HttpPost("unpin-signature-from-issue")]
    public async Task<ActionResult<ApiResponse<bool>>> UnpinSignatureFromIssue([FromBody] Guid signatureId)
        => await HandleErrors(async () => await Mediator.Send(new UnpinSignatureFromIssueCommand(signatureId)), "Unpinning issue from signature");
}