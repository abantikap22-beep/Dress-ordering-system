using dress_ordering_system.Services;
using Microsoft.AspNetCore.Mvc;

namespace dress_ordering_system.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly OpenAIChatService _chatService;

        public ChatbotController(OpenAIChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask(
            [FromBody] ChatRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please enter a message."
                });
            }

            try
            {
                string response =
                    await _chatService.GetResponseAsync(
                        request.Message);

                return Json(new
                {
                    success = true,
                    message = response
                });
            }
            catch (Exception ex)
            {
                // Show the real error while testing
                Console.WriteLine("================================");
                Console.WriteLine("CHATBOT ERROR");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "AI Error: " + ex.Message
                });
            }
        }
    }


    public class ChatRequest
    {
        public string Message { get; set; } = "";
    }
}