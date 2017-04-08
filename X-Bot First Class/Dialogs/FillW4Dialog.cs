using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using WildMouse.Unearth.Cognitive.TextAnalytics;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Common.Models;

namespace X_Bot_First_Class.Dialogs
{
    /// <summary>
    /// Fill W4 dialog class
    /// </summary>
    public class FillW4Dialog
    {
        public static IForm<W4Info> BuildForm()
        {
            return new FormBuilder<W4Info>()
                .Message($"Ok, let's fill out the W4 form. I'll ask you some question, simply type the answers and I'll assemble the form")
                .Build();
        }

        public static IDialog<W4Info> MakeW4Dialog(Applicant a)
        {
            return Chain.From(() => FormDialog.FromForm(FillW4Dialog.BuildForm));
        }
    }
}