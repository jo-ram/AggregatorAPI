using System.Text.Json.Serialization;

namespace Aggregator.Service.Models;

public class Info
{
    public string? Code { get; set; }
    public string? Message { get; set; }

    [JsonIgnore]
    public Exception? Exception { get; set; }
}
public class Result<T>
{
    public T? Data { get; set; }
    public Info? Info { get; set; }
    public bool Success { get; set; }
    public int Code { get; set; }

    [JsonIgnore]
    public bool HasException => Info?.Exception is not null;

    public static Result<T> ActionSuccessful(T data, int code, Info info = null)
    {
        return new Result<T>
        {
            Data = data,
            Success = true,
            Code = code,
            Info = new Info
            {
                Code = info?.Code ?? code.ToString(),
                Message = info?.Message ?? string.Empty,
                Exception = default
            }
        };
    }

    public static Result<T> ActionFailed(T data, int code, Info error = null)
    {
        return new Result<T>
        {
            Data = data,
            Success = false,
            Info = error,
            Code = code
        };
    }

    public static Result<T> Exception(int code, Exception ex)
    {
        return new Result<T>
        {
            Data = default(T),
            Success = false,
            Info = new Info
            {
                Exception = ex
            },
            Code = code
        };
    }

}