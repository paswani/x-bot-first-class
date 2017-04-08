using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Services;

namespace X_Bot_First_Class
{
    public class SmsController : ApiController
    {
        /// <summary>
        /// Sends the first day review.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="name">The name.</param>
        /// <param name="company">The company.</param>
        /// <param name="recruiterName">Name of the recruiter.</param>
        /// <returns></returns>
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
            var payload = new MessagePayload()
            {
                FromId = ConfigurationManager.AppSettings["Twilio_PhoneNumber"],
                ToId = phoneNumber,
                Text = string.Format("Hello, {0}!. This is Rachael from Express. How was your first day at {1}?", name, company),
                ServiceUrl = ConfigurationManager.AppSettings["BotFramework_SmsServiceUrl"]
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await Bot.SendMessage(payload, credentials);

            // save the conversation state so when the recipient responds we know in what context they replied in
            var stateClient = new StateClient(new Uri(ConfigurationManager.AppSettings["BotFramework_StateServiceUrl"]), credentials);
            var userData = await stateClient.BotState.GetUserDataAsync("sms", phoneNumber);
            userData.SetProperty<string>("conversationType", ConversationType.FirstDayReview.ToString());
            userData.SetProperty<string>("recruiterName", recruiterName);
            await stateClient.BotState.SetUserDataAsync("sms", phoneNumber, userData);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}