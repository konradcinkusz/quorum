namespace MR.Server.Controllers.Admin;

[Authorize(Policy = Policies.RequireAdminRole)]
public sealed class AdminIssueController : MRBaseController
{
    public AdminIssueController(IMapper mapper) : base(mapper)
    {
    }

    [HttpPost("create-issue-by-admin")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateOrEditIssueByAdmin([FromBody] IssueAdminCreateDTO dto)
    {
        var id = await Mediator.Send(new CreateIssueCommand
        {
            CreatedById = string.IsNullOrEmpty(dto.ApplicationUserId) ? GetUserId() : dto.ApplicationUserId,
            IssueVisibility = (IssueVisibility)dto.IssueVisibility,
            IssueProcess = (IssueProcess)dto.IssueProcess,
            IsVerifyByAdmin = dto.IsVerifyByAdmin,
            Question = dto.Question,
            Title = dto.Title,
            Icon = dto.Icon,
            BackgroundColor = dto.BackgroundColor,
        });

        return new ApiResponse<Guid> { Data = id };
    }

    [HttpPut("edit-issue-by-admin/{id}")]
    public async Task<ActionResult<ApiResponse<int>>> EditIssueByAdmin([FromRoute] Guid id, [FromBody] IssueAdminCreateDTO dto)
    {
        var idR = await Mediator.Send(new EditIssueCommand(id)
        {
            IssueVisibility = (IssueVisibility)dto.IssueVisibility,
            IssueProcess = (IssueProcess)dto.IssueProcess,
            IsVerifyByAdmin = dto.IsVerifyByAdmin,
            RatingValue = dto.RatingValue,
            Question = dto.Question,
            Title = dto.Title,
            Icon = dto.Icon,
            BackgroundColor = dto.BackgroundColor,
        });
        return new ApiResponse<int> { Data = idR };
    }

    [HttpPut("verify-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyIssue(Guid id, bool confirmed)
        => await HandleErrors(async () => await Mediator.Send(new VerifyByAdminCommand(id, confirmed)));

    [HttpGet("get-issues-by-search-params-admin")]
    public async Task<ActionResult<ApiResponse<PagedListDto<IssueReadDTO>>>> GetIssuesBySearchParamsAdmin([FromQuery] IssueSearchParamsDTO searchParams)
    {
        var command = new GetIssuesBySearchParamsQuery();
        SearchParamsExtension.AddIssueSearchParamsToCommand(command, searchParams);
        return await ProcessPagedRequest<GetIssuesBySearchParamsQuery, IssueReadDTO, Issue>(command);
    }

    [HttpPost("add-signature-to-signature-pool")]
    public async Task<ActionResult<ApiResponse<bool>>> AddSignatureToSignaturePool([FromBody] Guid signaturePoolDTO)
        => await HandleErrors(async () => await Mediator.Send(new AddSignatureToSignaturePoolCommand { SignaturePoolId = signaturePoolDTO }), "Adding new signature to the pool");

    [HttpDelete("force-delete-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> ForceDeleteIssue([FromRoute] Guid id)
        => await HandleErrors(async () => await Mediator.Send(new ForceDeleteIssueCommand(id)));
}