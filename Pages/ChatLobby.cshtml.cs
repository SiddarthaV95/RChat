using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace RChat.Pages
{
    [Authorize]
    public class ChatLobbyModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}