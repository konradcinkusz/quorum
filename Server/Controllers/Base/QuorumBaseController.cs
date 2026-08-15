namespace Quorum.Server.Controllers.Base;

[Route(Constants.RouteValues.AdvanceAPIRoute)]
[ApiController]
[ApiVersion(Constants.RouteValues.APIV1)]
public abstract class QuorumBaseController : ControllerBase
{
    protected readonly IMapper _mapper;

    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    public QuorumBaseController(IMapper mapper)
    {
        _mapper = mapper;
    }

    protected virtual string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// The caller's email from their token. Read from the claim rather than looked up: once
    /// identity moves to <c>authservice</c> (ADR 0001) there is no local user table to look
    /// it up in, and IDENTITY-AND-ACCOUNTS §1 is explicit that a service holding a token does
    /// not call back to ask about the user.
    /// <para>
    /// Both claim types are checked because the two identity stacks spell it differently:
    /// ASP.NET Core Identity uses <see cref="ClaimTypes.Email"/>, while a JWT carries the
    /// short <c>email</c> claim. Accepting both means this keeps working across the cutover
    /// rather than needing to change on the same commit.
    /// </para>
    /// </summary>
    protected virtual string? GetUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;

    protected async Task<ActionResult<ApiResponse<T>>> HandleErrors<T>(Func<Task<T>> action, string message = "")
    {
        try
        {
            T result = await action.Invoke();
            return new ApiResponse<T>(result) { Message = message };
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse<T>(new List<string> { ex.Message }, (int)HttpStatusCode.BadRequest);
            return errorResponse;
        }
    }

    //D = DTO
    protected async Task<ActionResult<ApiResponse<D>>> HandleErrors<T, D>(Func<Task<T>> action, string message = "")
    {
        try
        {
            T result = await action.Invoke();
            var item = _mapper.Map<D>(result);
            return new ApiResponse<D>(item) { Message = message };
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse<D>(new List<string> { ex.Message }, (int)HttpStatusCode.BadRequest);
            return errorResponse;
        }
    }

    //D = DTO; E = Entity
    protected async Task<ActionResult<ApiResponse<DTO>>> HandleErrors<Command, Entity, DTO>(Command command, string message = "") where Command : notnull where Entity : class
    {
        try
        {
            var result = await Mediator.Send(command) as Entity;

            if (result is null)
            {
                // Handle the case when result is null, for example by returning an appropriate error response
                return new ApiResponse<DTO>("Failed to process the issue request.", (int)HttpStatusCode.BadRequest);
            }

            var item = _mapper.Map<DTO>(result);
            return new ApiResponse<DTO>(item) { Message = message };
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse<DTO>(new List<string> { ex.Message }, (int)HttpStatusCode.BadRequest);
            return errorResponse;
        }
    }

    protected virtual async Task<ActionResult<ApiResponse<PagedListDto<D>>>> ProcessPagedRequest<T, D, E>(T command) where T : notnull where D : class
    {
        try
        {
            var result = await Mediator.Send(command) as PagedList<E>;

            if (result is null)
            {
                // Handle the case when result is null, for example by returning an appropriate error response
                return new ApiResponse<PagedListDto<D>>("Failed to process the issue request.", (int)HttpStatusCode.BadRequest);
            }

            var pagedListDTO = new PagedListDto<D>
            {
                Items = _mapper.Map<List<D>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<PagedListDto<D>>(pagedListDTO);
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedListDto<D>>(new PagedListDto<D>()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }
}
