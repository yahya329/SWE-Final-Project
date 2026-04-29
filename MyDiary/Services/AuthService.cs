using System;
using MyDiary.Data;
using MyDiary.Models;

namespace MyDiary.Services
{
    internal class AuthService
    {
        private readonly UserRepostiory _userRepo = new UserRepostiory();

        
        public User Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            User user = _userRepo.FindByUsername(username);

            if (user == null)
                return null; // username doesn't exist

            bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);

            return passwordMatch ? user : null;
        }

        
        public bool Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            User existing = _userRepo.FindByUsername(username);
            if (existing != null)
                return false; // username already exists

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return _userRepo.Register(username, hashedPassword);
        }
    }
}
