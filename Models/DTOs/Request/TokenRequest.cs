﻿using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models.DTOs.Request;

public class TokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}