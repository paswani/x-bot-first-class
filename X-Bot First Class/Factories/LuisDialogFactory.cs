using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Common.Models;
using X_Bot_First_Class.Dialogs;

namespace X_Bot_First_Class
{
    /// <summary>
    /// Public class to create luis dialogs
    /// </summary>
    public class LuisDialogFactory
    {
        /// <summary>
        /// The dialogs
        /// </summary>
        private static List<LuisDialogBase<object>> _dialogs = null;

        /// <summary>
        /// Gets the dialogs.
        /// </summary>
        /// <value>
        /// The dialogs.
        /// </value>
        private static List<LuisDialogBase<object>> Dialogs
        {
            get
            {
                if (_dialogs == null)
                {
                    EnsureDialogs();
                }
                return _dialogs;
            }
        }

        /// <summary>
        /// Creates the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="conversationType">Type of the conversation.</param>
        /// <returns></returns>
        public async Task<IDialog<object>> Create(string query, Applicant a, ConversationType conversationType)
        {
            EnsureDialogs();
            switch (conversationType)
            {
                case ConversationType.FirstDayReview:
                    return Dialogs.FirstOrDefault(dialog => dialog is FirstDayReviewDialog);
                case ConversationType.FillOutW4:
                    return FillW4Dialog.MakeW4Dialog(a);
                case ConversationType.RejectionNotice:
                    return Dialogs.FirstOrDefault(dialog => dialog is RejectionNoticeDialog);
                case ConversationType.ScheduleInterview:
                default:
                    query = query.ToLowerInvariant();

                    foreach (var resourceDialog in Dialogs)
                    {
                        if (await resourceDialog.CanHandle(query))
                        {
                            return resourceDialog;
                        }
                    }
                    return null;
            }
        }

        /// <summary>
        /// Ensures the dialogs.
        /// </summary>
        private static void EnsureDialogs()
        {
            _dialogs = new List<LuisDialogBase<object>>();
            var types = typeof(LuisDialogFactory).Assembly.GetTypes().Where(type => type.IsClass && type.BaseType == typeof(LuisDialogBase<object>)).ToList();
            types.ForEach(type =>
            {
                _dialogs.Add((LuisDialogBase<object>)Activator.CreateInstance(type));
            });
        }
    }
}
