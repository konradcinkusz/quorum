namespace MR.Server;

[Route(Constants.RouteValues.AdvanceAPIRoute)]
[ApiController]
[ApiVersion(Constants.RouteValues.APIV1)]
public abstract class MRBaseController : ControllerBase
{
    protected readonly IMapper _mapper;

    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    public MRBaseController(
        IMapper mapper)
    {
        _mapper = mapper;
    }

    protected virtual string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

    protected void AddSearchParamsToCommand<T>(T command, SearchParamsDTO searchParamsDTO) where T : QueryBase
    {
        command.SearchParams = new SearchParams
        {
            CurrentPage = searchParamsDTO.CurrentPage,
            PageSize = searchParamsDTO.PageSize
        };
        command.SortColumn = searchParamsDTO.SortColumn;
        command.SortOrder = (Microsoft.Data.SqlClient.SortOrder)searchParamsDTO.SortOrder;
    }

}
