namespace ToDoList.Models.DTOs
{
    public class TodoSelectDto
    {
        public int Id { get; set; }
        public string Task { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsComplete { get; set; }
    }
}
