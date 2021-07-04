using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using System.Text;
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
            if (string.IsNullOrEmpty(userRequest.Email) || 
                string.IsNullOrEmpty(userRequest.Password))
                throw new Exception("Username and password are required");

            var user = await _userRepository.GetByEmail(userRequest.Email);

            // check if username exists
            if (user == null)
                return BadRequest(new { message = "Email or password is incorrect" });

            // check if password is correct
            if (!VerifyPasswordHash(userRequest.Password, user.PasswordHash, user.PasswordSalt))
                return BadRequest(new { message = "Email or password is incorrect" });

            // return basic user info and authentication token
            return Ok( new UserLoginResponse {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = CreateToken(user),
                Email = user.Email
            });
        }

        [AllowAnonymous]
        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody]UserRegisterRequest userRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(userRequest.Email) || 
                    string.IsNullOrEmpty(userRequest.Password))
                    throw new Exception("Username and password are required");

                if (null != await _userRepository.GetByEmail(userRequest.Email))
                    throw new Exception("Username \"" + userRequest.Email + "\" is already taken");

                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(userRequest.Password, out passwordHash, out passwordSalt);

                var item = await _userRepository.Create(new UserItem(){
                    FirstName =  userRequest.FirstName,
                    LastName =  userRequest.LastName,
                    Email =  userRequest.Email,
                    PasswordHash =  passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = Role.User
                });

                return Ok(new UserRegisterResponse{
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Role = item.Role,
                    Token = CreateToken(item),
                    Email = item.Email
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
            var tokenUser =  await _userRepository.GetByEmail(User.Identity.Name); 
            if (tokenUser == null)
                return NotFound();

            // Is that user an admin?
            if (Role.Admin != await GetUserRole(User.Identity.Name))
                return Forbid();

            var users = await _userRepository.GetAllAsync();

            List<UserLoginResponse> result = new List<UserLoginResponse>();
            foreach(var user in users)
            {
                var userResponse = new UserLoginResponse {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role
                };
                result.Add(userResponse);
            }

            return Ok(result);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            // Does the user in the token still exist in db?
            var tokenUser =  await _userRepository.GetByEmail(User.Identity.Name);

            if (tokenUser == null)
                return NotFound();
            
            // Is the requested user in the db?
            var user = await _userRepository.GetByEmail(email);

            if (user == null)
                return NotFound();

            // only allow admins to access other user records
            if (email != User.Identity.Name && 
                Role.Admin != await GetUserRole(User.Identity.Name ))
                return Forbid();

            return Ok(new UserLoginResponse{
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            });
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> Update([FromBody]UserRegisterRequest userRequest)
        {
            try
            {
                // Does the user in the token still exist in db?
                var tokenUser =  await _userRepository.GetByEmail(User.Identity.Name);
                if (tokenUser == null)
                    return NotFound();

                // only allow admins to update other user records
                if (userRequest.Email != User.Identity.Name && 
                    Role.Admin != await GetUserRole(User.Identity.Name))
                    return Forbid();

                // update email if it has changed
                if (!string.IsNullOrWhiteSpace(userRequest.Email) &&
                     userRequest.Email != User.Identity.Name)
                {
                    // throw error if the new email is already taken
                    if (null != await _userRepository.GetByEmail(userRequest.Email))
                        throw new Exception("Email " + userRequest.Email + " is already taken");

                    if(Role.Admin == await GetUserRole(userRequest.Email))
                        throw new Exception("Only admins can modify email"); // Avoid normal user trying to become admin by renaming himself
                    tokenUser.Email = userRequest.Email;
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

        [HttpDelete("{email}")]
        public async Task<IActionResult> Delete(string email)
        {
            // Does the user in the token still exist in db?
            var tokenUser =  await _userRepository.GetByEmail(User.Identity.Name);
            if (tokenUser == null)
                return NotFound();
            
            // Is the requested user in the db?
            var reqUser =  await _userRepository.GetByEmail(email);
            if (reqUser == null)
                return NotFound();

            // only allow admins to delete other user records
            if (email != User.Identity.Name && 
                Role.Admin != await GetUserRole(tokenUser.Email))
                return Forbid();

            // delete user 
            await _userRepository.Delete(email);
            return Ok();
        }

        // private helper methods
        private async Task<Role> GetUserRole(string email)
        {
            var user =  await _userRepository.GetByEmail(email);
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

        private static string CreateToken(UserItem user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(System.Environment.GetEnvironmentVariable("JWT_SECRET"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Email.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}