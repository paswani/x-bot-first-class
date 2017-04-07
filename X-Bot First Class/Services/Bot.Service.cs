using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;

namespace X_Bot_First_Class.Services
{
    /// <summary>
    /// Public class used to send messages via the bot
    /// </summary>
    public static class Bot
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns></returns>
        public static async Task<ResourceResponse> SendMessage(MessagePayload data, MicrosoftAppCredentials credentials)
        {
            string fromId = data.FromId;
            string fromName = data.FromName ?? "ExpressBot";
            string toId = data.ToId;
            string toName = data.ToName;
            string locale = data.ToName ?? "en-US";
            string text = data.Text;
            
            var botAccount = new ChannelAccount(fromId, fromName);
            var userAccount = new ChannelAccount(toId, toName);

            // trust the service url if it is not already trusted
            if (!MicrosoftAppCredentials.IsTrustedServiceUrl(data.ServiceUrl))
            {
                MicrosoftAppCredentials.TrustServiceUrl(data.ServiceUrl);
            }

            var connector = new ConnectorClient(new Uri(data.ServiceUrl), credentials);

            var conversation = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);

            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(false, conversation.Id);
            message.Text = text;
            message.Locale = locale;
            return await connector.Conversations.SendToConversationAsync((Activity)message);
        }
    }
}