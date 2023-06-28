namespace MR.Server.Controllers;

[Authorize]
public sealed class IssueController : MRBaseController
{
    public IssueController(IMapper mapper) : base(mapper)
    {
    }

    private async Task<ActionResult<ApiResponse<IssuePagedListDTO>>> ProcessIssueRequest<T>(T command)
    where T : notnull
    {
        try
        {
            var result = await Mediator.Send(command) as PagedList<Issue>;

            if (result is null)
            {
                // Handle the case when result is null, for example by returning an appropriate error response
                return new ApiResponse<IssuePagedListDTO>("Failed to process the issue request.", (int)HttpStatusCode.BadRequest);
            }

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

    private void AddIssueSearchParamsToCommand(GetIssuesBySearchParamsQuery command, IssueSearchParamsDTO searchParams)
    {
        command.IssueId = searchParams.IssueId;
        command.CreatedByEmail = searchParams.CreatedByEmail;
        command.Title = searchParams.Title;
        command.Question = searchParams.Question;
        command.IsVerifyByAdmin = searchParams.IsVerifyByAdmin;
        command.IssueVisibility = (IssueVisibility?)searchParams.IssueVisibility;
        command.RatingValue = searchParams.RatingValue;
        command.HasInitialPayment = searchParams.PaymentOptions != null ? searchParams.PaymentOptions == IssuePaymentOptions.WithInitialPayment : null;
        command.QuarterNumber = searchParams.QuarterNumber;
        command.QuarterYear = searchParams.QuarterYear;

        AddSearchParamsToCommand(command, searchParams);
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

            var IssuesPagedListDto = new IssuePagedListDTO
            {
                Items = _mapper.Map<List<IssueReadDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<IssuePagedListDTO>(IssuesPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IssuePagedListDTO>(new IssuePagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [HttpGet("get-my-issues-by-search-params")]
    public async Task<ActionResult<ApiResponse<IssuePagedListDTO>>>
        GetMyIssuesBySearchParams([FromQuery] IssueSearchParamsDTO searchParams)
    {
        var command = new GetIssuesBySearchParamsQuery();
        AddIssueSearchParamsToCommand(command, searchParams);

        command.CreatedById = GetUserId();
        command.CreatedByEmail = string.Empty;

        return await ProcessIssueRequest(command);
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

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("get-issues-by-search-params-admin")]
    public async Task<ActionResult<ApiResponse<IssuePagedListDTO>>>
        GetIssuesBySearchParamsAdmin([FromQuery] IssueSearchParamsDTO searchParams)
    {
        var command = new GetIssuesBySearchParamsQuery();
        AddIssueSearchParamsToCommand(command, searchParams);
        return await ProcessIssueRequest(command);
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("create-issue-by-admin")]
    public async Task<ActionResult<ApiResponse<Guid>>>
        CreateOrEditIssueByAdmin([FromBody] IssueAdminCreateDTO dto)
    {
        var id = await Mediator.Send(new CreateIssueCommand
        {
            IssueId = dto.IssueId,
            CreatedById = string.IsNullOrEmpty(dto.ApplicationUserId) ? GetUserId() : dto.ApplicationUserId,
            IssueVisibility = (IssueVisibility)dto.IssueVisibility,
            IssueProcess = (IssueProcess)dto.IssueProcess,
            IsVerifyByAdmin = dto.IsVerifyByAdmin,
            RatingValue = dto.RatingValue,
            Question = dto.Question,
            Title = dto.Title,
            Icon = dto.Icon,
            BackgroundColor = dto.BackgroundColor,
        });

        return new ApiResponse<Guid> { Data = id };
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPut("edit-issue-by-admin/{id}")]
    public async Task<ActionResult<ApiResponse<Guid>>> EditIssueByAdmin([FromRoute] Guid id, [FromBody] IssueAdminCreateDTO dto)
    {
        if (dto.IssueId.HasValue)
        {
            var idR = await Mediator.Send(new EditIssueCommand(dto.IssueId.Value)
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
            return new ApiResponse<Guid> { Data = idR };
        }
        return new ApiResponse<Guid>() { Success = false };
    }
}
