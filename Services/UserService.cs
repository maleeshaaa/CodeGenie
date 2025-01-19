using CodeGenie.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CodeGenie.Services
{
    public class UserService : IUserService
    {
        private readonly Repositories.IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(Repositories.IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        // Add a new user
        public async Task AddUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username cannot be empty");

            await _userRepository.AddUserAsync(user);
        }

        // Register a new user
        public async Task<string> RegisterUserAsync(string username, string email, string password)
        {
            // Check if user already exists by email or username
            var existingUserByEmail = await _userRepository.GetUserByEmailAsync(email);
            if (existingUserByEmail != null)
                return "User with this email already exists.";

            var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(username);
            if (existingUserByUsername != null)
                return "User with this username already exists.";

            // Hash the password
            var passwordHash = HashPassword(password);

            // Create the new user object
            var user = new User
            {
                Username = username,
                Email = email,
                Password = passwordHash
            };

            // Do not manually set the Id, let the database handle it if it's auto-incremented
            await _userRepository.AddUserAsync(user);

            return "User registered successfully.";
        }

        // Login a user
        public async Task<string> LoginUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return "Invalid credentials.";  // Return early if username or password is empty

            // Find the user by username
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null)
                return "Invalid credentials.";  // User not found

            // Verify the password
            if (!VerifyPassword(password, user.Password))
                return "Invalid credentials.";  // Password mismatch

            // Generate JWT token if credentials are valid
            var token = GenerateJwtToken(user);
            return "Bearer " + token;  // Return the token with "Bearer" prefix
        }

        // Hash the password
        private string HashPassword(string password)
        {
            var salt = new byte[16]; // Generate a 16-byte salt
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Store the salt and the hashed password together
            return Convert.ToBase64String(salt) + ":" + hashedPassword;
        }

        // Verify the password
        private bool VerifyPassword(string password, string storedHash)
        {
            // Extract the salt and hash from the stored hash
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var storedPasswordHash = parts[1];

            // Hash the provided password using the same salt
            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Compare the hashes
            return storedPasswordHash == hashedPassword;
        }

        // Generate JWT token
        private string GenerateJwtToken(User user)
        {
            var claims = new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
