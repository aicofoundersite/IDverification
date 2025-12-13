using System;
using System.Security.Cryptography;
using System.Text;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;

namespace CrossSetaWeb.Services
{
    public class UserService : IUserService
    {
        private readonly IDatabaseHelper _dbHelper;

        public UserService(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public void RegisterUser(UserModel user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username and Password are required.");
            }

            user.PasswordHash = HashPassword(password);
            _dbHelper.InsertUser(user);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
