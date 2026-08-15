namespace Quorum.Server.Controllers;

[Authorize]
public class SignaturePoolController : QuorumBaseController
{
    public SignaturePoolController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-my-signature-pools")]
    public async Task<ActionResult<ApiResponse<PagedListDto<UserSignaturePool>>>> GetMySignaturePools([FromQuery] SignaturePoolSearchParamsDTO searchParamsDTO)
    {
        var command = new GetUserSignaturePools();
        SearchParamsExtension.AddSignaturePoolSearchParamsToCommand(command, searchParamsDTO, GetUserId());
        return await ProcessPagedRequest<GetUserSignaturePools, UserSignaturePool, SignaturePool>(command);
    }
}