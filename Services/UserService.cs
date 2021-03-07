using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerlessDotnetApi.Persistence;
using ServerlessDotnetApi.Models;

namespace ServerlessDotnetApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponse> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = await _userRepository.GetByUsername(username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            var result = new UserResponse();     
            result.FirstName =  user.FirstName;
            result.LastName =  user.LastName;
            result.Username =  user.Username;
            result.Password = password;

            // authentication successful
            return result;
        }

        public async Task<List<UserResponse>> GetAll()
        {
            var users = await _userRepository.GetAllAsync();

            List<UserResponse> result = new List<UserResponse>();
            foreach(var user in users)
            {
                var userResponse = new UserResponse();     
                userResponse.FirstName =  user.FirstName;
                userResponse.LastName =  user.LastName;
                userResponse.Username =  user.Username;
                result.Add(userResponse);
            }

            return result;
        }

        public async Task<UserResponse> GetByUsername(string username)
        {
            var user = await _userRepository.GetByUsername(username);
            if(null == user)
                return null;

            var result = new UserResponse();     
            result.FirstName =  user.FirstName;
            result.LastName =  user.LastName;
            result.Username =  user.Username;

            // authentication successful
            return result;
        }        

        public async Task<UserResponse> Create(UserRegisterRequest user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Password is required");

            if (null != await _userRepository.GetByUsername(user.Username))
                throw new Exception("Username \"" + user.Username + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            var newUserItem = new UserItem();
            newUserItem.FirstName =  user.FirstName;
            newUserItem.LastName =  user.LastName;
            newUserItem.Username =  user.Username;
            newUserItem.PasswordHash =  passwordHash;
            newUserItem.PasswordSalt = passwordSalt;

            var item = await _userRepository.Create(newUserItem);

            var newUserResponse = new UserResponse();     
            newUserResponse.FirstName =  user.FirstName;
            newUserResponse.LastName =  user.LastName;
            newUserResponse.Username =  user.Username;
            newUserResponse.Password =  user.Password;

            return newUserResponse;
        }

        public async Task Update(UserResponse userParam, string password = null)
        {
            var user = await _userRepository.GetByUsername(userParam.Username);

            if (user == null)
                throw new Exception("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
            {
                // throw error if the new username is already taken
            if (null != await _userRepository.GetByUsername(user.Username))
                    throw new Exception("Username " + userParam.Username + " is already taken");

                user.Username = userParam.Username;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.FirstName))
                user.FirstName = userParam.FirstName;

            if (!string.IsNullOrWhiteSpace(userParam.LastName))
                user.LastName = userParam.LastName;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            await _userRepository.Update(user);
        }

        public async Task Delete(string username)
        {
            await _userRepository.Delete(username);
            return;
        }

        // private helper methods

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