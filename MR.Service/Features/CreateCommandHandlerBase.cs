namespace MR.Service.Features;

public abstract class CreateCommandHandlerBase<TCommand, TResult, TCreate> : CommandHandlerBase<TCommand, TResult>
    where TCommand : IRequest<TResult>
    where TCreate : BaseEntity<TResult>
    where TResult : IEquatable<TResult>
{
    public CreateCommandHandlerBase(IApplicationDbContext context, ILogger<TCommand> logger) : base(context, logger)
    {
    }

    public override async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var created = await MakeAsync(request, cancellationToken);

        _ = await _context.Set<TCreate>().AddAsync(created);
        _ = await _context.SaveChangesAsync(cancellationToken);

        return created.Id;
    }

    protected abstract Task<TCreate> MakeAsync(TCommand command, CancellationToken cancellationToken);
}
