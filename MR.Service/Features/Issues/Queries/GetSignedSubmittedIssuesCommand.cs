namespace MR.Service.Features.Issues.Queries;

/// <summary>
/// Get issues that requires sign real form
/// </summary>
public class GetSignedSubmittedIssuesCommand : QueryBase, IRequest<PagedList<Issue>>
{
    internal class GetSignedSubmittedIssuesCommandHandler : CommandQueryHandlerBase<GetSignedSubmittedIssuesCommand, PagedList<Issue>>
    {
        public GetSignedSubmittedIssuesCommandHandler(IApplicationDbContext context, ILogger<GetSignedSubmittedIssuesCommand> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetSignedSubmittedIssuesCommand request, CancellationToken cancellationToken)
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