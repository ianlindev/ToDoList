using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToDoList.Models.DTOs.Request;
using ToDoList.Models.DTOs.Response;
using ToDoList.Models.EFModel;
using ToDoList.Models.Helpers.Interface;

namespace ToDoList.Models.Helpers.Implement;

public class JwtHelpers : IJwtHelpers
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly TokenValidationParameters _tokenValidationParams;

    public JwtHelpers(IConfiguration configuration, IServiceProvider serviceProvider, TokenValidationParameters tokenValidationParameters)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _tokenValidationParams = tokenValidationParameters;
    }

    /// <summary>
    /// 產生 JWT Token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<AuthResult> GenerateToken(Account user)
    {
        var issuer = _configuration.GetValue<string>("JwtSettings:Issuer");
        var signKey = _configuration.GetValue<string>("JwtSettings:SignKey");

        // Configuring "Claims" to your JWT Token
        var claims = new List<Claim>();

        // In RFC 7519 (Section#4), there are defined 7 built-in Claims, but we mostly use 2 of them.
        //claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.UserName)); // User.Identity.Name
        //claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "The Audience"));
        //claims.Add(new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds().ToString()));
        //claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
        //claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID

        // TODO: You can define your "roles" to your Claims.
        //claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        //claims.Add(new Claim(ClaimTypes.Role, "Users"));

        var userClaimsIdentity = new ClaimsIdentity(claims);

        // Create a SymmetricSecurityKey for JWT Token signatures
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey!));

        // HmacSha256 MUST be larger than 128 bits, so the key can't be too short. At least 16 and more characters. Suggestion is 32 characters.
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        // Create SecurityTokenDescriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            //Audience = issuer, // Sometimes you don't have to define Audience.
            //NotBefore = DateTime.Now, // Default is DateTime.Now
            //IssuedAt = DateTime.Now, // Default is DateTime.Now
            Subject = userClaimsIdentity,
            Expires = DateTime.Now.AddMinutes(30),
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        var refreshToken = new RefreshTokens()
        {
            Token = RandomString(14) + Guid.NewGuid().ToString(),
            UserId = user.Id,
            JwtId = token.Id,
            IsUsed = false,
            IsRevorked = false,
            AddedDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddMonths(6)
        };

        // Save the RefreshToken to the Database
        // can't inject the DbContext directly, so we have to use the ServiceLocator pattern
        var serviceScope = _serviceProvider.CreateScope();
        var _context = serviceScope.ServiceProvider.GetRequiredService<ToDoListContext>();

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResult
        {
            Success = true,
            Token = jwtToken,
            RefreshToken = refreshToken.Token
        };
    }

    /// <summary>
    /// 驗證並產生新的 JWT Token
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <returns></returns>
    public async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // 確認 token 是否為 JWT token，並驗證 token 是否有效(過期ValidateLifetime = true、簽章)
            var tokenInVerification = tokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParams, out var validatedToken);

            // 確認 token 的簽章是否為 HmacSha256
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                if (result == false)
                {
                    return null;
                }
            }
        }
        catch (SecurityTokenExpiredException)
        {
            // Token 已過期，繼續檢查 refresh token
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                Errors = new List<string> { ex.Message }
            };
        }

        // 驗證 token 是否存在於資料庫
        var serviceScope = _serviceProvider.CreateScope();
        var _context = serviceScope.ServiceProvider.GetRequiredService<ToDoListContext>();
        var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

        if (storedRefreshToken == null)
        {
            return new AuthResult()
            {
                Success = false,
                Errors = new List<string>()
                {
                    "Refresh Token 不存在"
                }
            };
        }

        // 驗證資料庫 RefreshToken 是否已過期
        if (DateTime.Now > storedRefreshToken.ExpiryDate)
        {
            return new AuthResult()
            {
                Errors = new List<string>() { "Refresh Token 已過期，請重新登入" },
                Success = false
            };
        }

        // 驗證 RefreshToken 是否已使用
        if (storedRefreshToken.IsUsed)
        {
            return new AuthResult()
            {
                Success = false,
                Errors = new List<string>()
                {
                    "Refresh Token 已使用過!!"
                }
            };
        }

        // 驗證 RefreshToken 是否已撤銷
        if (storedRefreshToken.IsRevorked)
        {
            return new AuthResult()
            {
                Success = false,
                Errors = new List<string>()
                {
                    "Refresh Token 已撤銷!!"
                }
            };
        }

        // 取得JWT Token ID
        var jti = tokenHandler.ReadJwtToken(tokenRequest.Token).Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)!.Value;

        // 驗證 token 是否與資料庫中的 token 相符
        if (storedRefreshToken.JwtId != jti)
        {
            return new AuthResult()
            {
                Success = false,
                Errors = new List<string>()
                {
                    "The token doesn't mateched the saved token"
                }
            };
        }

        // 更新資料庫 RefreshToken，舊的 RefreshToken 立即失效
        storedRefreshToken.IsUsed = true; // 更新 RefreshToken 為已使用
        _context.RefreshTokens.Update(storedRefreshToken);
        await _context.SaveChangesAsync();

        // 取得使用者資料，並產生新的 JWT Token & Refresh Token
        var dbUser = await _context.Account.FindAsync(storedRefreshToken.UserId);
        return await GenerateToken(dbUser!);
    }

    /// <summary>
    /// 產生隨機字串
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    private static string RandomString(int length)
    {
        var reandom = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[reandom.Next(s.Length)]).ToArray());
    }
}