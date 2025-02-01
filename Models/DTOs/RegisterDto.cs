using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models.DTOs;

public class RegisterDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Compare("Password", ErrorMessage = "密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
    [Required]
    public string UserName { get; set; } = string.Empty;
}