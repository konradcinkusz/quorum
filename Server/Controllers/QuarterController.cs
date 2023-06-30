namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class QuarterController : MRBaseController
{
    public QuarterController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-quarters-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<QuarterDTO>>>> GetQuartersBySearchParams([FromQuery] QuarterSearchParamsDTO searchParams)
    {
        var command = new GetQuartersBySearchParamsQuery();
        SearchParamsExtension.AddQuarterSearchParamsToCommand(command, searchParams);
        return await ProcessPagedRequest<GetQuartersBySearchParamsQuery, QuarterDTO, Quarter>(command);
    }

    [HttpPost("init-quarter")]
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
