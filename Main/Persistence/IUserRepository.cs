using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.Persistence
{
    public interface IUserRepository
    {
        Task<UserItem> GetByEmail(string email);
        Task<IEnumerable<UserItem>> GetAllAsync();
        Task<UserItem> Create(UserItem user);
        Task Update(UserItem userParam);
        Task Delete(string email);
    }
}