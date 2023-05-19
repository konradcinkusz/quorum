namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class AdminController : MRBaseController
{
    private readonly IConfiguration _configuration;
    public AdminController(IMapper mapper, IConfiguration configuration) : base(mapper)
    {
        _configuration = configuration;
    }

    [HttpGet("GetAdminLogsByQuery")]
    public async Task<ActionResult<AdminLogPagedListDTO>> GetAdminLogsByQuery([FromQuery] AdminLogSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetAdminLogsBySearchParamsQuery
        {
            SearchParams = new()
            {
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize
            },
            LastHour = query.LastHour,
            LastMonth = query.LastMonth,
            ValuesText = query.ValuesText,
        });

        var adminLogsDTO = _mapper.Map<List<AdminLogDTO>>(result);

        return Ok(new AdminLogPagedListDTO { Items = adminLogsDTO, CurrentPage = result.CurrentPage, PageSize = result.PageSize, TotalItems = result.TotalItems, TotalPages = result.TotalPages });
    }

    [HttpPost("SeedPayments")]
    public async Task<IActionResult> SeedPayments([FromBody] SeedPaymentRequest seedPaymentRequest)
    {
        if (!_configuration.GetValue<bool>("SeedData:IsSeeded"))
        {
            var command = new SeedPaymentCommand(GetUserId()) { Count = seedPaymentRequest.Count };
            await Mediator.Send(command);
            _configuration["SeedData:IsSeeded"] = "true";

            return Ok();
        }
        return BadRequest("Data has already been seeded");
    }


    [HttpPost("ActivateSubscription")]
    public async Task<IActionResult> ActivateSubscription()
    {
        var result = await Mediator.Send(new ActivateSubscriptionCommand());
        return Ok(result);
    }
}
