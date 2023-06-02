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
}
