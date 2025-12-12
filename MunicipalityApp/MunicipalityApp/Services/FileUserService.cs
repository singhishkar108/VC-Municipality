using MunicipalityApp.Helpers;
using MunicipalityApp.Models;
using System.Text.Json;

namespace MunicipalityApp.Services
{
    public class FileUserService
    {
        private readonly string _filePath;

        public FileUserService()
        {
            _filePath = AppDataHelper.GetFilePath("users.json");
            AppDataHelper.EnsureJsonFile("users.json");
        }

        private List<User> LoadUsers()
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public bool Register(string username, string password)
        {
            var users = LoadUsers();
            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return false;

            string salt = PasswordHelper.GenerateSalt();
            string hashedPassword = PasswordHelper.HashPassword(password, salt);

            users.Add(new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                Salt = salt,
                Role = "User"
            });

            SaveUsers(users);
            return true;
        }

        public User? Login(string username, string password)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null) return null;

            string hashedInput = PasswordHelper.HashPassword(password, user.Salt);
            return hashedInput == user.PasswordHash ? user : null;
        }
    }
}
