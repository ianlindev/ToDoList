using System.Security.Cryptography;
using ToDoList.Models.Service.Interface;

namespace ToDoList.Models.Service.Implement;

public class EncryptionService : IEncryptionService
{
    /// <summary>
    /// Compute SHA256 hash
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public string ComputeSHA256(string input)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}