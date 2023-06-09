namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class AdminController : MRBaseController
{
    public AdminController(IMapper mapper) : base(mapper)
    {
    }

    private async Task<ActionResult<ApiResponse<AdminLogPagedListDTO>>> ProcessAdminRequest<T>(T command)
    where T : notnull
    {
        var result = await Mediator.Send(command) as PagedList<AdminLog>;

        if (result is null)
        {
            // Handle the case when result is null, for example by returning an appropriate error response
            return new ApiResponse<AdminLogPagedListDTO>
                ("Failed to process the admin log request.", (int)HttpStatusCode.BadRequest);
        }
        var DTOs = _mapper.Map<List<AdminLogDTO>>(result);
        var response =
            new ApiResponse<AdminLogPagedListDTO>(
                new AdminLogPagedListDTO
                {
                    Items = DTOs,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages
                })
            {
                Success = true
            };

        return response;
    }

    [HttpGet("get-user-email-by-user-id")]
    public async Task<ActionResult<ApiResponse<string>>> GetUserEmailByUserId([FromQuery] string userId) 
        => await HandleErrors(async () => await Mediator.Send(new GetUserEmailByUserIdCommand(userId)));

    [HttpGet("get-admin-logs-by-query")]
    public async Task<ActionResult<ApiResponse<AdminLogPagedListDTO>>> GetAdminLogsByQuery([FromQuery] AdminLogSearchParamsDTO query)
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

    [HttpPost("seed-payments")]
    public async Task<ActionResult<ApiResponse<PaymentPagedListDTO>>> SeedPayments([FromBody] SeedPaymentRequest seedPaymentRequest)
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
