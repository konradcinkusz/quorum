namespace MR.Server.Controllers;

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
        var payment = await Mediator.Send(new GetPaymentQuery { PaymentId = id });

        if (payment == null)
        {
            return NotFound();
        }

        var paymentDto = _mapper.Map<PaymentDTO>(payment);

        return paymentDto;
    }

    [HttpGet(nameof(GetPaymentsByQuery))]
    public async Task<ActionResult<PaymentPagedListDTO>> GetPaymentsByQuery([FromQuery] PaymentSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetPaymentsBySearchParamsQuery
        {
            ApplicationUserId = query.ApplicationUserId,
            MinPaymentValuePLN = query.MinPaymentValuePLN,
            MaxPaymentValuePLN = query.MaxPaymentValuePLN,
            SearchParams = new SearchParams
            {
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize,
                Name = query.Name,
                Question = query.Question
            }
        });

        var paymentDTOs = _mapper.Map<List<PaymentDTO>>(result);

        return Ok(new PaymentPagedListDTO { Items = paymentDTOs, CurrentPage = result.CurrentPage, PageSize = result.PageSize, TotalItems = result.TotalItems, TotalPages = result.TotalPages });
    }

    [HttpPut("EditPayment/{id}")]
    public async Task<IActionResult> EditPayment(Guid id, [FromBody] PaymentUpdateDTO paymentDto)
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

}
