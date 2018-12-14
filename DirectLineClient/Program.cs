using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DirectLineClient
{
    class Program
    {
        private const string directLineSecret = "{PUT_DIRECTLINE_KEY_HERE}";

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();

            Console.ReadKey();
        }

        public static async Task MainAsync(string[] args)
        {
            DirectLine dl = new DirectLine();

            ChatConfig cf = await dl.Authenticate(directLineSecret);

            Console.WriteLine("User ID" + cf.UserId);
            Console.WriteLine("Token" + cf.Token);

            Conversation conversation = await dl.StartConversation(cf);

            Console.WriteLine("Conversation id" + conversation.conversationId);


            PasswordEventNotification pe = new PasswordEventNotification
            {
                PasswordEvent = new PasswordEvent { UserID = "tbaggs@microsoft.com", PasswordExpirationDate = DateTime.Now.AddDays(7).ToLongDateString() }
            };

            MessageEvent msgEvent = await dl.SendMessage(JsonConvert.SerializeObject(pe), conversation, cf);

            Console.WriteLine("message event code: " + msgEvent.id);
        }
    }
}

