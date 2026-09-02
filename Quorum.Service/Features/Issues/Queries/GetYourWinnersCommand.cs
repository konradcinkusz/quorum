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
            // Retrieve signed winner issues. The three clauses this used to spell out inline
            // are SignedDocumentAccess.RestrictToSignatory, which the upload and download
            // paths also call -- so what a user may see here and what they may fetch cannot
            // drift apart.
            var query = _context.Issues
                .Include(x => x.CloudinaryFileIssues).ThenInclude(y => y.CloudinaryFile)
                .RestrictToSignatory(request.ApplicationUserId);

            var pagedList = await PagedList<Issue>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}