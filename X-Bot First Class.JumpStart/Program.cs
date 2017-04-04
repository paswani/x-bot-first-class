using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace X_Bot_First_Class.JumpStart
{
    class Program
    {
        static void Main(string[] args)
        {
            SendSms().Wait();
        }

        public static async Task SendSms()
        {
            var serviceUrl = new Uri("https://sms.botframework.com");
            var botAccount = new ChannelAccount("+18327722087", "ExpressBot");
            var userAccount = new ChannelAccount("+12817489336", "Zubair");

            if (!MicrosoftAppCredentials.IsTrustedServiceUrl(serviceUrl.ToString()))
            {
                MicrosoftAppCredentials.TrustServiceUrl(serviceUrl.ToString());
            }

            var connector = new ConnectorClient(serviceUrl, "c537f444-c2f9-42f3-984d-255fc66d4ef6", "uvQrm1qEMiCtXomcDgpWXcz");
            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);

            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(false, conversationId.Id);
            message.Text = "Hello";
            message.Locale = "en-Us";
            var conv = await connector.Conversations.SendToConversationAsync((Activity)message);
        }
    }
}
