using System;
using Amazon.DynamoDBv2.DataModel;
using Main.Models;

namespace Main.Persistence
{
    [DynamoDBTable("Todo")]
    public class TodoItem
    {
        [DynamoDBHashKey] public string Id { get; set; }

        public string UserEmail { get; set; }
        public string Name { get; set; }
        public int Status {get; set;}
    }
}