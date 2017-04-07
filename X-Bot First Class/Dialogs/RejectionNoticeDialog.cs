using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
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

            if (jobIds.Count == 1)
            {
                response = "Ok. I have applied for the 1 position you selected. Good luck!";
            }
            else if (jobIds.Count > 1)
            {
                response = "Ok. I have applied for the " + jobIds.Count + " positions you selected. Good luck!";
            }

            await context.PostAsync(response);

            context.Done<string>(null);
        }
    }
}