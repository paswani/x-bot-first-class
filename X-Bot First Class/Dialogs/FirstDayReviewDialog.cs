using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using X_Bot_First_Class.Common;

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
            context.UserData.RemoveValue("conversationType");

            // do sentiment analysis and give appropriate response

            await context.PostAsync("first day review");

            context.Done<string>(null);
        }
    }
}