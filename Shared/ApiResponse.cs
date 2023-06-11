namespace MR.Service;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public string ResponseTime { get; set; } = DateTime.Now.ToString("HH:mm:ss");

    public ApiResponse()
    {
        Success = true;
        StatusCode = (int)HttpStatusCode.OK;
    }

    public ApiResponse(T data)
    {
        Success = true;
        Data = data;
        StatusCode = (int)HttpStatusCode.OK;
    }

    public ApiResponse(string message, int statusCode)
    {
        Success = false;
        Message = message;
        StatusCode = statusCode;
    }

    public ApiResponse(List<string> errors, int statusCode)
    {
        Success = false;
        Errors = errors;
        StatusCode = statusCode;
    }

    public static ApiResponse<T> CreatedApiResponse(T id)
    {
        return new ApiResponse<T>(id) { Message = "Created", StatusCode = (int)HttpStatusCode.Created, Success = true };
    }

    public static ApiResponse<T> BadRequestApiResponse(T id)
    {
        return new ApiResponse<T>(id) { Message = "Something wen wrong", StatusCode = (int)HttpStatusCode.BadRequest, Success = false };
    }
}
