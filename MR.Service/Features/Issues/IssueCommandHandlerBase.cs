using System.Threading;

namespace MR.Service.Features.Issues;

public abstract class IssueCommandHandlerBase<TCommand, TResult> : CommandHandlerBase<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    private readonly MRUserManager _MRUserManager;

    protected IssueCommandHandlerBase(
        MRUserManager MRUserManager, IApplicationDbContext context, ILogger<TCommand> logger) : base(context, logger)
    {
        _MRUserManager = MRUserManager;
    }

    protected async Task<Issue> CheckBasicConditions(IIssueCommandData request, CancellationToken cancellationToken)
    {
        var isActiveSub = await _MRUserManager.HasActiveSubscription(request.CreatedById);

        if (!isActiveSub)
        {
            throw new ApplicationException("You don't have an active sub and you cannot publish issue.");
        }

        var issue = await _context.Issues
            .Include(x => x.InitialPayment)
            .FirstAsync(x => x.Id == request.IssueId, cancellationToken);

        return issue;
    }
}