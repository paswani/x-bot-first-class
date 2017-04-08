using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using WildMouse.Unearth.Cognitive.TextAnalytics;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Factories;

namespace X_Bot_First_Class.Dialogs
{
    /// <summary>
    /// First Day Review dialog class
    /// </summary>
    [LuisModel("2a49aec7-3787-4105-b52e-c43e401599e0", "08107661360644ab8532afffc187bbac")]
    [Serializable]
    public class FirstDayReviewDialog : LuisDialogBase<object>
    {
        /// <summary>
        /// No intent.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            var textAnalyticsClient = new TextAnalyticsClient(ConfigurationManager.AppSettings["CognitiveServices_TextAnalyticsKey"]);
            var sentiment = await textAnalyticsClient.GetSentimentForTextAsync(result.Query);

            // attempt to obtain applicant info
            var a = await ApplicantFactory.GetApplicantByContext(context);
            var app = a.Applications.First().Value;

            if (sentiment < 0.45)
            {
                PromptDialog.Text(context, ResumeAfterEmailPromptAsync, string.Format("I'm sorry to hear that. Would you like me to email your recruiter, {0}, so that you can discuss the matter?", app.Recrutier.Name));
            }
            else if (sentiment >= 0.45 && sentiment <= 0.75)
            {
                await context.PostAsync(string.Format("Ok. Contact your recruiter, {0}, if there is anything you would like to discuss regarding your placement.", app.Recrutier.Name));

                context.Done<string>(null);
            }
            else
            {
                await context.PostAsync("Great! That's good to hear. I'll check back in with you toward the end of your contract.");

                context.Done<string>(null);
            }
        }

        /// <summary>
        /// Resumes the after email prompt asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task ResumeAfterEmailPromptAsync(IDialogContext context, IAwaitable<string> result)
        {
            var response = await result;
            if (Regex.Match(response, "(Yes|yes|yea|yeah|ok|sure)").Success)
            {
                // attempt to obtain applicant info
                var a = await ApplicantFactory.GetApplicantByContext(context);
                var app = a.Applications.First().Value;

                await context.PostAsync(string.Format("Ok. I have sent {0} an email and you should be hearing from us soon.", app.Recrutier.Name));

                context.Done<string>(null);
            }
            else
            {
                await context.PostAsync("Ok. If you would like to discuss this in the future, please do not hesitate to reach out to us.");

                context.Done<string>(null);
            }
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
            // hack to force all replies that are not strictly trained for this intent to route to the None intent
            if (result.TopScoringIntent.Score < 0.60)
            {
                await None(context, result);
            }
            else
            {
                await context.PostAsync("Take care!");

                context.Done<string>(null);
            }
        }
    }
}