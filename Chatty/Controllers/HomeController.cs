using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatty.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConversationRepository _conversationRepository;

        public HomeController(IConversationRepository conversationRepository) => _conversationRepository = conversationRepository;

        [Authorize]
        public IActionResult Index() => View();

        [Authorize]
        public async Task<IActionResult> History(string otherMember)
        {
            string convId = await _conversationRepository.GetOrCreateConversationId(new[] { otherMember, User.FindFirstValue(ClaimTypes.NameIdentifier) });
            List<Message> messages = await _conversationRepository.GetLastMessagesOfConversation(convId);
            List<MessageViewModel> messageViewModels = messages.Select(message => new MessageViewModel
            {
                Mine = message.AuthorId.ToString() == User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Message = message.Body,
                TimeStamp = message.TimeStamp
            }).ToList();
            return Json(messageViewModels);
        }
    }

    public class MessageViewModel
    {
        public string Message { get; set; }
        public bool Mine { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
