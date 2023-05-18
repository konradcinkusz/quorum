using FluentValidation;
using Microsoft.VisualBasic;
using MR.Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MR.Service.Features.Issues;

public class CreateIssueCommand : IRequest<Guid>
{
    //nullable
    public string ApplicationUserId { get; set; }

    //not null
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    //bazujac na tym statusie ustawiamy widocznosc
    public IssueStatus IssueStatus { get; set; } = IssueStatus.NotVisible;

    public class CreateIssueCommandHandler : CreateCommandHandlerBase<CreateIssueCommand, Guid, Issue>
    {
        public CreateIssueCommandHandler(
            IApplicationDbContext context,
            ILogger<CreateIssueCommand> logger) : base(context, logger)
        {
        }

        protected override async Task<Issue> MakeAsync(CreateIssueCommand command, CancellationToken cancellationToken)
        {
            return new Issue
            {
                CreatedById = command.ApplicationUserId,
                Name = command.Title,
                Question = command.Question,
                IsVerifyByAdmin = command.IsVerifyByAdmin,
                IssueStatus = command.IssueStatus
            };
        }
    }
}

public abstract class CreateCommandHandlerBase<TCommand, TResult, TCreate> : CommandHandlerBase<TCommand, TResult>
    where TCommand : IRequest<TResult>
    where TCreate : BaseEntity<TResult>
    where TResult : IEquatable<TResult>
{
    public CreateCommandHandlerBase(IApplicationDbContext context, ILogger<TCommand> logger) : base(context, logger)
    {
    }

    public override async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var created = await MakeAsync(request, cancellationToken);

        _ = await _context.Set<TCreate>().AddAsync(created);
        _ = await _context.SaveChangesAsync(cancellationToken);

        return created.Id;
    }

    protected abstract Task<TCreate> MakeAsync(TCommand command, CancellationToken cancellationToken);
}
