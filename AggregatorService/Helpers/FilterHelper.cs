using AggregatorAPI.Models;

namespace AggregatorAPI.Helpers;

public class FilterHelper
{

    public static Func<Article, bool> ParseFilter(string filter)
    {
        if (string.IsNullOrEmpty(filter)) return null;

        var filterParts = filter.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

        if (filterParts.Length != 3) return null;  

        var field = filterParts[0];
        var operatorSymbol = filterParts[1];
        var value = filterParts[2].Trim('\'');

        switch (field.ToLower())
        {
            case "author":
                if (operatorSymbol == "eq")
                    return article => string.Equals(article.Author, value, StringComparison.OrdinalIgnoreCase);
                break;

            case "title":
                if (operatorSymbol == "eq")
                    return article => string.Equals(article.Title, value, StringComparison.OrdinalIgnoreCase);
                break;
        }

        return null; 
    }
}
