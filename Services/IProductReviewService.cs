using System.Collections.Generic;
using System.Threading.Tasks;
using serverless_dotnet_api.Models;

namespace serverless_dotnet_api.Services
{
    public interface IProductReviewService
    {
        Task AddAsync(int userId, ProductReviewRequest request);

        Task<IEnumerable<ProductReviewResponse>> GetAllReviewsAsync();
        Task<IEnumerable<ProductReviewResponse>> GetUserReviewsAsync(int userId);
        Task<ProductReviewResponse> GetReviewAsync(int userId, string productName);
    }
}