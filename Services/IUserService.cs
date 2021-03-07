using System.Collections.Generic;
using System.Threading.Tasks;
using ServerlessDotnetApi.Models;

namespace ServerlessDotnetApi.Services
{
    public interface IUserService
    {
        Task<UserResponse> Authenticate(string username, string password);
        Task<List<UserResponse>> GetAll();
        Task<UserResponse> GetByUsername(string username);
        Task<UserResponse> Create(UserRegisterRequest user, string password);
        Task Update(UserRegisterRequest user, string password = null);
        Task Delete(string username);
    }
}