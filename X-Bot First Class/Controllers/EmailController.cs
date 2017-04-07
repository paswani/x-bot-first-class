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
    public class EmailController : ApiController
    {
        /// <summary>
        /// Sends the rejection notice.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="name">The name.</param>
        /// <param name="recruiterName">Name of the recruiter.</param>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/email/rejectionNotice")]
        public async Task<HttpResponseMessage> RejectionNotice(string email, string name, string recruiterName, string job)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(recruiterName) || string.IsNullOrEmpty(job))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // send the email
            var payload = new MessagePayload()
            {
                ToId = email,
                Text = string.Format("Hello, {0}!. This is Rachael from Express. How was your first day at {1}?", name),
                ServiceUrl = ConfigurationManager.AppSettings["BotFramework_EmailServiceUrl"]
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await Bot.SendMessage(payload, credentials);

            // save the conversation state so when the recipient responds we know in what context they replied in
            var stateClient = new StateClient(new Uri(ConfigurationManager.AppSettings["BotFramework_StateServiceUrl"]), credentials);
            var userData = await stateClient.BotState.GetUserDataAsync("email", email);
            userData.SetProperty<string>("conversationType", ConversationType.RejectionNotice.ToString());
            userData.SetProperty<string>("recruiterName", recruiterName);
            await stateClient.BotState.SetUserDataAsync("email", email, userData);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}