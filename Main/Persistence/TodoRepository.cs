using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: Amazon.Lambda.Core.LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Main.Persistence
{
    public class TodoRepository : ITodoRepository
    {
        private readonly DynamoDBContext _context;

        public TodoRepository(IAmazonDynamoDB dynamoDbClient)
        {
            if (dynamoDbClient == null) throw new ArgumentNullException(nameof(dynamoDbClient));
            _context = new DynamoDBContext(dynamoDbClient);
        }

        public async Task AddAsync(TodoItem reviewItem)
        {
            await _context.SaveAsync(reviewItem);
        }

        public async Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            return await _context.ScanAsync<TodoItem>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task<IEnumerable<TodoItem>> GetUserReviewsAsync(int userId)
        {
            return await _context.QueryAsync<TodoItem>(userId).GetRemainingAsync();
        }

        public async Task<TodoItem> GetReviewAsync(int userId, string productName)
        {
            return await _context.LoadAsync<TodoItem>(userId, productName);
        }
    }
}