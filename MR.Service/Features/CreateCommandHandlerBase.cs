namespace MR.Service.Features;

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

    protected virtual Task<TCreate> MakeAsync(TCommand command, CancellationToken cancellationToken)
    {
        Type issueType = typeof(TCreate);

        // Create a new instance of the Issue class using reflection
        //use the null-forgiving operator (!) to indicate that you are certain the values won't be null. 
        TCreate issue = (TCreate)Activator.CreateInstance(issueType)!;

        // Set the property values using reflection
        foreach (var property in issueType.GetProperties())
        {
            if (property.CanWrite)
            {
                var commandPropertyValue = typeof(TCommand).GetProperty(property.Name)?.GetValue(command);
                property.SetValue(issue, commandPropertyValue!);
            }
        }

        return Task.FromResult(issue);
    }

}
