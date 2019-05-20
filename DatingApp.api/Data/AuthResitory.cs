using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DatingApp.api.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DatingApp.api.Data
{
	public class AuthResitory : IAuthRepository
	{
		private readonly DataContext _context;
		public AuthResitory(DataContext context)
		{
			_context = context;

		}
		public async Task<User> Login(string userName, string password)
		{
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null) 
                return null;
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;
            return user;
		}

		private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
		{
            using(var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                for(int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
		}

		public async Task<User> Register(User user, string password)
		{
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            
            return user;
		}

		private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		{
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
		}

		public async Task<bool> UserExists(string userName)
		{
            return await _context.Users.AnyAsync(x => x.UserName == userName);
		}
	}
}