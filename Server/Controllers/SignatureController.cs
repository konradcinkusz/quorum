namespace MR.Server.Controllers;

[Authorize]
public class SignatureController : MRBaseController
{
    public SignatureController(IMapper mapper) : base(mapper)
    {
    }

    [HttpPut("sign-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> SignIssue([FromRoute] Guid id)
        => await HandleErrors(async () => await Mediator.Send(new SignIssueCommand(id, GetUserId())), "Signing issue");

    [HttpPut("unsign-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UnsignIssue([FromRoute] Guid id)
        => await HandleErrors(async () => await Mediator.Send(new UnsignIssueCommand(id, GetUserId())), "Unsigning issue");
}
