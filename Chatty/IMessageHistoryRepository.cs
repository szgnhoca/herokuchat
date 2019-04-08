using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Chatty.Helpers;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Chatty
{
    public interface IConversationRepository
    {
        Task<string> GetOrCreateConversationId(string[] members);
        Task AddMessageToConversation(string conversationId, string authorId, string body);
        Task<List<Message>> GetLastMessagesOfConversation(string conversationId, int page = 0, int size = 10);
    }

    public class ConversationRepository : IConversationRepository
    {
        private readonly IMongoDatabase _db;

        protected IMongoCollection<Conversation> ConversationCollection => _db.GetCollection<Conversation>(nameof(Conversation));
        protected IMongoCollection<Message> MessageCollection => _db.GetCollection<Message>(nameof(Message));

        public ConversationRepository(IConfiguration configuration, MongoDBHerokuConnectionProvider mongoDBCString)
        {
            //var values = mongoDBCString.GetConnectionString();
            (string url, string dbName) = mongoDBCString.GetConnectionString();
            MongoClient client = new MongoClient(url);
            _db = client.GetDatabase(dbName);
        }

        public async Task<string> GetOrCreateConversationId(string[] members)
        {
            Guid[] guidMembers = Array.ConvertAll(members.OrderBy(s => s).ToArray(), Guid.Parse);
            Expression<Func<Conversation, bool>> filter = document => document.Members == guidMembers || document.Members == guidMembers.Reverse();
            Conversation conversation = await ConversationCollection.Find(filter).SingleOrDefaultAsync();
            if (conversation is null)
            {
                Conversation conversationToAdd = new Conversation
                {
                    Members = guidMembers
                };
                await ConversationCollection.InsertOneAsync(conversationToAdd);
                return conversationToAdd.Id.ToString();
            }
            return conversation.Id.ToString();
        }

        public async Task AddMessageToConversation(string conversationId, string authorId, string body)
        {
            await MessageCollection.InsertOneAsync(new Message
            {
                ConversationId = ObjectId.Parse(conversationId),
                AuthorId = Guid.Parse(authorId),
                Body = body
            });
        }

        public async Task<List<Message>> GetLastMessagesOfConversation(string conversationId, int page = 0, int size = 10)
        {
            FilterDefinition<Message> filter =
                Builders<Message>.Filter.Where(conversation => conversation.ConversationId == ObjectId.Parse(conversationId));
            SortDefinition<Message> sort = Builders<Message>.Sort.Descending(conversation => conversation.TimeStamp);
            List<Message> messages = await MessageCollection
                .Find(filter)
                .Sort(sort)
                .Skip(page * size)
                .Limit(size)
                .ToListAsync();
            return messages;
        }
    }

    public class Conversation
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Members")]
        public Guid[] Members { get; set; }
    }

    public class Message
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("ConversationId")]
        public ObjectId ConversationId { get; set; }

        [BsonElement("AuthorId")]
        public Guid AuthorId { get; set; }

        [BsonElement("Body")]
        public string Body { get; set; }

        [BsonElement("TimeStamp")]
        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}