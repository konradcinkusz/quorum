using Quorum.Service.UserManagement;

namespace Quorum.Service.Features.AdminFeatures;

public class GetUserEmailByUserIdCommand : IRequest<string>
{
    public string? UserId { get; }
    public GetUserEmailByUserIdCommand(string userId)
    {
        UserId = userId;
    }

    public class GetUserEmailByUserIdCommandHandler : CommandHandlerBase<GetUserEmailByUserIdCommand, string>
    {
        private readonly IQuorumUserService _users;

        public GetUserEmailByUserIdCommandHandler(IQuorumUserService users, IApplicationDbContext context, ILogger<GetUserEmailByUserIdCommand> logger) : base(context, logger)
        {
            _users = users;
        }

        public override async Task<string> Handle(GetUserEmailByUserIdCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new ArgumentNullException(nameof(request.UserId));
            }

            var email = await _users.GetEmailAsync(request.UserId, cancellationToken);
            if (email is null)
            {
                // MR has never seen this user. Under the old model this meant "no such user",
                // because MR owned the user table; it now means "not known to MR", which is a
                // different and less alarming thing — the account may exist in the identity
                // service and simply never have signed in here.
                throw new NotFoundException(nameof(QuorumUser), request.UserId);
            }

            return string.IsNullOrEmpty(email) ? "User don't have email" : email;
        }
    }
}
