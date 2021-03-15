using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.Persistence
{
    public interface IUserRepository
    {
        Task<UserItem> GetByUsername(string username);
        Task<IEnumerable<UserItem>> GetAllAsync();
        Task<UserItem> Create(UserItem user);
        Task Update(UserItem userParam);
        Task Delete(string username);
    }
}