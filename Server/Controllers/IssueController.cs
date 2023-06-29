namespace MR.Server.Controllers;

[Authorize]
public sealed class IssueController : MRBaseController
{
    public IssueController(IMapper mapper) : base(mapper)
    {
    }

    [AllowAnonymous]
    [HttpGet("get-issues-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<IssueReadDTO>>>>
        GetIssuesBySearchParams([FromQuery] IssueSearchParamsDTO searchParams)
    {
        try
        {
            var command = new GetIssuesBySearchParamsQuery
            {
                IssueId = searchParams.IssueId,
                CreatedByEmail = searchParams.CreatedByEmail,
            };

            AddSearchParamsToCommand(command, searchParams);

            var result = await Mediator.Send(command);

            var IssuesPagedListDto = new PagedListDto<IssueReadDTO>
            {
                Items = _mapper.Map<List<IssueReadDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<PagedListDto<IssueReadDTO>>(IssuesPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedListDto<IssueReadDTO>>(new PagedListDto<IssueReadDTO>()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [HttpGet("get-my-issues-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<IssueReadDTO>>>> GetMyIssuesBySearchParams([FromQuery] IssueSearchParamsDTO searchParams)
    {
        var command = new GetIssuesBySearchParamsQuery();
        IssueSearchParams.AddIssueSearchParamsToCommand(command, searchParams);

        command.CreatedById = GetUserId();
        command.CreatedByEmail = string.Empty;

        return await ProcessPagedRequest<GetIssuesBySearchParamsQuery, IssueReadDTO, Issue>(command);
    }

    [HttpPut("publish-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>>
        PublishIssue([FromRoute] Guid id) => await HandleErrors(async () => await Mediator.Send(new PublishIssueCommand(GetUserId(), id)));

    [HttpPut("pay-for-an-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>>
        PayForAnIssue([FromRoute] Guid id, [FromBody] IssuePayDTO issuePayDTO)
            => await HandleErrors(async () => await Mediator.Send(new PayForAnIssueCommand(GetUserId(), id)
            {
                PaymentMethod = issuePayDTO.PaymentMethod,
                ReferenceNumber = issuePayDTO.ReferenceNumber,
                PaymentValue = issuePayDTO.PaymentValue,
            }));

    [HttpPut("change-issue-process-status/{id}")]
    public async Task<ActionResult<ApiResponse<Guid>>>
        ChangeIssueProcessStatus([FromRoute] Guid id, [FromBody] IssueProcessEnum issueProcessEnum)
    {
        var paymentId = await Mediator.Send(new CreateIssueCommand
        {
            IssueId = id,
            CreatedById = GetUserId(),
            IssueProcess = (IssueProcess)issueProcessEnum
        });

        return new ApiResponse<Guid> { Data = paymentId, Message = $"Edited, new status: {issueProcessEnum}" };
    }

    [HttpPut("edit-issue/{id}")]
    public async Task<ActionResult<ApiResponse<Guid>>>
        EditIssue([FromRoute] Guid id, [FromBody] IssueCreateDTO issueDTO)
    {
        var paymentId = await Mediator.Send(new CreateIssueCommand
        {
            IssueId = id,
            CreatedById = GetUserId(),
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        });

        return new ApiResponse<Guid> { Data = paymentId, Message = "Edited" };
    }

    [HttpPost("create-issue")]
    public async Task<ActionResult<ApiResponse<Guid>>>
        CreateIssue([FromBody] IssueCreateDTO issueDTO)
    {
        var userId = GetUserId();
        var id = await Mediator.Send(new CreateIssueCommand
        {
            CreatedById = userId,
            IssueVisibility = IssueVisibility.VisibleOnlyToMe,
            IssueProcess = IssueProcess.InCreation,
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        });
        if (id != Guid.Empty)
        {
            var createdStatus = await Mediator.Send(new IssueCreatedStatusChangeCommand(id, userId));
            if (createdStatus)
                return ApiResponse<Guid>.CreatedApiResponse(id);
        }
        return ApiResponse<Guid>.BadRequestApiResponse(Guid.Empty);
    }
}
