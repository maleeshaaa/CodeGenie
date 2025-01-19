using CodeGenie.Models;

namespace CodeGenie.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task<string> RegisterUserAsync(string username, string email, string password);
        Task<string> LoginUserAsync(string username, string password);
    }
}
