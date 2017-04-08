using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Common.Models;
using X_Bot_First_Class.Services;
using X_Bot_First_Class.Factories;

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


        /// <summary>
        /// Sends the an SMS informing the applicant of job selection and invites the .
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="name">The name.</param>
        /// <param name="jobId">The id of the job the applicant is accepted for.</param>
        /// <returns>Async HttpResponseMessage</returns>
        [HttpGet]
        [Route("api/sms/applicantaccepted")]
        public async Task<HttpResponseMessage> ApplicantAccepted(string phoneNumber, string jobId)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(jobId)) return Request.CreateResponse(HttpStatusCode.BadRequest);
            if (!phoneNumber.StartsWith("+")) phoneNumber = string.Concat("+", phoneNumber);

            // find application
            Application app = null;
            Applicant a = await ApplicantFactory.GetApplicantByPhone(phoneNumber);
            if (a == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!a.Applications.Keys.Contains(jobId)) return Request.CreateResponse(HttpStatusCode.NotFound);
            app = a.Applications[jobId];
            var payload = new MessagePayload()
            {
                FromId = ConfigurationManager.AppSettings["Twilio_PhoneNumber"],
                ToId = phoneNumber,
                Text = string.Format("Hello, {0}!. This is Rachael from Express. It is my pleasure to inform you that you have been accepted for the position '{1}' at {2}. I'd like to walk you through filling out your IRS W-9 form. We are required to get this information from you before you can start your position. To get started, please add me to your contacts in skype.", 
                    a.Name, app.Title, app.Company),
                ServiceUrl = ConfigurationManager.AppSettings["BotFramework_SmsServiceUrl"]
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await Bot.SendMessage(payload, credentials);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}