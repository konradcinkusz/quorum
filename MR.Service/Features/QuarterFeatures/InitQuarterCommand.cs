namespace MR.Service.Features.QuarterFeatures;

public class InitQuarterCommand : IRequest<Guid>
{
    private readonly int _year;
    private readonly int _month;
    public int SignaturesCount { get; set; } = 3;
    public InitQuarterCommand(int year, int month)
    {
        _year = year;
        _month = month;
    }

    internal class InitQuarterCommandHandler : CommandHandlerBase<InitQuarterCommand, Guid>
    {
        private readonly MRUserManager _MRUserManager;
        public InitQuarterCommandHandler(MRUserManager MRUserManager, IApplicationDbContext context, ILogger<InitQuarterCommand> logger) : base(context, logger)
        {
            _MRUserManager = MRUserManager;
        }

        public override async Task<Guid> Handle(InitQuarterCommand request, CancellationToken cancellationToken)
        {
            var q = await _context.Quarters.FirstOrDefaultAsync(x => x.Year == request._year && x.QuarterNumber == request._month, cancellationToken);
            if (q != null)
            {
                throw new ApplicationException("There is already a quarter at this dates");
            }
            var quarter = new Quarter { QuarterNumber = request._month, Year = request._year, PrimarySignatureCount = request.SignaturesCount };
            await _context.Quarters.AddAsync(quarter, cancellationToken);

            if (request.SignaturesCount > 0)
            {
                //dla wszytkich userów dodaj pule sygnatur na ten kwartał
                foreach (var applicationUserId in _MRUserManager.Users.Select(x => x.Id))
                {
                    var signaturePool = new SignaturePool
                    {
                        ApplicationUserId = applicationUserId,
                        QuarterId = quarter.Id,
                        Signatures = new List<Signature>()
                    };

                    for (int i = 0; i < request.SignaturesCount; i++)
                    {
                        signaturePool.Signatures.Add(new());
                    }

                    await _context.SignaturePools.AddAsync(signaturePool, cancellationToken);
                }
            }

            return await _context.SaveChangesAsync(cancellationToken) > 0 ? quarter.Id : Guid.Empty;
        }
    }
}
