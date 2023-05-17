namespace MR.Service.Features.SignatureFeautres;

public class RemoveSignatureCommand : IRequest<bool>
{
    public Guid SignatureId { get; set; }

    public class RemoveSignatureCommandHandler : CommandHandlerBase<RemoveSignatureCommand, bool>
    {
        public RemoveSignatureCommandHandler(IApplicationDbContext context, ILogger<RemoveSignatureCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(RemoveSignatureCommand request, CancellationToken cancellationToken)
        {
            var signature = await _context.Signatures.FirstOrDefaultAsync(x=>x.Id == request.SignatureId, cancellationToken);

            if (signature == null)
            {
                return false;
            }
            else
            {
                _context.Signatures.Remove(signature);
               return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
        }
    }
}
