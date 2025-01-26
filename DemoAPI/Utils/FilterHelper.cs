using AggregatorAPI.Models;

namespace AggregatorAPI.Utils;

public class FilterHelper
{

    public static Func<Article, bool> ParseFilter(string filter)
    {
        if (string.IsNullOrEmpty(filter)) return null;

        // Trim the filter and handle cases where the filter contains extra spaces
        var filterParts = filter.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

        if (filterParts.Length != 3) return null;  // Ensure it splits into exactly 3 parts

        var field = filterParts[0];
        var operatorSymbol = filterParts[1];
        var value = filterParts[2].Trim('\'');

        switch (field.ToLower())
        {
            case "author":
                if (operatorSymbol == "eq")
                    return article => string.Equals(article.Author, value, StringComparison.OrdinalIgnoreCase);
                break;

            // You can extend this for other fields like title, description, etc.
            case "title":
                if (operatorSymbol == "eq")
                    return article => string.Equals(article.Title, value, StringComparison.OrdinalIgnoreCase);
                break;

                // Handle other fields or operators if needed
        }

        return null; // Return null if no valid filter
    }


    //public static Func<Article, bool> ParseFilter(string filter)
    //{
    //    if (string.IsNullOrEmpty(filter)) return null;

    //    var filterParts = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    //    if (filterParts.Length != 3) return null;

    //    var field = filterParts[0];
    //    var operatorSymbol = filterParts[1];
    //    var value = filterParts[2].Trim('\'');

    //    switch (field.ToLower())
    //    {
    //        case "author":
    //            if (operatorSymbol == "eq")
    //                return article => article.Author.Equals(value, StringComparison.OrdinalIgnoreCase);
    //            break;

    //        case "title":
    //            if (operatorSymbol == "eq")
    //                return article => article.Title.Equals(value, StringComparison.OrdinalIgnoreCase);
    //            break;
    //    }

    //    return null; 
    //}
}
