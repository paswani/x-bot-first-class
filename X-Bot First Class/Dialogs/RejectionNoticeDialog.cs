using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;

namespace X_Bot_First_Class.Dialogs
{
    /// <summary>
    /// Rejection Notice dialog class
    /// </summary>
    [LuisModel("b71a7e18-f087-498a-852a-f09650aae283", "08107661360644ab8532afffc187bbac")]
    [Serializable]
    public class RejectionNoticeDialog : LuisDialogBase<object>
    {
        /// <summary>
        /// Apply intent.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        [LuisIntent("")]
        [LuisIntent("Apply")]
        [LuisIntent("None")]
        public async Task Apply(IDialogContext context, LuisResult result)
        {
            var entities = result.Entities;
            var jobIds = entities.Select(entity => entity.Entity).ToList<string>();
            var response = "I'm sorry, I did not understand your response.";
            var name = string.Empty;
            context.UserData.TryGetValue<string>("name", out name);

            if (jobIds.Count == 1)
            {
                response = "OK. I have applied for the 1 position you selected. Good luck!";
            }
            else if (jobIds.Count > 1)
            {
                response = "OK. I have applied for the " + jobIds.Count + " positions you selected. Good luck!";
            }

            dynamic channelData = new ExpandoObject();
            channelData.HtmlBody = string.Format(Resources.suggestionApplyEmailTemplate, response);

            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = context.Activity.From;
            message.Recipient = context.Activity.Recipient;
            message.Conversation = context.Activity.Conversation;
            message.Locale = "en-us";
            message.ChannelData = channelData;

            await context.PostAsync(message);

            context.Done<string>(null);
        }

        /// <summary>
        /// Goodbye intent.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        [LuisIntent("ThankYou")]
        public async Task ThankYou(IDialogContext context, LuisResult result)
        {
            dynamic channelData = new ExpandoObject();
            channelData.HtmlBody = Resources.thankYouEmailTemplate;

            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = context.Activity.From;
            message.Recipient = context.Activity.Recipient;
            message.Conversation = context.Activity.Conversation;
            message.Locale = "en-us";
            message.ChannelData = channelData;

            await context.PostAsync(message);

            context.Done<string>(null);
        }

        /// <summary>
        /// Goodbye intent.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        [LuisIntent("Goodbye")]
        public async Task Goodbye(IDialogContext context, LuisResult result)
        {
            dynamic channelData = new ExpandoObject();
            channelData.HtmlBody = Resources.goodbyeEmailTemplate;

            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = context.Activity.From;
            message.Recipient = context.Activity.Recipient;
            message.Conversation = context.Activity.Conversation;
            message.Locale = "en-us";
            message.ChannelData = channelData;

            await context.PostAsync(message);

            context.Done<string>(null);
        }
    }
}