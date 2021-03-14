using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerlessDotnetApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerlessDotnetApi.Persistence;
using System;

namespace ServerlessDotnetApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly ILogger<ProductReviewsController> _logger;
        private readonly IProductReviewRepository _productReviewRepository;

        public ProductReviewsController(ILogger<ProductReviewsController> logger,
            IProductReviewRepository productReviewRepository)
        {
            _logger = logger;
            _productReviewRepository = productReviewRepository ??
                throw new ArgumentNullException(nameof(productReviewRepository));
        }

        [HttpPost]
        [Route("{userId}")]
        public async Task<IActionResult> AddProductReview(int userId, ProductReviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var reviewItem = ToProductReviewItem(userId, request);
            await _productReviewRepository.AddAsync(reviewItem);

            return Created(
                Url.Link("GetUserProductReview", new
                {
                    userId,
                    productName = request.ProductName
                }), null);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductReviews()
        {
            var results = await _productReviewRepository.GetAllAsync();
            var reviews = ToProductReviewResponses(results);
            return ToActionResult(reviews);
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetUserProductReviews(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            var results = await _productReviewRepository.GetUserReviewsAsync(userId);
            var reviews = ToProductReviewResponses(results);
            return ToActionResult(reviews);
        }

        [HttpGet]
        [Route("{userId}/{productName}", Name = "GetUserProductReview")]
        public async Task<IActionResult> GetUserProductReview(int userId, string productName)
        {
            if (productName == null) throw new ArgumentNullException(nameof(productName));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            var result = await _productReviewRepository.GetReviewAsync(userId, productName);
            var review = ToProductReviewResponse(result);
            return result is null ? (IActionResult) NotFound(null) : Ok(review);
        }

        private IActionResult ToActionResult(IEnumerable<ProductReviewResponse> reviews)
        {
            return reviews.Any() ? Ok(reviews) : (IActionResult) NotFound(null);
        }

                private static IEnumerable<ProductReviewResponse> ToProductReviewResponses(
            IEnumerable<ProductReviewItem> results)
        {
            return results?.Select(ToProductReviewResponse);
        }

        private static ProductReviewResponse ToProductReviewResponse(ProductReviewItem item)
        {
            if (item is null)
                return default;

            return new ProductReviewResponse
            {
                ProductName = item.ProductName,
                UserId = item.UserId,
                Rank = item.Rank,
                Review = item.Review,
                ReviewOn = item.ReviewOn
            };
        }

        private static ProductReviewItem ToProductReviewItem(int userId, ProductReviewRequest request)
        {
            return new ProductReviewItem
            {
                UserId = userId,
                ProductName = request.ProductName,
                Rank = request.Rank,
                Review = request.Review,
                ReviewOn = request.ReviewOn ?? DateTime.UtcNow
            };
        }
    }
}