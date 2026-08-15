namespace Quorum.Service.Features.SignaturePoolsFeatures;

public class AddSignatureToSignaturePoolCommand : IRequest<bool>
{
    public Guid SignaturePoolId { get; set; }

    public class AddSignatureToSignaturePoolCommandHandler : CommandHandlerBase<AddSignatureToSignaturePoolCommand, bool>
    {
        public AddSignatureToSignaturePoolCommandHandler(IApplicationDbContext context, ILogger<AddSignatureToSignaturePoolCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(AddSignatureToSignaturePoolCommand request, CancellationToken cancellationToken)
        {
            var signaturePool = await _context.SignaturePools.Where(x=>x.Id == request.SignaturePoolId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            
            if (signaturePool != null)
            {
                _ = await _context.Signatures.AddAsync(new Signature { SignaturePoolId = signaturePool.Id });
                return (await _context.SaveChangesAsync(cancellationToken)) > 0;
            }
            else
            {
                return false;
            }
        }
    }
}
