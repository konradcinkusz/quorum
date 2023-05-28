namespace MR.Server.Controllers;

[Authorize]
public class SubscriptionController : MRBaseController
{
    public SubscriptionController(IMapper mapper) : base(mapper)
    {
    }

    private async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> ProcessSubscriptionRequest<T>(T command)
    {
        var result = await Mediator.Send(command) as PagedList<Subscription>;

        var DTOs = _mapper.Map<List<SubscriptionDTO>>(result);
        var response = new ApiResponse<SubscriptionPagedListDTO>(
            new SubscriptionPagedListDTO
            {
                Items = DTOs,
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            })
        { Success = true };

        return response;
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
    public async Task<ActionResult<ApiResponse<bool>>> BuySubscription()
    {
        try
        {
            var uId = GetUserId();
            var subscription = await Mediator.Send(new BuySubscriptionCommand { ApplicationUserId = uId });

            if (subscription == null || !subscription)
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

    [HttpGet("GetMyPayments")]
    public async Task<ActionResult<PaymentPagedListDTO>> GetMyPayments([FromQuery] PaymentSearchParamsDTO query)
    {
        var result = await Mediator.Send(new GetPaymentsBySearchParamsQuery
        {
            ApplicationUserEmail = GetUserId(),
            SearchParams = new SearchParams
            {
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize
            }
        });

        var paymentDTOs = _mapper.Map<List<PaymentDTO>>(result);

        return Ok(new PaymentPagedListDTO { Items = paymentDTOs, CurrentPage = result.CurrentPage, PageSize = result.PageSize, TotalItems = result.TotalItems, TotalPages = result.TotalPages });
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("GetSubscriptionsByQuery")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> 
        GetSubscriptionsByQuery([FromQuery] SubscriptionSearchParamsDTO query)
    {
        var command = new GetSubscriptionsBySearchParamsQuery
        {
            ApplicationUserEmail = query.ApplicationUserEmail,
            Begin = query.Begin,
            End = query.End,
            Activity = (GetSubscriptionsBySearchParamsQuery.ActivityEnum?)query.Activity
        };

        AddSearchParamsToCommand(command, query);

        return await ProcessSubscriptionRequest(command);
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
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

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("ActivateSubscription")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> ActivateSubscription()
    {
        return await ProcessSubscriptionRequest(new ActivateSubscriptionCommand());
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("GetSubscriptionsThatCouldBeActivate")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> GetSubscriptionsThatCouldBeActivate()
    {
        return await ProcessSubscriptionRequest(new GetSubscriptionsThatCouldBeActivateCommand());
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPost("DeactivateSubscription")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> DeactivateSubscription()
    {
        return await ProcessSubscriptionRequest(new DeactivateSubscriptionCommand());
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("GetSubscriptionsThatCouldBeDeactivate")]
    public async Task<ActionResult<ApiResponse<SubscriptionPagedListDTO>>> GetSubscriptionsThatCouldBeDeactivate()
    {
        return await ProcessSubscriptionRequest(new GetSubscriptionsThatCouldBeDeactivateCommand());
    }
}
