using System.Collections.Generic;
using System.Threading.Tasks;
using ServerlessDotnetApi.Models;

namespace ServerlessDotnetApi.Services
{
    public interface IProductReviewService
    {
        Task AddAsync(int userId, ProductReviewRequest request);

        Task<IEnumerable<ProductReviewResponse>> GetAllReviewsAsync();
        Task<IEnumerable<ProductReviewResponse>> GetUserReviewsAsync(int userId);
        Task<ProductReviewResponse> GetReviewAsync(int userId, string productName);
    }
}