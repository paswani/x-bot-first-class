using System;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Common.Models;
using X_Bot_First_Class.Factories;
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
        /// <returns></returns>
        [HttpGet]
        [Route("api/email/rejectionNotice")]
        public async Task<HttpResponseMessage> RejectionNotice(string email, string jobId)
        {
            return await RejectionNotice(string.Empty, email, string.Empty, jobId, string.Empty, string.Empty);
        }

        /// <summary>
        /// Sends the rejection notice.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="email">The email.</param>
        /// <param name="name">The name.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="jobTitle">The job title.</param>
        /// <param name="company">The company.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/email/rejectionNotice")]
        public async Task<HttpResponseMessage> RejectionNotice(string phoneNumber, string email, string name, string jobId, string jobTitle, string company)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(jobId)) return Request.CreateResponse(HttpStatusCode.BadRequest);

            // find application
            Application app = null;
            Applicant a = await ApplicantFactory.GetApplicantByEmail(email);
            if (a == null)
            {
                if (!string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(name))
                {
                    a = new Applicant() { Phone = phoneNumber, Email = email, Name = name };
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            if (!a.Applications.Keys.Contains(jobId))
            {
                if (!string.IsNullOrEmpty(jobTitle) && !string.IsNullOrEmpty(company))
                {
                    app = new Application() { Id = jobId, Applied = DateTime.Parse("1/1/2017"), State = ConversationType.None, Title = jobTitle, Company = company };
                    a.Applications.Add(jobId, app);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            app = a.Applications[jobId];

            // do job matching
            var jobTitles = Express.GetJobSuggestions(app.Title);

            var jobSuggestionHtml = new StringBuilder();
            var filteredJobTitles = jobTitles.Take(3).ToList<string>();
            foreach (var title in filteredJobTitles)
            {
                jobSuggestionHtml.Append("<li><a href='#'>" + title + "</a>");
            }

            // send the email
            dynamic channelData = new ExpandoObject();
            channelData.HtmlBody = string.Format(Resources.rejectionEmailTemplate, a.Name, app.Title, filteredJobTitles.Count, jobSuggestionHtml.ToString());
            channelData.Subject = string.Format("Job Application Response: {0}", app.Title);

            var payload = new MessagePayload()
            {
                FromId = ConfigurationManager.AppSettings["Office356_Email"],
                ToId = email,
                ChannelData = channelData,
                ServiceUrl = ConfigurationManager.AppSettings["BotFramework_EmailServiceUrl"]
            };
            var credentials = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var response = await Bot.SendMessage(payload, credentials);

            // save the conversation state so when the recipient responds we know in what context they replied in
            app.State = ConversationType.RejectionNotice;
            await ApplicantFactory.PersistApplicant(a);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}