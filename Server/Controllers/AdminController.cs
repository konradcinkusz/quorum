namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class AdminController : MRBaseController
{
    public AdminController(IMapper mapper) : base(mapper)
    {
    }

    private async Task<ActionResult<ApiResponse<AdminLogPagedListDTO>>> ProcessAdminRequest<T>(T command)
    {
        var result = await Mediator.Send(command) as PagedList<AdminLog>;

        var DTOs = _mapper.Map<List<AdminLogDTO>>(result);
        var response = new ApiResponse<AdminLogPagedListDTO>(
            new AdminLogPagedListDTO
            {
                Items = DTOs,
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            })
        { Success = true };

        return response;
    }

    [HttpGet("GetAdminLogsByQuery")]
    public async Task<ActionResult<ApiResponse<AdminLogPagedListDTO>>>
        GetAdminLogsByQuery([FromQuery] AdminLogSearchParamsDTO query)
    {
        var command = new GetAdminLogsBySearchParamsQuery
        {
            LastHour = query.LastHour,
            LastMonth = query.LastMonth,
            ValuesText = query.ValuesText,
            Action = query.Action
        };

        AddSearchParamsToCommand(command, query);

        return await ProcessAdminRequest(command);
    }

    [HttpPost("SeedPayments")]
    public async Task<ActionResult<ApiResponse<PaymentPagedListDTO>>> SeedPayments(
        [FromBody] SeedPaymentRequest seedPaymentRequest)
    {
        var result = await Mediator.Send(new SeedPaymentCommand(GetUserId()) { Count = seedPaymentRequest.Count });

        var paymentDTOs = _mapper.Map<List<PaymentDTO>>(result);
        var response = new ApiResponse<PaymentPagedListDTO>(
            new PaymentPagedListDTO
            {
                Items = paymentDTOs,
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            })
        { Success = true };

        return response;
    }
}
