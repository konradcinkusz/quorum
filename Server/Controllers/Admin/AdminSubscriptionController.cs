namespace MR.Server.Controllers.Admin;

[Authorize(Policy = Policies.RequireAdminRole)]
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
        AddSearchParamsToCommand(command, searchParams);

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

    [HttpPost("activate-subscription")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> ActivateSubscription()
        => await ProcessPagedRequest<ActivateSubscriptionCommand, SubscriptionDTO, Subscription>(new ActivateSubscriptionCommand());

    [HttpGet("get-subscriptions-that-could-be-activated")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> GetSubscriptionsThatCouldBeActivated()
        => await ProcessPagedRequest<GetSubscriptionsThatCouldBeActivateCommand, SubscriptionDTO, Subscription>(new GetSubscriptionsThatCouldBeActivateCommand());

    [HttpPost("deactivate-subscription")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> DeactivateSubscription([FromBody] string applicationUserId)
        => await ProcessPagedRequest<DeactivateSubscriptionCommand, SubscriptionDTO, Subscription>(new DeactivateSubscriptionCommand(applicationUserId));

    [HttpGet("get-subscriptions-that-could-be-deactivated")]
    public async Task<ActionResult<ApiResponse<PagedListDto<SubscriptionDTO>>>> GetSubscriptionsThatCouldBeDeactivated()
        => await ProcessPagedRequest<GetSubscriptionsThatCouldBeDeactivateCommand, SubscriptionDTO, Subscription>(new GetSubscriptionsThatCouldBeDeactivateCommand());
}
