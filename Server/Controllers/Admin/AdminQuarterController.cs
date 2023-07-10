namespace MR.Server.Controllers.Admin;

[Authorize(Policy = Policies.RequireAdminRole)]
public class AdminQuarterController : MRBaseController
{
    public AdminQuarterController(IMapper mapper) : base(mapper)
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
        => await HandleErrors(async () => await Mediator.Send(new InitQuarterCommand(initQuarterDTO.Year, initQuarterDTO.Month) { SignaturesCount = initQuarterDTO.SignaturesCount }));

    [HttpDelete("delete-quarter/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> ForceDeleteIssue([FromRoute] Guid id)
        => await HandleErrors(async () => await Mediator.Send(new DeleteQuarterCommand(id)));
}
