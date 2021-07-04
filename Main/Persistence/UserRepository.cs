using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.Json;
using System.Linq;

namespace Main.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly DynamoDBContext _context;

        public UserRepository(IAmazonDynamoDB dynamoDbClient)
        {
            if (dynamoDbClient == null) throw new ArgumentNullException(nameof(dynamoDbClient));
            _context = new DynamoDBContext(dynamoDbClient);
        }

        public async Task<IEnumerable<UserItem>> GetAllAsync()
        {
            return await _context.ScanAsync<UserItem>(new List<ScanCondition>()).GetRemainingAsync();
        }
        public async Task<UserItem> GetByEmail(string email)
        {
            return await _context.LoadAsync<UserItem>(email);
        }
        public async Task<UserItem> Create(UserItem user)
        {
            await _context.SaveAsync<UserItem>(user);
            return user;
        }
        public async Task Update(UserItem userParam)
        {
            await _context.SaveAsync<UserItem>(userParam);
            return;
        }
        public async Task Delete(string email)
        {
            await _context.DeleteAsync<UserItem>(email);
            return;
        }
    }
}