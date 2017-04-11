using System;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Bot_First_Class.Common.Models
{
    /// <summary>
    /// Captures Applicants W4 Data
    /// </summary>
    [Serializable]
    public class W4Info
    {
        [Prompt("Please enter your legal first name.")]
        public string FirstName { get; set; }
        [Prompt("Please enter your legal last name.")]
        public string LastName { get; set; }
        [Prompt("Please enter your social security number.")]
        public string SSN { get; set; }
        [Prompt("Please enter your street address.")]
        public string Street { get; set; }
        [Prompt("Please enter your city, State and Zip.")]
        public string City { get; set; }
        [Prompt("Please select your mariage status. If you are married, you might choose to withold on the higher, Single status if you have other income. {||}", ChoiceFormat = "\n({0}) - {1}", ChoiceStyle=ChoiceStyleOptions.PerLine)]
        public MariageStatus MariageStatus { get; set; }
        [Numeric(0, 10)]
        [Prompt ("How many allowances do want to claim. Generally, you can claim an allowance for yourself, one for your spouse and one for each of your dependent children.")]
        public int Allowances { get; set; }
        [Numeric(0,1000000)]
        [Prompt("Do you want to withold and additional amount each paycheck? This might be useful if you have additional income outside of this position to avoid a year end tax liability. If you want to withold additionally, please simply enter the amount.")]
        [Optional]
        [Template(TemplateUsage.NoPreference, "0")]
        public int? AdditionalAmountToWithold { get; set; }
        [Prompt("Does your legal name differ from the one shown on your Social Security Card?")]
        [Optional]
        public Boolean? FirstNameDiffers { get; set;  }
        [Prompt("Do you think you are exempt from federal withholdings? Please answer 'yes' or 'no'. Generally, you are excempt if you did not have a federal tax liability last year and do not expect to have a tax liability this year.")]
        [Template(TemplateUsage.NotUnderstood, "Hmm, this did not seem to work. You need to answer yes or no. Type 'yes' if you are excempt, 'no' otherwise.", "Something is stil not right. Try again: 'yes' for excempt, 'no' otherwise.")]
        [Template(TemplateUsage.NoPreference, "no")]
        public Boolean Exempt { get; set; }
    }

    public enum MariageStatus
    {
        None = 0,
        [Terms("single", "1")]
        Single = 1, 
        [Terms("married", "2")]
        Married = 2,
        [Terms("withold", "married witholding single", "3")]
        MarriedWitholdingSingle = 3
    }
}
