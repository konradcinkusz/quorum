using MR.Shared.DTOs.Subscription;

namespace MR.Server.Controllers;

[Authorize]
public class SubscriptionController : MRBaseController
{
    public SubscriptionController(IMapper mapper) : base(mapper)
    {
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost(nameof(CreateSubscriptionForUser))]
    public async Task<IActionResult> CreateSubscriptionForUser(SubscriptionCreateForUserDTO subscriptionDto)
    {
        var SubscriptionId = await Mediator.Send(new CreateSubscriptionCommand
        {
            ApplicationUserId = subscriptionDto.ApplicationUserId,
            Begin = subscriptionDto.Begin,
            End = subscriptionDto.End
        });

        return CreatedAtAction(nameof(GetSubscription), new { id = subscriptionDto.ApplicationUserId }, null);
    }

    [HttpGet]
    public async Task<ActionResult<SubscriptionReadDTO>> GetSubscription()
    {
        var Subscription = await Mediator.Send(new GetSubscriptionsBySearchParamsQuery { ApplicationUserId = GetUserId() });

        if (Subscription == null || !Subscription.Any())
        {
            return NotFound();
        }

        var SubscriptionDto = _mapper.Map<SubscriptionReadDTO>(Subscription.FirstOrDefault());

        SubscriptionDto.Price = 5;
        var payment = await Mediator.Send(new GetSubscriptionPayment { ApplicationUserId = GetUserId() });
        var paymentDTO = _mapper.Map<PaymentDTO>(payment);
        SubscriptionDto.LastPayment = paymentDTO;
        return SubscriptionDto;
    }

    [HttpPost]
    public async Task<ActionResult<bool>> BuySubscription()
    {
        var uId = GetUserId();
        var Subscription = await Mediator.Send(new BuySubscriptionCommand { ApplicationUserId = uId });

        if (Subscription == null || !Subscription)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetSubscription), new { id = uId }, null);
    }


    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet(nameof(GetSubscriptionsByQuery))]
    public async Task<ActionResult<SubscriptionPagedListDTO>> GetSubscriptionsByQuery([FromQuery] SubscriptionSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetSubscriptionsBySearchParamsQuery
        {
            ApplicationUserId = query.ApplicationUserId,
            SearchParams = new SearchParams
            {
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize
            }
        });

        var subscriptionDTOs = _mapper.Map<List<SubscriptionDTO>>(result);

        return Ok(new SubscriptionPagedListDTO { Items = subscriptionDTOs, CurrentPage = result.CurrentPage, PageSize = result.PageSize, TotalItems = result.TotalItems, TotalPages = result.TotalPages });
    }

    [HttpGet(nameof(GetMyPayments))]
    public async Task<ActionResult<PaymentPagedListDTO>> GetMyPayments([FromQuery] PaymentSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetPaymentsBySearchParamsQuery
        {
            ApplicationUserId = GetUserId(),
            SearchParams = new SearchParams
            {
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize,
                Question = query.Description
            }
        });

        var paymentDTOs = _mapper.Map<List<PaymentDTO>>(result);

        return Ok(new PaymentPagedListDTO { Items = paymentDTOs, CurrentPage = result.CurrentPage, PageSize = result.PageSize, TotalItems = result.TotalItems, TotalPages = result.TotalPages });
    }
}
