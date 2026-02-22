// Pages/API/Rooms.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RChat.Services;

namespace RChat.Pages.API
{
    [Route("api/rooms")]
    public class RoomsModel : PageModel
    {
        private readonly IChatRoomService _roomService;
        public RoomsModel(IChatRoomService roomService) => _roomService = roomService;

        public async Task<IActionResult> OnGet()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            var result = rooms.Select(r => new
            {
                name = r.Name,
                isProtected = r.IsProtected
            }).OrderBy(r => r.name);
            return new JsonResult(result);
        }
    }
}