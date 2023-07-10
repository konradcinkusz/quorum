namespace MR.Server.Controllers;

[Authorize]
public class SignaturePoolController : MRBaseController
{
    public SignaturePoolController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-my-signature-pools")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SignaturePoolAdminDTO>>>> GetMySignaturePools([FromQuery] SignaturePoolAdminSearchParamsDTO searchParams)
    {
        var command = new GetSignaturePoolsBySearchParamsQuery();
        AddSearchParamsToCommand(command, searchParams);
        command.ApplicationUserId = GetUserId();
        return await ProcessPagedRequest<GetSignaturePoolsBySearchParamsQuery, SignaturePoolAdminDTO, SignaturePool>(command);
    }
}