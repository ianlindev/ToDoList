using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models.DTOs
{
    public class TodoCreateDto
    {
        [Required]
        public string Task { get; set; } = string.Empty;
    }
}
