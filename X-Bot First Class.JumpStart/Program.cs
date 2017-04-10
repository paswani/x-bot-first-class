using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Services;
using System.Linq;
using X_Bot_First_Class.Common.Models;
using X_Bot_First_Class.Factories;

namespace X_Bot_First_Class.JumpStart
{
    class Program
    {
        static void Main(string[] args)
        {
            UpdateApplicant("12816459336").Wait();
        }

        public static async Task UpdateApplicant(string phoneNumber)
        {
            Applicant a = null;
            a = await ApplicantFactory.GetApplicantByPhone("12817489336");
            a.Applications.First().Value.Recrutier = new Recruiter()
            {
                Name = "Alan Zigelman",
            };
            await ApplicantFactory.PersistApplicant(a);
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

        public static async Task SendEmail()
        {
            var name = "Zubair";
            var job = "chef";

            var jobTitles = Express.GetJobSuggestions(job);

            var jobSuggestionHtml = new StringBuilder();
            var filteredJobTitles = jobTitles.Take(3).ToList<string>();
            foreach (var jobTitle in filteredJobTitles)
            {
                jobSuggestionHtml.Append("<li><a href='#'>" + jobTitle + "</a>");
            }

            // send the email
            dynamic channelData = new ExpandoObject();
            channelData.HtmlBody = string.Format(Resources.rejectionEmailTemplate, name, job, filteredJobTitles.Count, jobSuggestionHtml.ToString());
            channelData.Subject = string.Format("Job Application Response: {0}", job);

            await Bot.SendMessage(new MessagePayload()
            {
                FromId = "zubairv85@zubairv.onmicrosoft.com",
                ToId = "zubairv85@gmail.com",
                ServiceUrl = "https://email.botframework.com",
                ChannelData = channelData
            }, new MicrosoftAppCredentials("c537f444-c2f9-42f3-984d-255fc66d4ef6", "uvQrm1qEMiCtXomcDgpWXcz"));
        }
    }
}
