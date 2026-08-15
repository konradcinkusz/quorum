namespace MR.Server.Controllers;

[Authorize]
public sealed class IssueController : MRBaseController
{
    public IssueController(IMapper mapper) : base(mapper)
    {
    }

    // "get-issues-by-search-params" used to live here: [Authorize] only, no owner filter and
    // no visibility filter, eager-loading CreatedBy — so any registered user could page
    // through every other user's unpublished drafts along with the author's contact details.
    // It had no caller; the client's user-facing pages use get-my-issues-by-search-params
    // and the admin console uses AdminIssueController's get-issues-by-search-params-admin.
    // Removed rather than gated, because an unscoped listing with an admin-scoped twin
    // already existing is a duplicate, not a missing policy.

    [AllowAnonymous]
    [HttpGet("get-current-quarter-issues-published")]
    public async Task<ActionResult<ApiResponse<PagedListDto<PublicPublishedIssueRead>>>> GetCurrentQuarterPublishedIssues([FromQuery] PublicPublishedIssueSearchParamsDTO searchParams)
    {
        var command = new GetCurrentQuarterPublishedIssues();
        searchParams.SortColumn = "RatingValue";
        searchParams.SortOrder = SortOrder.Descending;
        searchParams.PageSize = 100;
        searchParams.CurrentPage = 1;
        SearchParamsExtension.AddPublicPublishedIssueSearchParamsToCommand(command, searchParams);
        var request = await ProcessPagedRequest<GetCurrentQuarterPublishedIssues, PublicPublishedIssueRead, Issue>(command);

        string currentUserId = GetUserId();
        if (!string.IsNullOrWhiteSpace(currentUserId) && request.Value != null)
        {
            var signedIssue = await Mediator.Send(new GetSignedIssueByUser(currentUserId));
            request.Value.Data.Items.ForEach(x => x.SignedByCurrentUser = signedIssue.Any(y => x.Id == y));
        }

        return request;
    }

    [HttpGet("get-your-winners")]
    public async Task<ActionResult<ApiResponse<PagedListDto<IssueSignedAndSubmittedDTO>>>> GetYourWinners([FromQuery] IssueSignAndSubmitSearchParamsDTO searchParams)
    {
        var command = new GetYourWinnersCommand();
        command.ApplicationUserId = GetUserId();
        searchParams.SortColumn = "RatingValue";
        searchParams.SortOrder = SortOrder.Descending;
        searchParams.PageSize = 100;
        searchParams.CurrentPage = 1;
        SearchParamsExtension.AddIssueSignAndSubmitSearchParamsToCommand(command, searchParams);
        var request = await ProcessPagedRequest<GetYourWinnersCommand, IssueSignedAndSubmittedDTO, Issue>(command);
        return request;
    }

    [AllowAnonymous]
    [HttpGet("get-the-winning-issues-for-the-quarter")]
    public async Task<ActionResult<ApiResponse<PagedListDto<PublicPublishedEndedIssueRead>>>> GetTheWinningIssuesForTheQuarter([FromQuery] IssueWinnersSearchParamsDTO searchParams)
    {
        var command = new GetTheWinningIssuesForTheQuarterQuery();
        SearchParamsExtension.AddPublicPublishedEndeedIssueSearchParamsToCommand(command, searchParams);
        return await ProcessPagedRequest<GetTheWinningIssuesForTheQuarterQuery, PublicPublishedEndedIssueRead, Issue>(command);
    }

    [HttpGet("get-issue-by-id-for-edit")]
    public async Task<ActionResult<ApiResponse<IssueReadDTO>>> GetIssueByIdForEdit(Guid id)
        => await HandleErrors<Issue, IssueReadDTO>(async () => await Mediator.Send(new GetIssueByIdForEdit(id, IssueOwnerScope.OwnedBy(GetUserId()))));

    [HttpGet("get-my-issues-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<IssueReadDTO>>>> GetMyIssuesBySearchParams([FromQuery] IssueSearchParamsDTO searchParams)
    {
        var command = new GetIssuesBySearchParamsQuery();
        SearchParamsExtension.AddIssueSearchParamsToCommand(command, searchParams);

        command.CreatedById = GetUserId();
        command.CreatedByEmail = string.Empty;

        return await ProcessPagedRequest<GetIssuesBySearchParamsQuery, IssueReadDTO, Issue>(command);
    }

    [HttpPut("publish-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> PublishIssue([FromRoute] Guid id)
        => await HandleErrors(async () => await Mediator.Send(new PublishIssueCommand(GetUserId(), id)));

    [HttpPut("pay-for-an-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> PayForAnIssue([FromRoute] Guid id, [FromBody] IssuePayDTO issuePayDTO)
        => await HandleErrors(async () => await Mediator.Send(new PayForAnIssueCommand(GetUserId(), id)
        {
            PaymentMethod = issuePayDTO.PaymentMethod,
            ReferenceNumber = issuePayDTO.ReferenceNumber,
            PaymentValue = issuePayDTO.PaymentValue,
        }));

    [HttpPut("edit-issue/{id}")]
    public async Task<ActionResult<ApiResponse<int>>> EditIssue([FromRoute] Guid id, [FromBody] IssueCreateDTO issueDTO)
        // Wrapped in HandleErrors because the command now throws NotFoundException when the
        // issue is not the caller's; unwrapped, that would surface as an unhandled 500.
        => await HandleErrors(async () => await Mediator.Send(new EditIssueCommand(id, IssueOwnerScope.OwnedBy(GetUserId()))
        {
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        }), "Edited");

    [HttpPost("create-issue")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateIssue([FromBody] IssueCreateDTO issueDTO)
    {
        var userId = GetUserId();

        var id = await Mediator.Send(new CreateIssueCommand
        {
            CreatedById = userId,
            CreatedByEmail = GetUserEmail(),
            IssueVisibility = IssueVisibility.VisibleOnlyToMe,
            IssueProcess = IssueProcess.InCreation,
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        });

        if (id != Guid.Empty)
        {
            var createdStatus = await Mediator.Send(new IssueCreatedStatusChangeCommand(id));
            if (createdStatus)
                return ApiResponse<Guid>.CreatedApiResponse(id);
        }

        return ApiResponse<Guid>.BadRequestApiResponse(Guid.Empty);
    }

    [HttpDelete("archive-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> ArchiveIssue([FromRoute] Guid id)
        => await HandleErrors(async () => await Mediator.Send(new ArchiveIssueCommand(id, IssueOwnerScope.OwnedBy(GetUserId()))));

    [HttpGet("get-signed-issues")]
    public async Task<ActionResult<ApiResponse<PagedListDto<PublicPublishedIssueRead>>>> GetSignedIssues([FromQuery] PublicPublishedIssueSearchParamsDTO searchParams)
    {
        var request = await ProcessPagedRequest<GetSignedIssuesCommand, PublicPublishedIssueRead, Issue>(new GetSignedIssuesCommand { ApplicationUserId = GetUserId() });
        return request;
    }

    // The size cap is enforced twice on purpose: here, so an oversized body is rejected by
    // the framework before it is buffered, and again in the handler, so the rule still holds
    // if this endpoint is ever called from somewhere that does not set the attribute.
    [RequestSizeLimit(SignedDocumentRules.MaxSizeBytes)]
    [HttpPut("upload-signed-document/{id}")]
    public async Task<ActionResult<ApiResponse<string>>> UploadSignedDocument([FromRoute] Guid id, [FromForm] IFormFile file)
        => await HandleErrors(async () => await Mediator.Send(new UploadSignedDocumentCommand(id, file, GetUserId())));
}
