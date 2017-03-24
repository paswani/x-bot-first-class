using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace X_Bot_First_Class.Dialogs
{
    /// <summary>
    /// Root dialog class
    /// </summary>
    [LuisModel("660d5761-1f2f-4e2c-8d30-e937614ea94f", "b42a61226cd749b693bec5142aafc557")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        /// <summary>
        /// Welcome intent.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        [LuisIntent("Welcome")]
        public async Task Welcome(IDialogContext context, LuisResult result)
        {
            var userData = await GetUserData(context);

            var name = userData.GetProperty<string>("name");

            if (string.IsNullOrEmpty(name))
            {
                // prompt for the user's name
                PromptDialog.Text(context, ResumeAfterNamePromptAsync, Resources.msgWelcome);
            }
            else
            {
                await context.PostAsync(string.Format(Resources.msgWelcomeBack, name));
            }
        }

        /// <summary>
        /// Callback for name prompt.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task ResumeAfterNamePromptAsync(IDialogContext context, IAwaitable<string> result)
        {
            var userData = await GetUserData(context);

            var name = await result;
            if (!string.IsNullOrEmpty(name))
            {
                // persist the data for the current user
                userData.SetProperty<string>("name", name);
                await context.PostAsync(string.Format(Resources.mgsWelcomeWithName, name));
            }
        }

        /// <summary>
        /// Gets the activity.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The activity.</returns>
        private Activity GetActivity(IDialogContext context)
        {
            return (Activity)context.Activity;
        }

        /// <summary>
        /// Gets the state client.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The state client.</returns>
        private StateClient GetStateClient(IDialogContext context)
        {
            return GetActivity(context).GetStateClient();
        }

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The user data.</returns>
        private async Task<BotData> GetUserData(IDialogContext context)
        {
            var activity = GetActivity(context);
            var stateClient = GetStateClient(context);
            return await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
        }
    }
}