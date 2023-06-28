namespace MR.Service.RequestHandling;

public abstract class CommandQueryHandlerBase<TCommand, TResult> : CommandHandlerBase<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    protected CommandQueryHandlerBase(IApplicationDbContext context, ILogger<TCommand> logger) : base(context, logger)
    {
    }

    protected virtual IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortColumn, SortOrder sortOrder)
    {
        if (string.IsNullOrEmpty(sortColumn))
            return query;

        var type = typeof(T);
        var applicationUserProperty = type.GetProperty("ApplicationUser");
        var emailProperty = applicationUserProperty?.PropertyType.GetProperty("Email");

        if (sortColumn == "ApplicationUserEmail" && emailProperty != null)
        {
            var parameterExpression = Expression.Parameter(type, "p");
            var memberExpression = Expression.Property(Expression.Property(parameterExpression, applicationUserProperty), emailProperty);
            var lambdaExpression = Expression.Lambda<Func<T, string>>(memberExpression, parameterExpression);

            return sortOrder == SortOrder.Ascending ? query.OrderBy(lambdaExpression) : query.OrderByDescending(lambdaExpression);
        }

        return sortOrder == SortOrder.Ascending ? query.OrderBy(sortColumn) : query.OrderByDescending(sortColumn);
    }

    protected virtual IQueryable<T> ApplyUserFilter<T>(IQueryable<T> query, QueryBase queryBase)
    {
        if (queryBase is null)
        {
            throw new ArgumentNullException(nameof(queryBase));
        }

        var type = typeof(T);
        var userIdProperty = type.GetProperty("ApplicationUserId");
        var applicationUserProperty = type.GetProperty("ApplicationUser");
        var emailProperty = applicationUserProperty?.PropertyType.GetProperty("Email");

        if (userIdProperty != null && !string.IsNullOrEmpty(queryBase.ApplicationUserId))
        {
            var parameterExpression = Expression.Parameter(type, "x");
            var userIdExpression = Expression.Property(parameterExpression, userIdProperty);
            var valueExpression = Expression.Constant(queryBase.ApplicationUserId, typeof(string));
            var equalExpression = Expression.Equal(userIdExpression, valueExpression);
            var lambdaExpression = Expression.Lambda<Func<T, bool>>(equalExpression, parameterExpression);
            query = query.Where(lambdaExpression);
        }

        if (emailProperty != null && !string.IsNullOrEmpty(queryBase.ApplicationUserEmail))
        {
            var parameterExpression = Expression.Parameter(type, "x");
            var memberExpression = Expression.Property(parameterExpression, applicationUserProperty);
            var emailExpression = Expression.Property(memberExpression, emailProperty);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var valueExpression = Expression.Constant(queryBase.ApplicationUserEmail, typeof(string));
            var containsExpression = Expression.Call(emailExpression, containsMethod, valueExpression);
            var emailFilterExpression = Expression.AndAlso(
                Expression.NotEqual(emailExpression, Expression.Constant(null)),
                containsExpression);
            var lambdaExpression = Expression.Lambda<Func<T, bool>>(emailFilterExpression, parameterExpression);
            query = query.Where(lambdaExpression);
        }

        return query;
    }
}