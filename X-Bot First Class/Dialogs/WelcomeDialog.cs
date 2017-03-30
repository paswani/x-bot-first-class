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
    /// Welcome dialog class
    /// </summary>
    [LuisModel("660d5761-1f2f-4e2c-8d30-e937614ea94f", "08107661360644ab8532afffc187bbac")]
    [Serializable]
    public class WelcomeDialog : LuisDialogBase<object>
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
            var name = string.Empty;
            context.UserData.TryGetValue<string>("name", out name);

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
        /// Change Name intent.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        [LuisIntent("ChangeName")]
        public async Task ChangeName(IDialogContext context, LuisResult result)
        {
            List<EntityRecommendation> nameEntities = null;
            if (result.TryFindEntities("Name", out nameEntities))
            {
                var name = "";
                var nameList = nameEntities.ConcatEntities(" ").Split(' ').ToList<string>();
                nameList.ForEach(str =>
                {
                    name += string.Concat(str[0].ToString().ToUpper(), str.Substring(1), " ");
                });
                name = name.Trim();

                context.UserData.SetValue<string>("name", name);
                await context.PostAsync(string.Format(Resources.msgIWillCallYou, name));
            }
            else
            {
                // prompt for the user's name
                PromptDialog.Text(context, ResumeAfterNameChangeAsync, Resources.msgWhatShouldICallYou);
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
            await context.PostAsync(Resources.msgGoodbye);
        }

        /// <summary>
        /// Callback for name prompt.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task ResumeAfterNamePromptAsync(IDialogContext context, IAwaitable<string> result)
        {
            var name = await result;
            if (!string.IsNullOrEmpty(name))
            {
                // persist the data for the current user
                context.UserData.SetValue<string>("name", name);
                await context.PostAsync(string.Format(Resources.mgsWelcomeWithName, name));
            }
        }

        /// <summary>
        /// Callback for name change.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task ResumeAfterNameChangeAsync(IDialogContext context, IAwaitable<string> result)
        {
            var name = await result;
            if (!string.IsNullOrEmpty(name))
            {
                // persist the data for the current user
                context.UserData.SetValue<string>("name", name);
                await context.PostAsync(string.Format(Resources.msgIWillCallYou, name));
            }
        }
    }
}