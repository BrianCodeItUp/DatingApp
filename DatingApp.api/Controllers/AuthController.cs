using System;
using System.Threading.Tasks;
using DatingApp.api.Data;
using DatingApp.api.Dtos;
using DatingApp.api.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.api.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
    public class AuthController : ControllerBase
    {
		private readonly IAuthRepository _repo;

		public AuthController (IAuthRepository repo)
        {
			_repo = repo;
		}

        [HttpPost("register")]
        public async Task<IActionResult> Register (UserForRegisterDto userForRegistorDto)
        {
            Console.WriteLine("The register action has been` called");
            // validate request
            userForRegistorDto.UserName = userForRegistorDto.UserName.ToLower();

            bool isUserAlreadyExist = await _repo.UserExists(userForRegistorDto.UserName);
            if(isUserAlreadyExist) return BadRequest("Username already exists");

            var userToCreate = new User {
                UserName = userForRegistorDto.UserName
            };
            
            var createdUser = await _repo.Register(userToCreate, userForRegistorDto.Password);
            return StatusCode(201);
        }
    }
}