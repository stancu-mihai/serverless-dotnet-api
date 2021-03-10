using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ServerlessDotnetApi.Services;
using ServerlessDotnetApi.Persistence;
using ServerlessDotnetApi.Models;
using ServerlessDotnetApi.Helpers;
using System.Threading.Tasks;

namespace ServerlessDotnetApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        private readonly AppSettings _appSettings;

        public UsersController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("auth/authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]UserLoginRequest model)
        {
            var user = await _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            return Ok(new
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody]UserRegisterRequest user)
        {
            try
            {
                // create user
                await _userService.Create(user, user.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Does the user in the token still exist in db?
            var tokenUser =  await _userService.GetByUsername(User.Identity.Name);
            if (tokenUser == null)
                return NotFound();

            // Is that user an admin?
            if (Role.Admin != await _userService.GetUserRole(User.Identity.Name))
                return Forbid();

            return Ok(await _userService.GetAll());
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            // Does the user in the token still exist in db?
            var tokenUser =  await _userService.GetByUsername(User.Identity.Name);

            if (tokenUser == null)
                return NotFound();
            
            // Is the requested user in the db?
            var user = await _userService.GetByUsername(username);

            if (user == null)
                return NotFound();

            // only allow admins to access other user records
            if (username != User.Identity.Name && 
                Role.Admin != await _userService.GetUserRole(username))
                return Forbid();

            return Ok(user);
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> Update(string username, [FromBody]UserRegisterRequest user)
        {
            try
            {
                // Does the user in the token still exist in db?
                var tokenUser =  await _userService.GetByUsername(User.Identity.Name);
                if (tokenUser == null)
                    return NotFound();

                // Is the requested user in the db?
                var reqUser =  await _userService.GetByUsername(username);
                if (reqUser == null)
                    return NotFound();

                // only allow admins to update other user records
                if (username != User.Identity.Name && 
                    Role.Admin != await _userService.GetUserRole(username))
                    return Forbid();

                // update user 
                await _userService.Update(user, user.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            // Does the user in the token still exist in db?
            var tokenUser =  await _userService.GetByUsername(User.Identity.Name);
            if (tokenUser == null)
                return NotFound();
            
            // Is the requested user in the db?
            var reqUser =  await _userService.GetByUsername(username);
            if (reqUser == null)
                return NotFound();

            // only allow admins to delete other user records
            if (username != User.Identity.Name && 
                Role.Admin != await _userService.GetUserRole(username))
                return Forbid();

            // delete user 
            await _userService.Delete(username);
            return Ok();
        }
    }
}