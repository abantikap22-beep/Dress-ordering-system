#pragma warning disable OPENAI001

using OpenAI.Responses;

namespace dress_ordering_system.Services
{
    public class OpenAIChatService
    {
        private readonly ResponsesClient _client;

        public OpenAIChatService(IConfiguration configuration)
        {
            string? apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is missing. Check User Secrets: OpenAI:ApiKey");
            }

            _client = new ResponsesClient(apiKey);
        }

        public async Task<string> GetResponseAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "Please type a message.";
            }

            try
            {
                ResponseResult response =
                    await _client.CreateResponseAsync(
                        "gpt-5-mini",
                        message);

                string? answer = response.GetOutputText();

                if (string.IsNullOrWhiteSpace(answer))
                {
                    return "Sorry, I could not generate a response.";
                }

                return answer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("OPENAI CHATBOT ERROR");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                return "AI_ERROR: " + ex.Message;
            }
        }
    }
}

#pragma warning restore OPENAI001