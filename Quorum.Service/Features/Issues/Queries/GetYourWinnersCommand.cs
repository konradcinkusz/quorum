namespace Quorum.Service.Features.Issues.Queries;

/// <summary>
/// Get issues that requires sign real form
/// </summary>
public class GetYourWinnersCommand : QueryBase, IRequest<PagedList<Issue>>
{
    internal class GetYourWinnersCommandHandler : CommandQueryHandlerBase<GetYourWinnersCommand, PagedList<Issue>>
    {
        public GetYourWinnersCommandHandler(IApplicationDbContext context, ILogger<GetYourWinnersCommand> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetYourWinnersCommand request, CancellationToken cancellationToken)
        {
            // Retrieve signed winner issues
            var query = _context.Issues
                .Include(x => x.CloudinaryFileIssues).ThenInclude(y => y.CloudinaryFile)
                .Where(issue => issue.Signatures.Any(signature => signature.SignaturePool.ApplicationUserId == request.ApplicationUserId))
                .Where(issue => issue.IssueProcess == IssueProcess.EndedInCurrentQuarter)
                .Where(issue => issue.IssueVisibility == IssueVisibility.VisibleForAll);

            var pagedList = await PagedList<Issue>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}