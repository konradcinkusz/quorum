namespace MR.Server.Controllers;

[Authorize]
public class SignaturePoolController : MRBaseController
{
    public SignaturePoolController(IMapper mapper) : base(mapper)
    {
    }

    private async Task<ActionResult<ApiResponse<SignaturePoolsPagedListDTO>>> ProcessSignaturePoolsRequest<T>(T command)
    where T : notnull
    {
        try
        {
            var result = await Mediator.Send(command) as PagedList<SignaturePool>;

            if (result is null)
            {
                // Handle the case when result is null, for example by returning an appropriate error response
                return new ApiResponse<SignaturePoolsPagedListDTO>("Failed to process the signature pools request.", (int)HttpStatusCode.BadRequest);
            }

            var IssuePoolPagedListDto = new SignaturePoolsPagedListDTO
            {
                Items = _mapper.Map<List<SignaturePoolDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<SignaturePoolsPagedListDTO>(IssuePoolPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<SignaturePoolsPagedListDTO>(new SignaturePoolsPagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [HttpGet("get-my-signature-pools")]
    public async Task<ActionResult<ApiResponse<SignaturePoolsPagedListDTO>>> GetMySignaturePools([FromQuery] SignaturePoolsSearchParamsDTO searchParams)
    {
        var command = new GetSignaturePoolsBySearchParamsQuery();
        AddSearchParamsToCommand(command, searchParams);

        command.ApplicationUserId = GetUserId();

        return await ProcessSignaturePoolsRequest(command);
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
    {
        return await HandleErrors(async () => await Mediator.Send(new AddSignatureToSignaturePoolCommand { SignaturePoolId = signaturePoolDTO }), "Adding new signature to the pool");
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("remove-signature")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveSignature([FromBody] Guid signatureId)
    {
        return await HandleErrors(async () => await Mediator.Send(new RemoveSignatureCommand { SignatureId = signatureId }), "Removing signature");
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("unpin-signature-from-issue")]
    public async Task<ActionResult<ApiResponse<bool>>> UnpinSignatureFromIssue([FromBody] Guid signatureId)
    {
        return await HandleErrors(async () => await Mediator.Send(new UnpinSignatureFromIssueCommand { SignatureId = signatureId }), "Unpinning issue from signature");
    }
}
