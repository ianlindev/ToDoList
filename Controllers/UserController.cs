using Microsoft.AspNetCore.Mvc;
using ToDoList.Models.DTOs;
using ToDoList.Models.Helpers;

namespace ToDoList.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly JwtHelpers _jwtHelpers;

    public UserController(JwtHelpers jwtHelpers)
    {
        _jwtHelpers = jwtHelpers;
    }

    /// <summary>
    /// Login POST: api/Login
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesDefaultResponseType]
    public IActionResult Login(LoginDto login)
    {
        if (login.Email != "ianlin" || login.Password != "123456")
        {
            return Unauthorized();
        }

        var token = _jwtHelpers.GenerateToken(login.Email);

        return Ok(token);
    }
}