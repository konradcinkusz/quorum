namespace MR.Service.Features.AdminFeatures;

public class GetUserEmailByUserIdCommand : IRequest<string>
{
    public string? UserId { get; }
    public GetUserEmailByUserIdCommand(string userId)
    {
        UserId = userId;
    }

    public class GetUserEmailByUserIdCommandHandler : CommandHandlerBase<GetUserEmailByUserIdCommand, string>
    {
        private readonly MRUserManager _MRUserManager;

        public GetUserEmailByUserIdCommandHandler(MRUserManager MRUserManager, IApplicationDbContext context, ILogger<GetUserEmailByUserIdCommand> logger) : base(context, logger)
        {
            _MRUserManager = MRUserManager;
        }

        public override async Task<string> Handle(GetUserEmailByUserIdCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new ArgumentNullException(nameof(request.UserId));
            }

            var usr = await _MRUserManager.FindByIdAsync(request.UserId);
            if (usr == null)
            {
                throw new ApplicationException("Cannot find the proper user");
            }

            return string.IsNullOrEmpty(usr.Email) ? "User don't have email" : usr.Email;
        }
    }
}
