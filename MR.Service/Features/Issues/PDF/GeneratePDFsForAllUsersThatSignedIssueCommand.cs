namespace MR.Service.Features.Issues.PDF;

public class GeneratePDFsForAllUsersThatSignedIssueCommand : IRequest<bool>
{
    private readonly Guid _quarterId;

    public GeneratePDFsForAllUsersThatSignedIssueCommand(Guid quarterId)
    {
        _quarterId = quarterId;
    }

    internal class GeneratePDFsForAllUsersThatSignedIssueCommandHandler : CommandHandlerBase<GeneratePDFsForAllUsersThatSignedIssueCommand, bool>
    {
        public GeneratePDFsForAllUsersThatSignedIssueCommandHandler(IApplicationDbContext context, ILogger<GeneratePDFsForAllUsersThatSignedIssueCommand> logger) : base(context, logger)
        {
        }

        public override Task<bool> Handle(GeneratePDFsForAllUsersThatSignedIssueCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}