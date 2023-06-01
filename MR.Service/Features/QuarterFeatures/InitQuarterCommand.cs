namespace MR.Service.Features.QuarterFeatures;

public class InitQuarterCommand : IRequest<Guid>
{
    public int Year { get; set; }
    public int Month { get; set; }
    //ile sygnatur należy dać wszystkich userom
    public int SignaturesCount { get; set; } = 3;
    public class InitQuarterCommandHandler : CommandHandlerBase<InitQuarterCommand, Guid>
    {
        private readonly MRUserManager _MRUserManager;
        public InitQuarterCommandHandler(
            MRUserManager MRUserManager,
            IApplicationDbContext context,
            ILogger<InitQuarterCommand> logger) : base(context, logger)
        {
            _MRUserManager = MRUserManager;
        }

        public override async Task<Guid> Handle(InitQuarterCommand request, CancellationToken cancellationToken)
        {
            var q = await _context.Quarters.FirstOrDefaultAsync(x => x.Year == request.Year && x.QuarterNumber == request.Month);
            if (q != null)
            {
                throw new ApplicationException("There is already a quarter at this dates");
            }
            var quarter = new Quarter { QuarterNumber = request.Month, Year = request.Year };
            await _context.Quarters.AddAsync(quarter, cancellationToken);

            //dla wszytkich userów dodaj pule sygnatur na ten kwartał
            foreach (var uId in _MRUserManager.Users.Select(x => x.Id))
            {
                var sPool = new SignaturePool
                {
                    ApplicationUserId = uId,
                    QuarterId = quarter.Id
                };
                sPool.Signatures = new List<Signature>();
                for (int i = 0; i < request.SignaturesCount; i++)
                {
                    sPool.Signatures.Add(new());
                }
                await _context.SignaturePools.AddAsync(sPool);
            }

            int result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0 ? quarter.Id : Guid.Empty;
        }
    }
}
