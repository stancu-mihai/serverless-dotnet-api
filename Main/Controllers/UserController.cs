using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using System.Text;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Main.Persistence;
using Main.Models;
using System.Threading.Tasks;

namespace Main.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [AllowAnonymous]
        [HttpPost("auth/authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]UserLoginRequest userRequest)
        {
            if (string.IsNullOrEmpty(userRequest.Username) || 
                string.IsNullOrEmpty(userRequest.Password))
                throw new Exception("Username and password are required");

            var users = await _userRepository.GetAllAsync();
            var user = users.Where(user => user.Username == userRequest.Username).First();

            // check if username exists
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            // check if password is correct
            if (!VerifyPasswordHash(userRequest.Password, user.PasswordHash, user.PasswordSalt))
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(System.Environment.GetEnvironmentVariable("JWT_SECRET"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            return Ok(new
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = tokenString,
                Username = user.Username,
                Id = user.Id
            });
        }

        [AllowAnonymous]
        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody]UserRegisterRequest userRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(userRequest.Username) || 
                    string.IsNullOrEmpty(userRequest.Password))
                    throw new Exception("Username and password are required");

                UserItem user = null;
                var users = await _userRepository.GetAllAsync();
                if(0 < users.Count())
                    user = users.Where(user => user.Username == userRequest.Username).First();

                if (null != user)
                    throw new Exception("Username \"" + userRequest.Username + "\" is already taken");

                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(userRequest.Password, out passwordHash, out passwordSalt);

                var newUserItem = new UserItem();
                newUserItem.Id = System.Guid.NewGuid().ToString();
                newUserItem.FirstName =  userRequest.FirstName;
                newUserItem.LastName =  userRequest.LastName;
                newUserItem.Username =  userRequest.Username;
                newUserItem.PasswordHash =  passwordHash;
                newUserItem.PasswordSalt = passwordSalt;
                newUserItem.Role = Role.User; 

                var item = await _userRepository.Create(newUserItem);

                return Ok(new
                {
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Role = item.Role,
                    Username = item.Username,
                    Id = item.Id
                });
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
            var tokenUser =  await _userRepository.GetByID(User.Identity.Name); 
            if (tokenUser == null)
                return NotFound();

            // Is that user an admin?
            if (Role.Admin != await GetUserRole(User.Identity.Name))
                return Forbid();

            var users = await _userRepository.GetAllAsync();

            List<UserResponse> result = new List<UserResponse>();
            foreach(var user in users)
            {
                var userResponse = new UserResponse
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    Role = user.Role
                };
                result.Add(userResponse);
            }

            return Ok(result);
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetByUsername(string guid)
        {
            // Does the user in the token still exist in db?
            var tokenUser =  await _userRepository.GetByID(User.Identity.Name);

            if (tokenUser == null)
                return NotFound();
            
            // Is the requested user in the db?
            var user = await _userRepository.GetByID(guid);

            if (user == null)
                return NotFound();

            // only allow admins to access other user records
            if (guid != User.Identity.Name && 
                Role.Admin != await GetUserRole(User.Identity.Name ))
                return Forbid();

            return Ok(new{
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Role = user.Role,
                Id = user.Id
            });
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> Update([FromBody]UserRegisterRequest userRequest, string guid)
        {
            try
            {
                // Does the user in the token still exist in db?
                var tokenUser =  await _userRepository.GetByID(User.Identity.Name);
                if (tokenUser == null)
                    return NotFound();

                // only allow admins to update other user records
                if (guid != User.Identity.Name && 
                    Role.Admin != await GetUserRole(User.Identity.Name))
                    return Forbid();

                // update username if it has changed
                if (!string.IsNullOrWhiteSpace(userRequest.Username) &&
                     userRequest.Username != tokenUser.Username)
                {
                    // throw error if the new username is already taken
                    var users = await _userRepository.GetAllAsync();
                    var usersWithSameUsername = users.Where(user => user.Username == userRequest.Username);
                    if (usersWithSameUsername.Count() > 0)
                        throw new Exception("Username " + userRequest.Username + " is already taken");

                    tokenUser.Username = userRequest.Username;
                }

                // update user properties if provided
                if (!string.IsNullOrWhiteSpace(userRequest.FirstName))
                    tokenUser.FirstName = userRequest.FirstName;

                if (!string.IsNullOrWhiteSpace(userRequest.LastName))
                    tokenUser.LastName = userRequest.LastName;

                // update password if provided
                if (!string.IsNullOrWhiteSpace(userRequest.Password))
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(userRequest.Password, out passwordHash, out passwordSalt);

                    tokenUser.PasswordHash = passwordHash;
                    tokenUser.PasswordSalt = passwordSalt;
                }

                // update user 
                await _userRepository.Update(tokenUser);

                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            // Does the user in the token still exist in db?
            var tokenUser =  await _userRepository.GetByID(User.Identity.Name);
            if (tokenUser == null)
                return NotFound();
            
            // Is the requested user in the db?
            var reqUser =  await _userRepository.GetByID(guid);
            if (reqUser == null)
                return NotFound();

            // only allow admins to delete other user records
            if (guid != User.Identity.Name && 
                Role.Admin != await GetUserRole(tokenUser.Username))
                return Forbid();

            // delete user 
            await _userRepository.Delete(guid);
            return Ok();
        }

        // private helper methods
        private async Task<Role> GetUserRole(string guid)
        {
            var user =  await _userRepository.GetByID(guid);
            if (user == null)
                throw new Exception("User not found");
                
            return user.Role;
        }
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}