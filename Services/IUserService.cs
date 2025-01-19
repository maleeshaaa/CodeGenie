using CodeGenie.Models;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task AddUserAsync(User user);
}
