namespace Quorum.Service.Features.QuarterFeatures;

/// <summary>
/// Delete od force delete różni się tym, że nie usuwamy powiązań
/// W force delete usuwamy wszystkie powiązane obiekty -> patrz na ForceDeleteIssueCommand
/// </summary>
public class DeleteQuarterCommand : IRequest<bool>
{
    private readonly Guid _quarterId;
    public DeleteQuarterCommand(Guid quarterId)
    {
        _quarterId = quarterId;
    }

    internal class DeleteQuarterCommandHandler : CommandHandlerBase<DeleteQuarterCommand, bool>
    {
        public DeleteQuarterCommandHandler(IApplicationDbContext context, ILogger<DeleteQuarterCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(DeleteQuarterCommand request, CancellationToken cancellationToken)
        {
            bool result = false;

            var quarter = await _context.Quarters.FirstOrDefaultAsync(x => x.Id == request._quarterId, cancellationToken);
            
            if (quarter != null)
            {
                _context.Quarters.Remove(quarter);
                result = await _context.SaveChangesAsync(cancellationToken) > 0;
            }

            return result;
        }
    }
}
