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
        /// <param name="recruiterName">Name of the recruiter.</param>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/email/rejectionNotice")]
        public async Task<HttpResponseMessage> RejectionNotice(string email, string jobId)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(jobId)) return Request.CreateResponse(HttpStatusCode.BadRequest);

            // find application
            Application app = null;
            Applicant a = await ApplicantFactory.GetApplicantByEmail(email);
            if (a == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!a.Applications.Keys.Contains(jobId)) return Request.CreateResponse(HttpStatusCode.NotFound);
            app = a.Applications[jobId];

            // do job matching
            var jobTitles = Express.GetJobSuggestions(app.Title);

            var jobSuggestionHtml = new StringBuilder();
            var filteredJobTitles = jobTitles.Take(3).ToList<string>();
            foreach(var jobTitle in filteredJobTitles)
            {
                jobSuggestionHtml.Append("<li><a href='#'>" + jobTitle + "</a>");
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