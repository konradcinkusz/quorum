namespace MR.Shared;

public abstract class SearchParamsDTO
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    public string SortColumn { get; set; } = "CreatedAt";

    /// <summary>
    ///  clear only the properties of the child classes and not the properties inherited from the base class
    /// </summary>
    public virtual void Clear()
    {
        var properties = GetType().GetTypeInfo().DeclaredProperties;

        foreach (var property in properties)
        {
            if (property.CanWrite && property.PropertyType.IsValueType)
            {
                property.SetValue(this, Activator.CreateInstance(property.PropertyType));
            }
        }
    }

    /// <summary>
    /// clear only the nullable properties of the child classes
    /// </summary>
    public virtual void ClearOnlyNullable()
    {
        var properties = GetType().GetTypeInfo().DeclaredProperties;

        foreach (var property in properties)
        {
            if (property.CanWrite && property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                property.SetValue(this, null);
            }
        }
    }
}
