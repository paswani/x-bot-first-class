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
        [HttpGet]
        [Route("api/sms/firstdayreview")]
        public async Task<HttpResponseMessage> FirstDayReview(string phoneNumber, string name, string company, string recruiterName)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(company) || string.IsNullOrEmpty(recruiterName))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            if (!phoneNumber.StartsWith("+"))
            {
                phoneNumber = string.Concat("+", phoneNumber);
            }

            // send the sms
            var smsPayload = new SmsPayload()
            {
                ToId = phoneNumber,
                Text = string.Format("Hello, {0}!. This is Rachael from Express. How was your first day at {1}?", name, company)
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await SendSms(smsPayload, credentials);

            // save the conversation state so when the recipient responds we know in what context they replied in
            var stateClient = new StateClient(new Uri(ConfigurationManager.AppSettings["BotFramework_StateServiceUrl"]), credentials);
            var userData = await stateClient.BotState.GetUserDataAsync("sms", phoneNumber);
            userData.SetProperty<string>("conversationType", ConversationType.FirstDayReview.ToString());
            userData.SetProperty<string>("recruiterName", recruiterName);
            await stateClient.BotState.SetUserDataAsync("sms", phoneNumber, userData);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Send sms
        /// </summary>
        /// <param name="data"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private async Task<ResourceResponse> SendSms(SmsPayload data, MicrosoftAppCredentials credentials)
        {
            string fromId = data.FromId ?? ConfigurationManager.AppSettings["Twilio_PhoneNumber"];
            string fromName = data.FromName ?? "ExpressBot";
            string toId = data.ToId;
            string toName = data.ToName;
            string locale = data.ToName ?? "en-US";
            string text = data.Text;

            var serviceUrl = new Uri(ConfigurationManager.AppSettings["BotFramework_SmsServiceUrl"]);
            var botAccount = new ChannelAccount(fromId, fromName);
            var userAccount = new ChannelAccount(toId, toName);

            // trust the service url if it is not already trusted
            if (!MicrosoftAppCredentials.IsTrustedServiceUrl(serviceUrl.ToString()))
            {
                MicrosoftAppCredentials.TrustServiceUrl(serviceUrl.ToString());
            }

            var connector = new ConnectorClient(serviceUrl, credentials);

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