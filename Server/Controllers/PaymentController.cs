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
            UserEmail = paymentDto.UserEmail,
            PaymentLink = paymentDto.PaymentLink,
            ClientReferenceId = paymentDto.ClientReferenceId,
            PaymentIntentId = paymentDto.PaymentIntentId,
            SessionId = paymentDto.SessionId,
            ApplicationUserId = uId,
            PaymentValuePLN = paymentDto.PaymentValuePLN.HasValue ? paymentDto.PaymentValuePLN.Value : -1
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

        var paymentDto = new PaymentDTO
        {
            Id = payment.Id,
            UserEmail = payment.UserEmail,
            PaymentLink = payment.PaymentLink,
            ClientReferenceId = payment.ClientReferenceId,
            PaymentIntentId = payment.PaymentIntentId,
            SessionId = payment.SessionId,
            CreatedAt = payment.CreatedAt,
            PaymentValuePLN = payment.PaymentValuePLN,
            PaymentStatusHistory = payment.PaymentStatusHistories
                .Select(h => new PaymentStatusHistoryDTO { PaymentStatus = EnumHelper.EnumToString(h.PaymentStatus), StatusDate = h.CreatedAt })
                .ToList()
        };

        return paymentDto;
    }

    [HttpGet(nameof(GetPaymentsByQuery))]
    public async Task<ActionResult<PaymentPagedListDTO>> GetPaymentsByQuery([FromQuery] PaymentSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetPaymentsBySearchParamsQuery
        {
            UserEmail = query.UserEmail,
            ClientReferenceId = query.ClientReferenceId,
            PaymentIntentId = query.PaymentIntentId,
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

        var paymentDTOs = result.Select(payment => new PaymentDTO
        {
            Id = payment.Id,
            UserEmail = payment.UserEmail,
            PaymentLink = payment.PaymentLink,
            ClientReferenceId = payment.ClientReferenceId,
            PaymentIntentId = payment.PaymentIntentId,
            SessionId = payment.SessionId,
            CreatedAt = payment.CreatedAt,
            PaymentValuePLN = payment.PaymentValuePLN,
            PaymentStatusHistory = payment.PaymentStatusHistories
                .Select(h => new PaymentStatusHistoryDTO { PaymentStatus = EnumHelper.EnumToString(h.PaymentStatus), StatusDate = h.CreatedAt })
                .ToList()
        }).ToList();

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

        var command = new EditPaymentCommand
        {
            PaymentId = id,
            UserEmail = paymentDto.UserEmail,
            PaymentLink = paymentDto.PaymentLink,
            ClientReferenceId = paymentDto.ClientReferenceId,
            PaymentIntentId = paymentDto.PaymentIntentId,
            SessionId = paymentDto.SessionId,
            PaymentStatus = PaymentStatus.Pending,
            ApplicationUserId = GetUserId(),
            PaymentValuePLN = paymentDto.PaymentValuePLN
        };

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
