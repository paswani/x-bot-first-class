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
        public async Task<HttpResponseMessage> FirstDayReview(string phoneNumber, string jobId)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(jobId)) return Request.CreateResponse(HttpStatusCode.BadRequest);

            // find application
            Application app = null;
            Applicant a = await ApplicantFactory.GetApplicantByPhone(phoneNumber);
            if (a == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!a.Applications.Keys.Contains(jobId)) return Request.CreateResponse(HttpStatusCode.NotFound);
            app = a.Applications[jobId];

            if (!phoneNumber.StartsWith("+")) phoneNumber = string.Concat("+", phoneNumber);
            var payload = new MessagePayload()
            {
                FromId = ConfigurationManager.AppSettings["Twilio_PhoneNumber"],
                ToId = phoneNumber,
                Text = string.Format("Hello, {0}!. This is Rachel from Express. How was your first day at {1}?", a.Name, app.Company),
                ServiceUrl = ConfigurationManager.AppSettings["BotFramework_SmsServiceUrl"]
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await Bot.SendMessage(payload, credentials);

            // save the conversation state so when the recipient responds we know in what context they replied in
            app.State = ConversationType.FirstDayReview;
            await ApplicantFactory.PersistApplicant(a);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Schedule an interview.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/sms/scheduleinterview")]
        public async Task<HttpResponseMessage> ScheduleInterview(string phoneNumber, string jobId)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return Request.CreateResponse(HttpStatusCode.BadRequest);

            // find application
            Application app = null;
            Applicant a = await ApplicantFactory.GetApplicantByPhone(phoneNumber);
            if (a == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!a.Applications.Keys.Contains(jobId)) return Request.CreateResponse(HttpStatusCode.NotFound);
            app = a.Applications[jobId];

            if (!phoneNumber.StartsWith("+")) phoneNumber = string.Concat("+", phoneNumber);
            var payload = new MessagePayload()
            {
                FromId = ConfigurationManager.AppSettings["Twilio_PhoneNumber"],
                ToId = phoneNumber,
                Text = $"Hello, {a.Name}!. This is Rachel from Express. Your recruiter would like to schedule an interview with you. Would you like to schedule the interview?",
                ServiceUrl = ConfigurationManager.AppSettings["BotFramework_SmsServiceUrl"]
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await Bot.SendMessage(payload, credentials);

            // save the conversation state so when the recipient responds we know in what context they replied in
            app.State = ConversationType.ScheduleInterview;
            await ApplicantFactory.PersistApplicant(a);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Sends the an SMS informing the applicant of job selection and invites the .
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="jobId">The id of the job the applicant is accepted for.</param>
        /// <returns>Async HttpResponseMessage</returns>
        [HttpGet]
        [Route("api/sms/applicantaccepted")]
        public async Task<HttpResponseMessage> ApplicantAccepted(string phoneNumber, string jobId)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(jobId)) return Request.CreateResponse(HttpStatusCode.BadRequest);
            
            // find application
            Application app = null;
            Applicant a = await ApplicantFactory.GetApplicantByPhone(phoneNumber);
            if (a == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!a.Applications.Keys.Contains(jobId)) return Request.CreateResponse(HttpStatusCode.NotFound);
            app = a.Applications[jobId];

            if (!phoneNumber.StartsWith("+")) phoneNumber = string.Concat("+", phoneNumber);
            var payload = new MessagePayload()
            {
                FromId = ConfigurationManager.AppSettings["Twilio_PhoneNumber"],
                ToId = phoneNumber,
                Text = string.Format("Hello, {0}. This is Rachel from Express. It is my pleasure to inform you that you have been accepted for the position '{1}' at {2}. I'd like to walk you through filling out your IRS W-4 form. We are required to get this information from you before you can start your position. To get started, please add me to your contacts in skype.", 
                    a.Name, app.Title, app.Company),
                ServiceUrl = ConfigurationManager.AppSettings["BotFramework_SmsServiceUrl"]
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await Bot.SendMessage(payload, credentials);

            app.State = ConversationType.FillOutW4;
            await ApplicantFactory.PersistApplicant(a);

            // save the conversation state so when the recipient responds we know in what context they replied in
            app.State = ConversationType.FillOutW4;
            await ApplicantFactory.PersistApplicant(a);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}