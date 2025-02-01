using Microsoft.AspNetCore.Mvc;
using ToDoList.Models.DTOs;
using ToDoList.Models.DTOs.Request;
using ToDoList.Models.EFModel;
using ToDoList.Models.Helpers.Interface;
using ToDoList.Models.Service.Interface;

namespace ToDoList.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly ToDoListContext _context;
    private readonly IJwtHelpers _jwtHelpers;
    private readonly IEncryptionService _encrypt;

    public UserController(IJwtHelpers jwtHelpers, ToDoListContext context, IEncryptionService encrypt)
    {
        _context = context;
        _jwtHelpers = jwtHelpers;
        _encrypt = encrypt;
    }

    [HttpPost, Route("Register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    public IActionResult Register(RegisterDto registerDto)
    {
        //check if email exists
        if (_context.Account.Any(x => x.Email == registerDto.Email))
        {
            return BadRequest("Email already exists");
        }

        _context.Add(new Account
        {
            Email = registerDto.Email,
            PasswordHash = _encrypt.ComputeSHA256(registerDto.Password),
            UserName = registerDto.UserName,
            CreatedAt = DateTime.Now
        });
        _context.SaveChanges();

        return Ok();

    }

    /// <summary>
    /// 登入取得 Token
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    [HttpPost, Route("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Login(LoginDto login)
    {
        //hash password
        string hashPassword = _encrypt.ComputeSHA256(login.Password);

        //check if email and password match
        var account = _context.Account.FirstOrDefault(x => x.Email == login.Email && x.PasswordHash == hashPassword);

        if (account == null)
        {
            return Unauthorized();
        }

        var token = await _jwtHelpers.GenerateToken(account);

        return Ok(token);
    }

    [HttpPost, Route("RefreshToken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> RefreshToken(TokenRequest tokenRequest)
    {
        var result = await _jwtHelpers.VerifyAndGenerateToken(tokenRequest);

        if (result == null)
        {
            return BadRequest();
        }

        return Ok(result);
    }
}