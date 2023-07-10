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
}