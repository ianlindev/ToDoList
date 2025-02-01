namespace ToDoList.Models.DTOs.Response;

public class AuthResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
