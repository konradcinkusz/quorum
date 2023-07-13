namespace MR.Server.Controllers.Admin;

[Authorize(Policy = Constants.Policies.RequireAdminRole)]
public class AdminSubscriptionController : MRBaseController
{
    public AdminSubscriptionController(IMapper mapper) : base(mapper)
    {
    }

    [HttpGet("get-subscriptions-by-search-params")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> GetSubscriptionsBySearchParams([FromQuery] SubscriptionSearchParamsDTO searchParams)
    {
        var command = new GetSubscriptionsBySearchParamsQuery();
        SearchParamsExtension.AddSubscriptionsSearchParamsToCommand(command, searchParams);
        var result = await ProcessPagedRequest<GetSubscriptionsBySearchParamsQuery, SubscriptionDTO, Subscription>(command);
        result.Value?.Data.Items.ForEach(x => x.PaymentDTOs = x.PaymentDTOs.OrderByDescending(p => p.CreatedAt).ToList());

        return result;
    }

    [HttpPost("create-or-edit-subscription")]
    public async Task<ActionResult<ApiResponse<string>>> CreateOrEditSubscription(SubscriptionCreateForUserDTO subscriptionDto)
    {
        var subId = await Mediator.Send(new CreateSubscriptionCommand(subscriptionDto.ApplicationUserId)
        {
            Begin = subscriptionDto.Begin,
            End = subscriptionDto.End
        });
        if (subId)
            return new ApiResponse<string>(subscriptionDto.ApplicationUserId) { StatusCode = (int)HttpStatusCode.Created };
        return new ApiResponse<string>() { StatusCode = (int)HttpStatusCode.BadRequest, Success = false, Message = "Something went wrong" };
    }

    [HttpPut("activate-subscription/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateSubscription(string id)
        => await HandleErrors(async () => await Mediator.Send(new ActivateSubscriptionCommand(id)), "Subscription activated");

    [HttpPost("activate-subscriptions")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> ActivateSubscriptions()
        => await ProcessPagedRequest<ActivateSubscriptionsCommand, SubscriptionDTO, Subscription>(new ActivateSubscriptionsCommand());

    [HttpGet("get-subscriptions-that-could-be-activated")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> GetSubscriptionsThatCouldBeActivated()
        => await ProcessPagedRequest<GetSubscriptionsThatCouldBeActivateCommand, SubscriptionDTO, Subscription>(new GetSubscriptionsThatCouldBeActivateCommand());

    [HttpPut("deactivate-subscription/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateSubscription(string id)
        => await HandleErrors(async () => await Mediator.Send(new DeactivateSubscriptionCommand(id)), "Subscription deactivated");

    [HttpGet("get-subscriptions-that-could-be-deactivated")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> GetSubscriptionsThatCouldBeDeactivated()
        => await ProcessPagedRequest<GetSubscriptionsThatCouldBeDeactivateCommand, SubscriptionDTO, Subscription>(new GetSubscriptionsThatCouldBeDeactivateCommand());
}
