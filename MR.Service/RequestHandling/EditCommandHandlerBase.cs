namespace MR.Service.RequestHandling;

public abstract class EditCommandHandlerBase<TCommand, TResult, TCreate> : CreateCommandHandlerBase<TCommand, TResult, TCreate>
    where TCommand : IRequest<TResult>
    where TCreate : BaseEntity<TResult>
    where TResult : IEquatable<TResult>
{
    public EditCommandHandlerBase(IApplicationDbContext context, ILogger<TCommand> logger) : base(context, logger)
    {
    }

    public override async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await MakeAsync(request, cancellationToken);

        var existingEntity = await _context.Set<TCreate>().FindAsync(entity.Id);
        if (existingEntity == null)
        {
            throw new ArgumentNullException("No existing entity to update!");
        }

        var propertiesChanged = new List<string>();

        var issueType = typeof(TCreate);
        foreach (var property in issueType.GetProperties())
        {
            if (property.CanWrite)
            {
                var commandPropertyValue = typeof(TCommand).GetProperty(property.Name)?.GetValue(request);
                var existingPropertyValue = property.GetValue(existingEntity);

                if (!Equals(commandPropertyValue, existingPropertyValue))
                {
                    if (property.Name == "CreatedAt" && existingPropertyValue != null)
                    {
                        continue; // Skip setting the new value for CreatedAt if it already has a value
                    }
                    property.SetValue(existingEntity, commandPropertyValue);
                    propertiesChanged.Add(property.Name);
                }
            }
        }

        _logger.LogInformation(string.Join(" ", propertiesChanged));

        var sum = await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
