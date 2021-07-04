using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.Persistence
{
    public interface ITodoRepository
    {
        public Task AddAsync(TodoItem reviewItem);

        Task<IEnumerable<TodoItem>> GetAllAsync();
        Task<IEnumerable<TodoItem>> GetUserReviewsAsync(int userId);
        Task<TodoItem> GetReviewAsync(int userId, string productName);
    }
}