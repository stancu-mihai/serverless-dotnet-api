using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.Persistence
{
    public interface IUserRepository
    {
        Task<UserItem> GetByID(string guid);
        Task<IEnumerable<UserItem>> GetAllAsync();
        Task<UserItem> Create(UserItem user);
        Task Update(UserItem userParam);
        Task Delete(string username);
    }
}