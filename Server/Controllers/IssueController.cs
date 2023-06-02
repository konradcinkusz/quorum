namespace MR.Server.Controllers;

[Authorize]
public class IssueController : MRBaseController
{
    public IssueController(IMapper mapper) : base(mapper)
    {
    }

    [AllowAnonymous]
    [HttpGet("get-issues-by-search-params")]
    public async Task<ActionResult<ApiResponse<IssuePagedListDTO>>>
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

            var IssuePoolPagedListDto = new IssuePagedListDTO
            {
                Items = _mapper.Map<List<IssueReadDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<IssuePagedListDTO>(IssuePoolPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IssuePagedListDTO>(new IssuePagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [HttpPut("publish-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>>
        PublishIssue([FromQuery] Guid id) => await HandleErrors(async () => await Mediator.Send(new PublishIssueCommand(GetUserId(), id)));

    [HttpPut("pay-for-an-issue/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>>
        PayForAnIssue([FromQuery] Guid id, [FromBody] IssuePayDTO issuePayDTO)
            => await HandleErrors(async () => await Mediator.Send(new PayForAnIssueCommand(GetUserId(), id)
            {
                PaymentMethod = issuePayDTO.PaymentMethod,
                ReferenceNumber = issuePayDTO.ReferenceNumber,
                PaymentValue = issuePayDTO.PaymentValue,
            }));

    [HttpPost("create-issue")]
    public async Task<ActionResult<ApiResponse<Guid>>>
        CreateOrEditIssue([FromBody] IssueCreateDTO issueDTO)
    {
        var paymentId = await Mediator.Send(new CreateOrEditIssueCommand
        {
            IssueId = issueDTO.IssueId,
            CreatedById = GetUserId(),
            IssueStatus = IssueStatus.VisibleOnlyToMe,
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        });

        return new ApiResponse<Guid> { Data = paymentId, Message = issueDTO.IssueId != null ? "Edited" : "Created" };
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("get-issues-by-search-params-admin")]
    public async Task<ActionResult<ApiResponse<IssuePagedListDTO>>>
        GetIssuesBySearchParamsAdmin([FromQuery] IssueSearchParamsDTO searchParams)
    {
        try
        {
            var command = new GetIssuesBySearchParamsQuery
            {
                IssueId = searchParams.IssueId,
                CreatedByEmail = searchParams.CreatedByEmail,
                Title = searchParams.Title,
                Question = searchParams.Question,
                IsVerifyByAdmin = searchParams.IsVerifyByAdmin,
                IssueStatus = (IssueStatus?)searchParams.IssueStatus,
                RatingValue = searchParams.RatingValue,
                HasInitialPayment = searchParams.PaymentOptions == IssuePaymentOptions.WithInitialPayment,
                QuarterNumber = searchParams.QuarterNumber,
                QuarterYear = searchParams.QuarterYear,
            };

            AddSearchParamsToCommand(command, searchParams);

            var result = await Mediator.Send(command);

            var IssuePoolPagedListDto = new IssuePagedListDTO
            {
                Items = _mapper.Map<List<IssueReadDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<IssuePagedListDTO>(IssuePoolPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IssuePagedListDTO>(new IssuePagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("create-issue-by-admin")]
    public async Task<ActionResult<ApiResponse<Guid>>>
        CreateOrEditIssueByAdmin([FromBody] IssueAdminCreateDTO issueDTO)
    {
        var paymentId = await Mediator.Send(new CreateOrEditIssueCommand
        {
            IssueId = issueDTO.IssueId,
            CreatedById = string.IsNullOrEmpty(issueDTO.ApplicationUserId) ? GetUserId() : issueDTO.ApplicationUserId,
            IssueStatus = (IssueStatus)issueDTO.IssueStatus,
            IsVerifyByAdmin = issueDTO.IsVerifyByAdmin,
            RatingValue = issueDTO.RatingValue,
            Question = issueDTO.Question,
            Title = issueDTO.Title,
            Icon = issueDTO.Icon,
            BackgroundColor = issueDTO.BackgroundColor,
        });

        return new ApiResponse<Guid> { Data = paymentId };
    }
}
