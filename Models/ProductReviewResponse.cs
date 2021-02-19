using System;

namespace serverless_dotnet_api.Models
{
    public class ProductReviewResponse
    {
        public string ProductName { get; set; }
        public StarRank Rank { get; set; }
        public string Review { get; set; }
        public DateTime ReviewOn { get; set; }
        public int UserId { get; set; }
    }
}