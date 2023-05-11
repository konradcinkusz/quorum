namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class QuarterController : MRBaseController
{
    public QuarterController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet(nameof(GetQuartersBySearchParams))]
    public async Task<ActionResult<ApiResponse<QuarterPagedListDto>>> 
        GetQuartersBySearchParams([FromQuery] SearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetQuartersBySearchParamsQuery
        {
            SearchParams = new SearchParams
            {
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize
            },
            SortColumn = query.SortColumn,
            SortOrder = (Microsoft.Data.SqlClient.SortOrder)query.SortOrder
        });

        var quarterPagedListDto = new QuarterPagedListDto
        {
            Items = _mapper.Map<List<QuarterDTO>>(result),
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };

        return new ApiResponse<QuarterPagedListDto>(quarterPagedListDto);
    }
}
