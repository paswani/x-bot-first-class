using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Factories;

namespace X_Bot_First_Class.Dialogs
{
    /// <summary>
    /// Welcome dialog class
    /// </summary>
    [LuisModel("660d5761-1f2f-4e2c-8d30-e937614ea94f", "f209e6595d014a54b02e51b08d3b9c6d")]
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
            // attempt to obtain applicant info
            var a = await ApplicantFactory.GetApplicantByContext(context);

            if (string.IsNullOrEmpty(a.Name))
            {
                // prompt for the user's name
                PromptDialog.Text(context, ResumeAfterNamePromptAsync, Resources.msgWelcome);
            }
            else
            {
                await context.PostAsync(string.Format(Resources.msgWelcomeBack, a.Name));

                context.Done<string>(null);
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

                // attempt to save applicant info
                var a = await ApplicantFactory.GetApplicantByContext(context);
                a.Name = name;
                await ApplicantFactory.PersistApplicant(a);

                await context.PostAsync(string.Format(Resources.msgIWillCallYou, name));

                context.Done<string>(null);
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

            context.Done<string>(null);
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
                // attempt to save applicant info
                var a = await ApplicantFactory.GetApplicantByContext(context);
                a.Name = name;
                await ApplicantFactory.PersistApplicant(a);

                await context.PostAsync(string.Format(Resources.mgsWelcomeWithName, name));

                context.Done<string>(null);
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
                // attempt to save applicant info
                var a = await ApplicantFactory.GetApplicantByContext(context);
                a.Name = name;
                await ApplicantFactory.PersistApplicant(a);

                await context.PostAsync(string.Format(Resources.msgIWillCallYou, name));

                context.Done<string>(null);
            }
        }
    }
}