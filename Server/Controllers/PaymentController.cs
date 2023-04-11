using MR.Shared.ViewModel;

namespace MR.Server.Controllers;

public class PaymentController : MRBaseController
{
    public PaymentController(IMapper mapper) : base(mapper)
    {
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreatePayment(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var paymentId = await Mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetPayment), new { id = paymentId }, paymentId);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Infrastructure.ViewModel.PaymentViewModel>> GetPayment(Guid id)
    {
        var query = new GetPaymentQuery { PaymentId = id };
        var payment = await Mediator.Send(query);

        if (payment == null)
        {
            return NotFound();
        }

        var paymentViewModel = _mapper.Map<Infrastructure.ViewModel.PaymentViewModel>(payment);
        return Ok(paymentViewModel);
    }

}
