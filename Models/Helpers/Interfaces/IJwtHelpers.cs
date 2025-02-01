using ToDoList.Models.DTOs.Request;
using ToDoList.Models.DTOs.Response;
using ToDoList.Models.EFModel;

namespace ToDoList.Models.Helpers.Interface;

public interface IJwtHelpers
{
    /// <summary>
    /// 產生 JWT Token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<AuthResult> GenerateToken(Account user);

    /// <summary>
    /// 驗證並產生 JWT Token
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <returns></returns>
    Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest);
}