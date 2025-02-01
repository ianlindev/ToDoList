namespace ToDoList.Models.Service.Interface;

public interface IEncryptionService
{
    string ComputeSHA256(string input);
}