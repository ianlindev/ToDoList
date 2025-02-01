using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models.DTOs;
using ToDoList.Models.EFModel;

namespace ToDoList.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TodoController : ControllerBase
{
    private readonly ToDoListContext _context;
    private readonly ILogger<TodoController> _logger;

    public TodoController(ToDoListContext context, ILogger<TodoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all todos GET: api/Todo
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoSelectDto>>> GetTodo()
    {
        _logger.LogInformation("Get all todos");
        return Ok(await _context.Todo.Select(x => new TodoSelectDto
        {
            Id = x.Id,
            Task = x.Task,
            CreatedAt = x.CreatedAt,
            IsComplete = x.IsComplete
        }).ToListAsync());
    }

    /// <summary>
    /// Get a todo by id GET: api/Todo/5
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<TodoSelectDto>> GetTodo(int id)
    {
        var todo = await _context.Todo.FindAsync(id);

        if (todo == null)
        {
            return NotFound();
        }

        return new TodoSelectDto
        {
            Id = todo.Id,
            Task = todo.Task,
            CreatedAt = todo.CreatedAt,
            IsComplete = todo.IsComplete
        };
    }

    /// <summary>
    /// Update a todo PUT: api/Todo/1
    /// </summary>
    /// <param name="id"></param>
    /// <param name="todo"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> PutTodo(int id, TodoUpdateDto todo)
    {
        var todoDB = await _context.Todo.FindAsync(id);

        if (todoDB == null)
        {
            return NotFound();
        }

        todoDB.Task = todo.Task;

        _context.Entry(todoDB).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Create a todo POST: api/Todo
    /// </summary>
    /// <param name="todo"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<Todo>> PostTodo(TodoCreateDto todo)
    {
        //mapper to Todo
        var todoCreate = new Todo
        {
            Task = todo.Task
        };

        _context.Todo.Add(todoCreate);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTodo", new { id = todoCreate.Id }, todoCreate);
    }

    /// <summary>
    /// Delete a todo DELETE: api/Todo/1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.Todo.FindAsync(id);
        if (todo == null)
        {
            return NotFound();
        }

        _context.Todo.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}