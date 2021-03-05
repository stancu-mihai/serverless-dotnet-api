using System;
using Amazon.DynamoDBv2.DataModel;
using ServerlessDotnetApi.Models;

namespace ServerlessDotnetApi.Persistence
{
    [DynamoDBTable("User")]
    public class UserItem
    {
        [DynamoDBHashKey] public int Id { get; set; }
        [DynamoDBRangeKey] public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}