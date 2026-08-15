namespace Quorum.Service.Features.Issues.Base;

public interface IIssueCommandData
{
    string CreatedById { get; }
    Guid IssueId { get; }
}

public abstract class IssueCommandHandlerBase<TCommand, TResult> : CommandHandlerBase<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    private readonly IQuorumUserService _users;

    protected IssueCommandHandlerBase(
        IQuorumUserService users, IApplicationDbContext context, ILogger<TCommand> logger) : base(context, logger)
    {
        _users = users;
    }

    /// <summary>
    /// Loads the issue a command targets, having established that the caller both has an
    /// active subscription and owns it.
    /// <para>
    /// The ownership half is the important half and used to be missing: this method checked
    /// only the subscription, so any subscriber could publish or pay for any other user's
    /// issue by presenting its id. Every command routed through here inherits the check, so
    /// it must stay in this method rather than being repeated by each handler.
    /// </para>
    /// </summary>
    protected async Task<Issue> CheckBasicConditionsAndReturnIssue(IIssueCommandData request, CancellationToken cancellationToken)
    {
        var isActiveSub = await _users.HasActiveSubscriptionAsync(request.CreatedById, cancellationToken);

        if (!isActiveSub)
        {
            throw new ApplicationException("You don't have an active sub and you cannot publish issue.");
        }

        var issue = await _context.Issues
            .RestrictToOwner(IssueOwnerScope.OwnedBy(request.CreatedById))
            .Include(x => x.InitialPayment)
            .FirstOrDefaultAsync(x => x.Id == request.IssueId, cancellationToken);

        // Same result whether the issue does not exist or belongs to someone else.
        if (issue == null)
        {
            throw new NotFoundException(nameof(Issue), request.IssueId);
        }

        return issue;
    }
}