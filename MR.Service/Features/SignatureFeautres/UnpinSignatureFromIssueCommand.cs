namespace MR.Service.Features.SignatureFeautres;

public class UnpinSignatureFromIssueCommand : IRequest<bool>
{
    private readonly Guid _signatureId;
    public UnpinSignatureFromIssueCommand(Guid signatureId)
    {
        _signatureId = signatureId;
    }

    internal class UnpinIssueCommandHandler : CommandHandlerBase<UnpinSignatureFromIssueCommand, bool>
    {
        public UnpinIssueCommandHandler(IApplicationDbContext context, ILogger<UnpinSignatureFromIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(UnpinSignatureFromIssueCommand request, CancellationToken cancellationToken)
        {
            bool result = false;
            var signature = await _context.Signatures.FirstOrDefaultAsync(x => x.Id == request._signatureId, cancellationToken);

            if (signature != null)
            {
                signature.Issue = null;
                result = await _context.SaveChangesAsync(cancellationToken) > 0;
            }

            return result;
        }
    }
}