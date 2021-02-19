using System;

namespace serverless_dotnet_api.Models
{
    public class ProductReviewRequest
    {
        public string ProductName { get; set; }
        public StarRank Rank { get; set; }
        public string Review { get; set; }
        public DateTime? ReviewOn { get; set; }
    }

    public enum StarRank
    {
        One = 1,
        Two,
        Three,
        Four,
        Five
    }
}