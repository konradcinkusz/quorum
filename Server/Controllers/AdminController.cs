namespace MR.Server.Controllers;

public class AdminController : MRBaseController
{
    private readonly IConfiguration _configuration;
    public AdminController(IMapper mapper, IConfiguration configuration) : base(mapper)
    {
        _configuration = configuration;
    }
    
    [HttpGet(nameof(GetAdminLogsByQuery))]
    public async Task<ActionResult<AdminLogPagedListDTO>> GetAdminLogsByQuery([FromQuery] AdminLogSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetAdminLogsBySearchParamsQuery
        {
            CurrentPage = query.CurrentPage,
            PageSize = query.PageSize,
            Name = query.Name,
            Question = query.Question,
            LastHour = query.LastHour,
            LastMonth = query.LastMonth,
            ValuesText = query.ValuesText,
        });

        var adminLogsDTO = _mapper.Map<List<AdminLogDTO>>(result);

        return Ok(new AdminLogPagedListDTO { Items = adminLogsDTO, CurrentPage = result.CurrentPage, PageSize = result.PageSize, TotalItems = result.TotalItems, TotalPages = result.TotalPages });
    }

    [HttpPost(nameof(SeedPayments))]
    public async Task<IActionResult> SeedPayments()
    {
        if (!_configuration.GetValue<bool>("SeedData:IsSeeded"))
        {
            var command = new SeedPaymentCommand(GetUserId());
            await Mediator.Send(command);
            _configuration["SeedData:IsSeeded"] = "true";

            return Ok();
        }
        return BadRequest("Data has already been seeded");
    }
}
