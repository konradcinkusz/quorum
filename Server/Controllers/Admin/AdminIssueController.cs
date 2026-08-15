namespace MR.Server.Controllers.Admin;

[Authorize(Policy = Constants.Policies.RequireAdminRole)]
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
            // Only when the admin is creating it for themselves. Creating on another user's
            // behalf leaves this null rather than stamping the admin's own address onto
            // someone else's initiative; the backfill in the migration covers existing rows,
            // and step 3 of ADR 0001's plan decides how the admin console displays it.
            CreatedByEmail = string.IsNullOrEmpty(dto.ApplicationUserId) ? GetUserEmail() : null,
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
        // Administrator scope: this route is gated by the class-level RequireAdminRole
        // policy, so it deliberately edits issues belonging to any user.
        var changedPropertiesCount = await Mediator.Send(new EditIssueCommand(id, IssueOwnerScope.Administrator())
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
        return new ApiResponse<int> { Data = changedPropertiesCount };
    }

    [HttpPut("verify-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyIssue(Guid id)
        => await HandleErrors(async () => await Mediator.Send(new VerifyByAdminCommand(id)));

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

    [HttpPut("calculate-rating-for-published-issues")]
    public async Task<ActionResult<ApiResponse<PagedListDto<IssueAdminRatingValueCalculate>>>> CalculatePublishedIssueRatingForCurrentQuarter()
        => await ProcessPagedRequest<CalculatePublishedIssueRatingForCurrentQuarter, IssueAdminRatingValueCalculate, Issue>(new CalculatePublishedIssueRatingForCurrentQuarter());

    [HttpPut("choose-the-winner-of-current-quarter")]
    public async Task<ActionResult<ApiResponse<IssueReadDTO>>> ChooseTheWinnerOfCurrentQuarter()
        => await HandleErrors<ChooseTheWinnerOfCurrentQuarter, Issue, IssueReadDTO>(new ChooseTheWinnerOfCurrentQuarter());

    [HttpPut("generate-pdf-for-an-issue/{id}")]
    public async Task<ActionResult<ApiResponse<string>>> GeneratePDFForAnIssue(Guid id)
        => await HandleErrors(async () => await Mediator.Send(new GeneratePDFForAnIssueCommand(id)));
}