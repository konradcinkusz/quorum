namespace MR.Service.Features.SignatureFeautres;

public class UnpinSignatureFromIssueCommand : IRequest<bool>
{
    public Guid SignatureId { get; set; }

    public class UnpinIssueCommandHandler : CommandHandlerBase<UnpinSignatureFromIssueCommand, bool>
    {
        public UnpinIssueCommandHandler(IApplicationDbContext context, ILogger<UnpinSignatureFromIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(UnpinSignatureFromIssueCommand request, CancellationToken cancellationToken)
        {
            var signature = await _context.Signatures.FirstOrDefaultAsync(x => x.Id == request.SignatureId, cancellationToken);

            if (signature == null)
            {
                return false;
            }
            else
            {
                signature.Issue = null;
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
        }
    }
}