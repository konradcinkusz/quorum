using MR.Server.Controllers.Base;

namespace MR.Server.Controllers;

[Authorize]
public sealed class IssueController : MRBaseController
{
    public IssueController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-issues-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<IssueReadDTO>>>> GetIssuesBySearchParams([FromQuery] IssueSearchParamsDTO searchParams)
    {
        var command = new GetIssuesBySearchParamsQuery();
        SearchParamsExtension.AddIssueSearchParamsToCommand(command, searchParams);
        return await ProcessPagedRequest<GetIssuesBySearchParamsQuery, IssueReadDTO, Issue>(command);
    }

    [HttpGet("get-issue-by-id-for-edit")]
    public async Task<ActionResult<ApiResponse<IssueReadDTO>>> GetIssueByIdForEdit(Guid id)
        => await HandleErrors<Issue, IssueReadDTO>(async () => await Mediator.Send(new GetIssueByIdForEdit(id)));

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
    {
        var countOfChangedProperties = await Mediator.Send(new EditIssueCommand(id)
        {
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        });

        return new ApiResponse<int> { Data = countOfChangedProperties, Message = "Edited" };
    }

    [HttpPost("create-issue")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateIssue([FromBody] IssueCreateDTO issueDTO)
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
            var createdStatus = await Mediator.Send(new IssueCreatedStatusChangeCommand(id));
            if (createdStatus)
                return ApiResponse<Guid>.CreatedApiResponse(id);
        }

        return ApiResponse<Guid>.BadRequestApiResponse(Guid.Empty);
    }

    [HttpDelete("archive-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> ArchiveIssue([FromRoute] Guid id)
        => await HandleErrors(async () => await Mediator.Send(new ArchiveIssueCommand(id)));
}
