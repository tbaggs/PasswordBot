using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DirectLineClient
{
    public class DirectLine
    {
        public async Task<ChatConfig> Authenticate(string directLineSecret)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                $"https://directline.botframework.com/v3/directline/tokens/generate");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", directLineSecret);

            var userId = $"dl_{Guid.NewGuid()}";

            request.Content = new StringContent(JsonConvert.SerializeObject(new { User = new { Id = userId } }), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            string token = String.Empty;

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<Conversation>(body).token;
            }

            var config = new ChatConfig()
            {
                Token = token,
                UserId = userId
            };

            return config;
        }

        public async Task<MessageEvent> SendMessage(string message, Conversation conversation, ChatConfig chatConfig)
        {
            MessageEvent botMessageEvent = null;

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://directline.botframework.com/v3/directline/conversations/{conversation.conversationId}/activities");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", conversation.token);

            var botMessage = new { type = "message", text = message, from = new { id = chatConfig.UserId }};

            request.Content = new StringContent(JsonConvert.SerializeObject(botMessage), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                botMessageEvent = JsonConvert.DeserializeObject<MessageEvent>(body);
            }

            return botMessageEvent;
        }

        public async Task<Conversation> StartConversation(ChatConfig chatConfig)
        {
            Conversation conversation = null;

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                $"https://directline.botframework.com/v3/directline/conversations");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", chatConfig.Token);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                conversation = JsonConvert.DeserializeObject<Conversation>(body);
            }

            return conversation;
        }
    }

    public class Conversation
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
        public string streamUrl { get; set; }
    }

    public class ChatConfig
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }

    public class MessageEvent
    {
        public string id { get; set; }
    }

    public class PasswordEvent
    {
        public string UserID { get; set; }
        public string PasswordExpirationDate { get; set; }
    }

    public class PasswordEventNotification
    {
        [JsonProperty("PasswordEventNotification")]
        public PasswordEvent PasswordEvent { get; set; }
    }
}

