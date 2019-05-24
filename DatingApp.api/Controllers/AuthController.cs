using System;
using System.Threading.Tasks;
using DatingApp.api.Data;
using DatingApp.api.Dtos;
using DatingApp.api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repo;
    private readonly IConfiguration _config;
    
    public AuthController(IAuthRepository repo, IConfiguration config)
    {
       _repo = repo;
       _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForRegisterDto userForRegistorDto)
    {
      userForRegistorDto.UserName = userForRegistorDto.UserName.ToLower();

      bool isUserAlreadyExist = await _repo.UserExists(userForRegistorDto.UserName);
      if (isUserAlreadyExist) return BadRequest("Username already exists");

      var userToCreate = new User
      {
        UserName = userForRegistorDto.UserName
      };

      var createdUser = await _repo.Register(userToCreate, userForRegistorDto.Password);
      return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
      var userFromRepo = await _repo.Login(userForLoginDto.UserName.ToLower(), userForLoginDto.Password);
      if (userForLoginDto == null) return Unauthorized();

      // making claims
      var claims = new[]
      {
        new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
        new Claim(ClaimTypes.Name, userFromRepo.UserName)
      };

      // the key or secret that we set in AppSetting,then inject it into our controller
      var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config.GetSection("AppSetting:Token").Value)
      );

      // encrypte our key with specify algorithm
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      // compose our JWT
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1),
        SigningCredentials = creds
      };

      var tokenHandler = new JwtSecurityTokenHandler();
      var token = tokenHandler.CreateToken(tokenDescriptor);
      
      // OK takes a object
      return Ok(new {
        token = tokenHandler.WriteToken(token)
      });
    }
  }
}
