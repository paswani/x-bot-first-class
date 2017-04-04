using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using X_Bot_First_Class.Common;

namespace X_Bot_First_Class
{
    public class SmsController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        [Route("api/sms/firstdayreview")]
        public async Task<HttpResponseMessage> FirstDayReview([FromBody]string dataString)
        {
            var data = JsonConvert.DeserializeObject<SmsPayload>(dataString);
            if (data == null) return Request.CreateResponse(HttpStatusCode.BadRequest);

            string fromId = data.FromId ?? "+18327722087";
            string fromName = data.FromName ?? "ExpressBot";
            string toId = data.ToId;
            string toName = data.ToName;
            string locale = data.ToName ?? "en-US";
            string text = data.Text;

            var serviceUrl = new Uri("https://sms.botframework.com");
            var botAccount = new ChannelAccount(fromId, fromName);
            var userAccount = new ChannelAccount(toId, toName);

            if (!MicrosoftAppCredentials.IsTrustedServiceUrl(serviceUrl.ToString()))
            {
                MicrosoftAppCredentials.TrustServiceUrl(serviceUrl.ToString());
            }

            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var connector = new ConnectorClient(serviceUrl, credentials);

            var conversation = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);

            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(false, conversation.Id);
            message.Text = text;
            message.Locale = locale;
            var conv = await connector.Conversations.SendToConversationAsync((Activity)message);

            var state = new StateClient(new Uri("https://state.botframework.com"), credentials);
            var userData = await state.BotState.GetUserDataAsync("sms", userAccount.Id);
            userData.SetProperty<string>("conversationType", ConversationType.SmsFirstDayReview.ToString());
            await state.BotState.SetUserDataAsync("sms", userAccount.Id, userData);

            return Request.CreateResponse(HttpStatusCode.OK, conv);
        }
    }
}