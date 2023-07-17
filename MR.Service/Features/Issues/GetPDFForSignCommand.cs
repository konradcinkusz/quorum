namespace MR.Service.Features.Issues;

public class GetPDFForSignCommand : IRequest<string>
{
    private readonly Guid _issueId;

    public GetPDFForSignCommand(Guid issueId)
    {
        _issueId = issueId;
    }

    internal class GetPDFForSignCommandHandler : CommandHandlerBase<GetPDFForSignCommand, string>
    {
        public GetPDFForSignCommandHandler(IApplicationDbContext context, ILogger<GetPDFForSignCommand> logger) : base(context, logger)
        {
        }

        public override Task<string> Handle(GetPDFForSignCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}