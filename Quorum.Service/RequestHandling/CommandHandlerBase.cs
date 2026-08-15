namespace Quorum.Service.RequestHandling;

public abstract class CommandHandlerBase<TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    protected readonly IApplicationDbContext _context;
    protected readonly ILogger<TCommand> _logger;

    protected CommandHandlerBase(IApplicationDbContext context, ILogger<TCommand> logger)
    {
        _context = context;
        _logger = logger;
    }

    public abstract Task<TResult> Handle(TCommand request, CancellationToken cancellationToken);
}
