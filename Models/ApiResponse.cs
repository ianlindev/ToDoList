namespace ToDoList.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
    public object Errors { get; set; }

    public ApiResponse(bool success, string message, object data, object errors)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors;
    }
}