namespace MR.Server.Controllers;

[Authorize]
public class SignaturePoolController : MRBaseController
{
    public SignaturePoolController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-my-signature-pools")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SignaturePoolDTO>>>> GetMySignaturePools([FromQuery] SignaturePoolsSearchParamsDTO searchParams)
    {
        var command = new GetSignaturePoolsBySearchParamsQuery();
        AddSearchParamsToCommand(command, searchParams);
        command.ApplicationUserId = GetUserId();
        return await ProcessPagedRequest<GetSignaturePoolsBySearchParamsQuery, SignaturePoolDTO, SignaturePool>(command);
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("get-signature-pools-by-search-params")]
    public async Task<ActionResult<ApiResponse<SignaturePoolsPagedListDTO>>>
        GetSignaturePoolsBySearchParams([FromQuery] SignaturePoolsSearchParamsDTO searchParams)
    {
        try
        {
            var result = await Mediator.Send(new GetSignaturePoolsBySearchParamsQuery
            {
                Year = searchParams.Year,
                Quarter = searchParams.Quarter,
                ApplicationUserId = searchParams.ApplicationUserId,
                ApplicationUserEmail = searchParams.ApplicationUserEmail,
                Begin = searchParams.Begin,
                End = searchParams.End,
                SearchParams = new SearchParams
                {
                    CurrentPage = searchParams.CurrentPage,
                    PageSize = searchParams.PageSize
                },
                SortColumn = searchParams.SortColumn,
                SortOrder = (Microsoft.Data.SqlClient.SortOrder)searchParams.SortOrder
            });

            var SignaturePoolPagedListDto = new SignaturePoolsPagedListDTO
            {
                Items = _mapper.Map<List<SignaturePoolDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<SignaturePoolsPagedListDTO>(SignaturePoolPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<SignaturePoolsPagedListDTO>(new SignaturePoolsPagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("add-signature-to-signature-pool")]
    public async Task<ActionResult<ApiResponse<bool>>> AddSignatureToSignaturePool([FromBody] Guid signaturePoolDTO)
        => await HandleErrors(async () => await Mediator.Send(new AddSignatureToSignaturePoolCommand { SignaturePoolId = signaturePoolDTO }), "Adding new signature to the pool");

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpDelete("remove-signature")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveSignature([FromQuery] Guid signatureId)
        => await HandleErrors(async () => await Mediator.Send(new RemoveSignatureCommand { SignatureId = signatureId }), "Removing signature");

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("unpin-signature-from-issue")]
    public async Task<ActionResult<ApiResponse<bool>>> UnpinSignatureFromIssue([FromBody] Guid signatureId)
        => await HandleErrors(async () => await Mediator.Send(new UnpinSignatureFromIssueCommand { SignatureId = signatureId }), "Unpinning issue from signature");
}
