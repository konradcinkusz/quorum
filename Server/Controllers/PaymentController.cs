namespace Quorum.Server.Controllers;

[Authorize(Policy = Constants.Policies.RequireAdminRole)]
public class PaymentController : QuorumBaseController
{
    public PaymentController(IMapper mapper) : base(mapper)
    {
    }

    [HttpPost(nameof(CreatePayment))]
    public async Task<IActionResult> CreatePayment(PaymentCreateDTO paymentDto)
    {
        string uId = GetUserId();
        var paymentId = await Mediator.Send(new CreatePaymentCommand
        {
            PaymentStatus = PaymentStatus.New,
            PaymentMethod = paymentDto.PaymentMethod,
            ReferenceNumber = paymentDto.ReferenceNumber,
            ApplicationUserId = uId,
            PaymentValuePLN = paymentDto.PaymentValuePLN
        });

        return CreatedAtAction(nameof(GetPayment), new { id = paymentId }, null);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDTO>> GetPayment(Guid id)
    {
        var payment = await Mediator.Send(new GetPaymentsBySearchParamsQuery { PaymentId = id });

        if (payment.FirstOrDefault() == null)
        {
            return NotFound();
        }

        var paymentDto = _mapper.Map<PaymentDTO>(payment.FirstOrDefault());

        return paymentDto;
    }

    [HttpGet("get-payments-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<PaymentDTO>>>> GetPaymentsByQuery([FromQuery] PaymentSearchParamsDTO searchParams)
    {
        var command = new GetPaymentsBySearchParamsQuery();
        SearchParamsExtension.AddPaymentSearchParamsToCommand(command, searchParams);
        var result = await ProcessPagedRequest<GetPaymentsBySearchParamsQuery, PaymentDTO, Payment>(command);
        return result;
    }

    [HttpPut("edit-payment/{id}")]
    public async Task<ActionResult<ApiResponse<int>>> EditPayment([FromRoute] Guid id, [FromBody] PaymentUpdateDTO paymentDto)
    {
        var changedPropertiesCount = await Mediator.Send(new EditPaymentCommand(id)
        {
            ApplicationUserId = paymentDto.ApplicationUserId,
            ReferenceNumber = paymentDto.ReferenceNumber,
            PaymentMethod = paymentDto.PaymentMethod,
            PaymentStatus = (PaymentStatus?)paymentDto.PaymentStatus,
            PaymentValuePLN = paymentDto.PaymentValuePLN
        });
        return new ApiResponse<int> { Data = changedPropertiesCount };
    }

    [HttpPut("accept-payment/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> AcceptPayment(Guid id)
        => await HandleErrors(async () => await Mediator.Send(new AcceptPaymentCommand(id)), $"Payment {id} accepted");

    [HttpPut("accept-initial-payment/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> AcceptIssueInitialPayment(Guid id)
        => await HandleErrors(async () => await Mediator.Send(new AcceptIssueInitialPaymentCommand(id)), $"Payment {id} accepted");

    [HttpPut("set-payment-status/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> SetPaymentStatus([FromRoute] Guid id, [FromBody] PaymentStatusEnum paymentStatus)
        => await HandleErrors(async () => await Mediator.Send(new SetPaymentStatusCommand(id, (PaymentStatus)paymentStatus)), $"Payment {id} accepted");
}
