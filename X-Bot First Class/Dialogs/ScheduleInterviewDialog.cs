using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Factories;

namespace X_Bot_First_Class.Dialogs
{
    public class ScheduleInterviewDialog
    {
        internal static IDialog<ScheduleInterviewForm> MakeScheduleInterviewDialog()
        {
            return Chain.From(() => FormDialog.FromForm(ScheduleInterviewForm.BuildForm));
        }
    }
}