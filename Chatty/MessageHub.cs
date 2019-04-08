using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatty
{
    [Authorize]
    public class MessageHub : Hub
    {
        public IConversationRepository ConversationRepository { get; }
        public static ConcurrentDictionary<string, Person> Persons { get; set; } = new ConcurrentDictionary<string, Person>();

        public MessageHub(IConversationRepository conversationRepository) => ConversationRepository = conversationRepository;

        public async Task Connect()
        {
            string id = Context.ConnectionId;
            if (!Persons.ContainsKey(Context.UserIdentifier))
            {
                Person person = new Person(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value, id, Context.User.Identity.Name);
                Persons.TryAdd(Context.UserIdentifier, person);
                List<Person> persons = Persons.Values.Where(p => p.ConnectionId != id).ToList();
                await Clients.Caller.SendAsync("onConnected", persons.ToArray());
                await Clients.AllExcept(id).SendAsync("onNewUserConnected", person);
            }
        }

        public async Task SendMessageToAll(string userName, string message, string image)
        {
            await Clients.All.SendAsync("messageReceived", userName, message);
        }

        public async Task SendPrivateMessage(string toUserId, string message)
        {
            Person toUser = Persons.Values.FirstOrDefault(person => person.Id == toUserId);
            Person fromUser = Persons.Values.FirstOrDefault(person => person.Id == Context.UserIdentifier);
            if (toUser != null && fromUser != null)
            {
                await Clients.Client(toUser.ConnectionId).SendAsync("receivePrivateMessage", fromUser, message);
                string objectId = await ConversationRepository.GetOrCreateConversationId(new[] { fromUser.Id, toUser.Id });
                await ConversationRepository.AddMessageToConversation(objectId, fromUser.Id, message);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            bool exsist = Persons.TryGetValue(Context.UserIdentifier, out Person person);
            if (exsist)
            {
                Persons.TryRemove(Context.UserIdentifier, out Person deletedPerson);
                Clients.All.SendAsync("onUserDisconnected", deletedPerson).GetAwaiter().GetResult();
            }
            return base.OnDisconnectedAsync(exception);
        }
    }

    public class Person
    {
        public Person(string id, string connectionId, string name)
        {
            Id = id;
            ConnectionId = connectionId;
            Name = name;
        }

        public string ConnectionId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection) => connection.User.FindFirst(ClaimTypes.NameIdentifier).Value;
    }
}
