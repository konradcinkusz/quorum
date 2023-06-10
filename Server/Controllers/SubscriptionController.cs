namespace MR.Server.Controllers;

[Authorize]
public class SubscriptionController : MRBaseController
{
    public SubscriptionController(IMapper mapper) : base(mapper)
    {
    }

    private async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> ProcessSubscriptionRequest<T>(T command)
    where T : notnull
    {
        var result = await Mediator.Send(command) as PagedList<Subscription>;

        if (result is null)
        {
            // Handle the case when result is null, for example by returning an appropriate error response
            return new ApiResponse<SubscriptionPagedListDTO>("Failed to process the subscription request.", (int)HttpStatusCode.BadRequest);
        }

        var DTOs = _mapper.Map<List<SubscriptionDTO>>(result);
        var response =
            new ApiResponse<SubscriptionPagedListDTO>(
                new SubscriptionPagedListDTO
                {
                    Items = DTOs,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages
                })
            {
                Success = true
            };

        return response;
    }

    [HttpGet("get-my-subscription")]
    public async Task<ActionResult<ApiResponse<SubscriptionReadDTO>>> GetMySubscription()
    {
        var response = new ApiResponse<SubscriptionReadDTO>
        {
            Data = new SubscriptionReadDTO()
        };
        try
        {
            var subscription = await Mediator.Send(new GetMySubscription(GetUserId()));

            var lastPayment = subscription.SubscriptionPayments
                .Select(x => x.Payment)
                .OrderByDescending(x => x?.CreatedAt)
                .FirstOrDefault();

            var sub = _mapper.Map<SubscriptionReadDTO>(subscription);
            sub.PaymentStatus = (PaymentStatusEnum?)lastPayment?.PaymentStatus;
            sub.PaymentDate = lastPayment?.CreatedAt;

            response.Data = sub;
            if (lastPayment == null)
            {
                sub.SubscriptionViewStatusEnum = SubscriptionViewStatusEnum.NoPaymentYouHaveToBuySubscription;
            }
            else if (!sub.Begin.HasValue && !sub.End.HasValue)
            {
                if (lastPayment.PaymentStatus == PaymentStatus.Pending)
                {
                    sub.SubscriptionViewStatusEnum = SubscriptionViewStatusEnum.SubBoughtAndWaitingForPayment;
                }
                else
                {
                    sub.SubscriptionViewStatusEnum = SubscriptionViewStatusEnum.SubBoughtButSomethingHappendWithAPayment;
                }
            }
            else if (sub.IsActive && lastPayment.PaymentStatus == PaymentStatus.Completed)
            {
                sub.SubscriptionViewStatusEnum = SubscriptionViewStatusEnum.YouHaveAnActiveSub;
            }
            else if (!sub.IsActive && lastPayment.PaymentStatus == PaymentStatus.Accepted)
            {
                sub.SubscriptionViewStatusEnum = SubscriptionViewStatusEnum.PaymentHasBeenAcceptedWaitingForAdminActivation;
            }
            response.Data = sub;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            response.Success = false;
            response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        return response;
    }

    [HttpPost("buy-subscription")]
    public async Task<ActionResult<ApiResponse<bool>>> BuySubscription()
    {
        try
        {
            var subscription = await Mediator.Send(new BuySubscriptionCommand(GetUserId()));

            if (!subscription)
            {
                return new ApiResponse<bool> { StatusCode = (int)HttpStatusCode.NotFound, Success = false, Message = "Subscription not found" };
            }

            return new ApiResponse<bool> { StatusCode = (int)HttpStatusCode.Created, Success = true, Message = "Subscription purchased successfully" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { StatusCode = (int)HttpStatusCode.BadRequest, Success = false, Message = ex.Message };
        }
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("get-subscriptions-by-search-params")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>>
        GetSubscriptionsBySearchParams([FromQuery] SubscriptionSearchParamsDTO searchParams)
    {
        var command = new GetSubscriptionsBySearchParamsQuery
        {
            ApplicationUserId = searchParams.ApplicationUserId,
            ApplicationUserEmail = searchParams.ApplicationUserEmail,
            Begin = searchParams.Begin,
            End = searchParams.End,
            Activity = (GetSubscriptionsBySearchParamsQuery.ActivityEnum?)searchParams.Activity
        };

        AddSearchParamsToCommand(command, searchParams);

        var result = await ProcessSubscriptionRequest(command);
        result.Value?.Data.Items.ForEach(x => x.PaymentDTOs = x.PaymentDTOs.OrderByDescending(p => p.CreatedAt).ToList());

        return result;
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
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

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("activate-subscription")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> ActivateSubscription()
    {
        return await ProcessSubscriptionRequest(new ActivateSubscriptionCommand());
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("get-subscriptions-that-could-be-activated")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> GetSubscriptionsThatCouldBeActivated()
    {
        return await ProcessSubscriptionRequest(new GetSubscriptionsThatCouldBeActivateCommand());
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("deactivate-subscription")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> DeactivateSubscription([FromBody] string applicationUserId)
    {
        return await ProcessSubscriptionRequest(new DeactivateSubscriptionCommand(applicationUserId));
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("get-subscriptions-that-could-be-deactivated")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> GetSubscriptionsThatCouldBeDeactivated()
    {
        return await ProcessSubscriptionRequest(new GetSubscriptionsThatCouldBeDeactivateCommand());
    }

    [HttpPost("reject-subscription")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectSubscription()
    {
        var result = await Mediator.Send(new RejectSubscriptionCommand(GetUserId()));
        if (result)
            return new ApiResponse<bool>(result) { Success = true, Message = "Succesfully rejected", StatusCode = (int)HttpStatusCode.Accepted };
        return new ApiResponse<bool>(result) { Success = false, Message = "Problem with rejected", StatusCode = (int)HttpStatusCode.BadRequest };
    }
}
