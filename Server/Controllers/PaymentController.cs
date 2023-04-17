namespace MR.Server.Controllers;

public class PaymentController : MRBaseController
{
    public PaymentController(IMapper mapper) : base(mapper)
    {
    }

    [HttpPost]
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
            PaymentValuePLN = paymentDto.PaymentValuePLN
        });

        return CreatedAtAction(nameof(GetPayment), new { id = paymentId }, null);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentReadDTO>> GetPayment(Guid id)
    {
        var payment = await Mediator.Send(new GetPaymentQuery { PaymentId = id });

        if (payment == null)
        {
            return NotFound();
        }

        var paymentDto = new PaymentReadDTO
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
                .Select(h => new PaymentStatusHistoryDTO { PaymentStatus = EnumHelper.EnumToString(h.PaymentStatus) })
                .ToList()
        };

        return paymentDto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentReadDTO>>> GetPayments([FromQuery] PaymentQueryDTO query)
    {
        var result = await Mediator.Send(new GetPaymentsByQuery
        {
            UserEmail = query.UserEmail,
            ClientReferenceId = query.ClientReferenceId,
            PaymentIntentId = query.PaymentIntentId,
            MinPaymentValuePLN = query.MinPaymentValuePLN,
            MaxPaymentValuePLN = query.MaxPaymentValuePLN
        });

        var paymentDTOs = result.Select(payment => new PaymentReadDTO
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
                .Select(h => new PaymentStatusHistoryDTO { PaymentStatus = EnumHelper.EnumToString(h.PaymentStatus) })
                .ToList()
        }).ToList();

        return Ok(paymentDTOs);
    }
}
