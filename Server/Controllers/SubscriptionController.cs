namespace MR.Server.Controllers;

[Authorize(Policy = "RequireAdminRole")]
public class SubscriptionController : MRBaseController
{
    public SubscriptionController(IMapper mapper) : base(mapper)
    {
    }
    [HttpPost(nameof(CreateSubscriptionForUser))]
    public async Task<IActionResult> CreateSubscriptionForUser(SubscriptionCreateForUserDTO subscriptionDto)
    {
        var SubscriptionId = await Mediator.Send(new CreateSubscriptionCommand
        {
            ApplicationUserId = subscriptionDto.ApplicationUserId,
            Begin = subscriptionDto.Begin,
            End = subscriptionDto.End
        });

        return CreatedAtAction(nameof(GetSubscription), new { id = SubscriptionId }, null);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionDTO>> GetSubscription(Guid id)
    {
        var Subscription = await Mediator.Send(new GetSubscriptionsBySearchParamsQuery { SubscriptionId = id });

        if (Subscription == null || !Subscription.Any())
        {
            return NotFound();
        }

        var SubscriptionDto = _mapper.Map<SubscriptionDTO>(Subscription.FirstOrDefault());

        return SubscriptionDto;
    }

    [HttpGet(nameof(GetSubscriptionsByQuery))]
    public async Task<ActionResult<SubscriptionPagedListDTO>> GetSubscriptionsByQuery([FromQuery] SubscriptionSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetSubscriptionsBySearchParamsQuery
        {
            ApplicationUserId = GetUserId(),
            SearchParams = new SearchParams
            {
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize
            }
        });

        var subscriptionDTOs = _mapper.Map<List<SubscriptionDTO>>(result);

        return Ok(new SubscriptionPagedListDTO { Items = subscriptionDTOs, CurrentPage = result.CurrentPage, PageSize = result.PageSize, TotalItems = result.TotalItems, TotalPages = result.TotalPages });
    }
}
