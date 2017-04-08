using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Factories;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Common.Models;

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

            // attempt to obtain applicant info
            Applicant a = null;
            switch (context.Activity.ChannelId)
            {
                case "sms":
                    a = await ApplicantFactory.GetApplicantByPhone(context.Activity.From.Id);
                    break;
                case "skype":
                case "email":
                    a = await ApplicantFactory.GetApplicantByEmail(context.Activity.From.Id);
                    break;
                case "emulator":
                    a = await ApplicantFactory.GetApplicantByPhone("1-504-813-3964");
                    break;
                default:
                    throw new Exception($"Unsupported channel type {context.Activity.ChannelId} in Root Dialog.");
            }
            if (a == null) {
                a = new Applicant();
                switch (context.Activity.ChannelId)
                {
                    case "sms":
                        a.Phone = context.Activity.From.Id;
                        break;
                    case "skype":
                    case "email":
                        a.Email = context.Activity.From.Id;
                        a.Phone = "1-713-000-0000";
                        break;
                }
                // need logic at some point to obtain phone number when in skype/email channel and email when in SMS channel
            }
            if (conversationType == ConversationType.None && a.Applications.Count > 0) {
                conversationType = a.Applications.First().Value.State;
            }

            var factory = new LuisDialogFactory();
            var dialog = await factory.Create(result.Query, a, conversationType);

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
            if (string.IsNullOrEmpty(message?.ToString()))
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