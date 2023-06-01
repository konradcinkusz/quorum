namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class QuarterController : MRBaseController
{
    public QuarterController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet(nameof(GetQuartersBySearchParams))]
    public async Task<ActionResult<ApiResponse<QuarterPagedListDTO>>>
        GetQuartersBySearchParams([FromQuery] QuarterSearchParamsDTO searchParams)
    {
        try
        {
            var result = await Mediator.Send(new GetQuartersBySearchParamsQuery
            {
                QuarterNumber = searchParams.Quarter,
                Year =  searchParams.Year,
                Begin = searchParams.Begin,
                End = searchParams.End,
                SearchParams = new SearchParams
                {
                    CurrentPage = searchParams.CurrentPage,
                    PageSize = searchParams.PageSize
                },
                SortColumn = searchParams.SortColumn,
                SortOrder = (Microsoft.Data.SqlClient.SortOrder)searchParams.SortOrder
            });

            var quarterPagedListDto = new QuarterPagedListDTO
            {
                Items = _mapper.Map<List<QuarterDTO>>(result),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return new ApiResponse<QuarterPagedListDTO>(quarterPagedListDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse<QuarterPagedListDTO>(new QuarterPagedListDTO()) { Message = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
        }
    }

    [HttpPost(nameof(InitQuarter))]
    public async Task<ActionResult<ApiResponse<Guid>>> InitQuarter([FromBody] InitQuarterDTO initQuarterDTO)
    {
        try
        {
            var result = await Mediator.Send(new InitQuarterCommand
            {
                Month = initQuarterDTO.Month,
                Year = initQuarterDTO.Year,
                SignaturesCount = initQuarterDTO.SignaturesCount
            });
            return new ApiResponse<Guid>(result);
        }
        catch (ApplicationException ex)
        {
            return new ApiResponse<Guid>() { Message = ex.Message };
        }
    }
}
