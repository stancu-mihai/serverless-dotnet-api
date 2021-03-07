using System;
using Amazon.DynamoDBv2.DataModel;
using ServerlessDotnetApi.Models;

namespace ServerlessDotnetApi.Persistence
{
    [DynamoDBTable("User")]
    public class UserItem
    {
        [DynamoDBHashKey] // Having a hash key is mandatory
        //[DynamoDBRangeKey] Having a range key is optional
        public string Username { get; set; }
        //[DynamoDBProperty(AttributeName = "name")] Having a property is optional in .net3, used only when the table field name differs from member
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}