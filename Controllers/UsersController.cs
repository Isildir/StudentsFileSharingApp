using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities.Models;
using StudentsFileSharingApp.Interfaces;
using StudentsFileSharingApp.Utility;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentsFileSharingApp.Controllers
{
    [Authorize, ApiController, Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService userService;
        private readonly AppSettings appSettings;

        public UsersController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            this.userService = userService;
            this.appSettings = appSettings.Value;
        }

        [AllowAnonymous, HttpPost("[action]")]
        public IActionResult Authenticate([FromBody]UserDto userDto)
        {
            User user;

            try
            {
                user = userService.Authenticate(userDto.Login, userDto.Password);
            }
            catch (Exception ex)
            {
                Logger.Log($"{nameof(UsersController)} {nameof(Authenticate)}", ex.Message, NLog.LogLevel.Warn, ex);

                return BadRequest(ex.Message);
            }

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                user.Id,
                user.Login,
                Token = tokenString
            });
        }

        [AllowAnonymous, HttpPost("[action]")]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            var user = new User
            {
                Id = userDto.Id,
                Login = userDto.Login,
                Name = userDto.Name
            };

            try
            {
                userService.Create(user, userDto.Password);

                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Log($"{nameof(UsersController)} {nameof(Register)}", ex.Message, NLog.LogLevel.Warn, ex);

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("[action]/{id:int}")]
        public IActionResult Delete(int id)
        {
            userService.Delete(id);

            return Ok();
        }
    }
}