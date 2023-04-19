namespace MR.Server;

[Route(Constants.RouteValues.AdvanceAPIRoute)]
[ApiController]
[ApiVersion(Constants.RouteValues.APIV1)]
public abstract class MRBaseController : ControllerBase
{
    protected readonly IMapper _mapper;

    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    public MRBaseController(
        IMapper mapper)
    {
        _mapper = mapper;
    }

    protected virtual string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
