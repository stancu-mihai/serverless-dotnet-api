using System;
using Amazon.DynamoDBv2.DataModel;
using serverless_dotnet_api.Models;

namespace serverless_dotnet_api.Persistence
{
    [DynamoDBTable("ProductReview")]
    public class ProductReviewItem
    {
        [DynamoDBHashKey] public int UserId { get; set; }

        [DynamoDBRangeKey] public string ProductName { get; set; }

        public StarRank Rank { get; set; }
        public string Review { get; set; }
        public DateTime ReviewOn { get; set; }
    }
}