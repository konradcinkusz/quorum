namespace MR.Server.Controllers;

[Authorize(Policy = Policies.RequireAdminRole)]
public class PaymentController : MRBaseController
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
        return await ProcessPagedRequest<GetPaymentsBySearchParamsQuery, PaymentDTO, Payment>(command);
    }

    [HttpPut("edit-payment/{id}")]
    public async Task<IActionResult> EditPayment([FromRoute] Guid id, [FromBody] PaymentUpdateDTO paymentDto)
    {
        if (id != paymentDto.Id)
        {
            return BadRequest("Payment ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        EditPaymentCommand command = _mapper.Map<EditPaymentCommand>(paymentDto);
        command.ApplicationUserId = GetUserId();
        command.PaymentId = id;

        try
        {
            var paymentId = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetPayment), new { id = paymentId }, null);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("accept-payment/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> AcceptPayment(Guid id)
        => await HandleErrors(async () => await Mediator.Send(new AcceptPaymentCommand(id)), $"Payment {id} accepted");
}
