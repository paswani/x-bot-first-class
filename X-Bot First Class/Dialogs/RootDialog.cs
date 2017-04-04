using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;

namespace X_Bot_First_Class.Dialogs
{
    /// <summary>
    /// Root dialog class
    /// </summary>
    [LuisModel("e82853ef-0ac6-4124-9e80-3e71057382a2", "08107661360644ab8532afffc187bbac")]
    [Serializable]
    public class RootDialog : LuisDialogBase<object>
    {
        private string _userToBot;
        private bool _serviceUrlSet = false;

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
            ConversationType conversationType;
            context.UserData.TryGetValue<ConversationType>("conversationType", out conversationType);

            var factory = new LuisDialogFactory();
            var dialog = await factory.Create(result.Query, conversationType);
            await context.PostAsync(conversationType.ToString());
            if (dialog != null)
            {
                var message = context.MakeMessage();
                message.Text = _userToBot;

                await context.Forward(dialog, ResumeAfterForward, message, CancellationToken.None);
            }
            else
            {
                await context.PostAsync(Resources.msgIDidntCatchThat);
                context.Wait(MessageReceived);
            }
        }

        /// <summary>
        /// Resumes the after forward.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task ResumeAfterForward(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;
            if (string.IsNullOrEmpty(message.ToString()))
            {
                context.Wait(MessageReceived);
            }
            else
            {
                await None(context, new LuisResult()); //the second dialog didn't understand the command
            }
        }

        /// <summary>
        /// Messages the received.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            //No way to get the message in the LuisIntent methods so saving it here
            _userToBot = message.Text.ToLowerInvariant();

            if (!_serviceUrlSet)
            {
                context.PrivateConversationData.SetValue("ServiceUrl", message.ServiceUrl);
                _serviceUrlSet = true;
            }

            await base.MessageReceived(context, item);
        }
    }
}