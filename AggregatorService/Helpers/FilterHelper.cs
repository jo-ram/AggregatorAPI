using System.Linq.Expressions;
using System.Reflection;

namespace AggregatorAPI.Helpers;

public class FilterHelper
{

    public static Func<T, bool> ParseFilter<T>(string filter)
    {
        if (string.IsNullOrEmpty(filter)) return null;

        var filterParts = filter.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

        if (filterParts.Length != 3) return null;  

        var propertyName = filterParts[0];
        var operatorSymbol = filterParts[1];
        var value = filterParts[2].Trim('\'');

        if (propertyName is null || operatorSymbol is null || value is null) return default;

        var property = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null) return null;

        var parameter = Expression.Parameter(typeof(T), "x");

        var propertyAccess = Expression.Property(parameter, property);

        var typedValue = Convert.ChangeType(value, property.PropertyType);

        Expression comparison = operatorSymbol.ToLower() switch
        {
            "eq" => Expression.Equal(propertyAccess, Expression.Constant(typedValue)), 
            "neq" => Expression.NotEqual(propertyAccess, Expression.Constant(typedValue)), 
            "contains" => Expression.Call(
                propertyAccess, 
                typeof(string).GetMethod("Contains", new[] { typeof(string) }), 
                Expression.Constant(typedValue)
            ),
            _ => default 
        };

        if (comparison == null) return default;
        var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
        return lambda.Compile();

        
    }
}
