using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vektora.Authentication.Interfaces;

namespace Chatty.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMembershipProvider<IdentityUser, IdentityRole, KaleUstaPortalContext> _membershipProvider;
        private readonly IConversationRepository _conversationRepository;

        public HomeController(IMembershipProvider<IdentityUser, IdentityRole, KaleUstaPortalContext> membershipProvider, IConversationRepository conversationRepository)
        {
            _membershipProvider = membershipProvider;
            _conversationRepository = conversationRepository;
        }

        [Route("~/auth/sign-in")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var signInResult = await _membershipProvider.SignIn(email, password, true, false);
            if (signInResult.Succeeded) return RedirectToAction("Index", "Home");
            return Json(signInResult);
        }

        [Route("~/auth/create")]
        public async Task<IActionResult> CreateUser(string email, string password)
        {
            IdentityResult result = await _membershipProvider.CreateUser(new IdentityUser
            {
                Email = email,
                UserName = email
            }, password);
            return Json(result);
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

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
