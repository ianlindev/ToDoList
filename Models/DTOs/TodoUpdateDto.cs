using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models.DTOs
{
    public class TodoUpdateDto
    {
        [Required]
        public string Task { get; set; } = string.Empty;
    }
}
