using System.Collections.Generic;
using System.Threading.Tasks;
using ServerlessDotnetApi.Models;

namespace ServerlessDotnetApi.Services
{
    public interface IUserService
    {
        Task<UserResponse> Authenticate(string username, string password);
        Task<List<UserResponse>> GetAll();
        Task<UserResponse> GetById(int id);
        Task<UserResponse> Create(UserRequest user, string password);
        Task Update(UserResponse user, string password = null); //Receives UserResponse due to id
        Task Delete(int id);
    }
}