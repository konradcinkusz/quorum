namespace MR.Server.Controllers;

[Authorize]
public class SubscriptionController : MRBaseController
{
    public SubscriptionController(IMapper mapper) : base(mapper)
    {
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
            if (lastPayment == null && sub.IsActive)
            {
                sub.SubscriptionViewStatusEnum = SubscriptionViewStatusEnum.YourSubHasBeenActivatedByAdmin;
            }
            else if (lastPayment == null)
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
            else if (!sub.IsActive && lastPayment != null)
            {
                sub.SubscriptionViewStatusEnum = SubscriptionViewStatusEnum.YouDontHaveActiveSub;
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

    [HttpPost("reject-subscription")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectSubscription()
    {
        var result = await Mediator.Send(new RejectSubscriptionCommand(GetUserId()));
        if (result)
            return new ApiResponse<bool>(result) { Success = true, Message = "Succesfully rejected", StatusCode = (int)HttpStatusCode.Accepted };
        return new ApiResponse<bool>(result) { Success = false, Message = "Problem with rejected", StatusCode = (int)HttpStatusCode.BadRequest };
    }
}
