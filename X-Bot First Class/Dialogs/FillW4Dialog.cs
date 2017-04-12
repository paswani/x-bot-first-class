using iTextSharp.text.pdf;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using X_Bot_First_Class.Common.Models;
using X_Bot_First_Class.Factories;

namespace X_Bot_First_Class.Dialogs
{

    /// <summary>
    /// Fill W4 dialog class
    /// </summary>
    public class FillW4Dialog
    {
        /// <summary>
        /// Builds the conversation form. 
        /// </summary>
        /// <returns>IForm instance.</returns>
        public static IForm<W4Info> BuildForm()
        {
            OnCompletionAsyncDelegate<W4Info> processW4 = async (context, state) =>
            {
                Applicant a = null;
                context.UserData.TryGetValue<Applicant>("applicant", out a);
                if (a == null) throw new ArgumentNullException("applicant");
                a.W4Info = state;
                a.Applications.First().Value.State = Common.ConversationType.None;
                context.UserData.SetValue<Applicant>("applicant", a);
                await ApplicantFactory.PersistApplicant(a);
                await context.PostAsync("Please hold a moment while I create your form...");

                string W4Uri = FillW4Dialog.FillForm(a);
                //IMessageActivity m = context.MakeMessage();
                //m.Attachments.Add(new Attachment()
                //{
                //    ContentUrl = W4Uri,
                //    ContentType = "application/pdf",
                //    Name = "w4.pdf"
                //});
                //m.Text = "Please save this form for future reference!Thanks and I hope we'll talk again soon.";
                //try
                //{
                //    await context.PostAsync(m);
                //}
                //catch (Exception ex) {
                //    Exception e = ex;
                //    throw;
                //}
                await context.PostAsync($"You can find your W4 at {W4Uri}. Please save this form for future reference!Thanks and I hope we'll talk again soon.");
                context.Done<Applicant>(a);
            };

            IForm<W4Info> b = new FormBuilder<W4Info>()
                .Message($"Ok, let's fill out the W4 form. I'll ask you some question, simply type the answers and I'll assemble the form.")
                .Message("We'll start by confirming some basic information about you.")
                .Field(nameof(W4Info.FirstName))
                .Field(nameof(W4Info.LastName))
                .Field(nameof(W4Info.FirstNameDiffers),
                    validate: async (state, value) =>
                    {
                        Task<ValidateResult> task = Task.Factory.StartNew(() =>
                        {
                            if (value == null) value = false;
                            var r = new ValidateResult { IsValid = true, Value = value };
                            if ((bool)value == true) r.Feedback = "Ok, we can proceed, but you must call 1-800-772-1213 to obtain a replacement social security card with your correct legal name.";
                            return r;
                        });
                        var result = await task;
                        return result;
                    })
                .Field(nameof(W4Info.Street))
                .Field(nameof(W4Info.City))
                .Field(nameof(W4Info.SSN),
                    validate: async (state, value) =>
                    {
                        Task<ValidateResult> task = Task.Factory.StartNew(() =>
                        {
                            var r = new ValidateResult { IsValid = false, Value = value };
                            Regex regex = new Regex(@"^(?!219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}-(?!00)\d{2}-(?!0{4})\d{4}$", RegexOptions.IgnoreCase);
                            if (regex.IsMatch((string)value))
                            {
                                r.IsValid = true;
                                r.Feedback = "Your social security number has been validated with DHS e-Verify.";
                            }
                            else
                            {
                                r.Feedback = "Your social security number has an invalid format. Please correct and try again.";
                            }
                            // make call to DHS employee validation endpoint
                            // this is just fake.
                            // add some delay to simulate elapsed time
                            Task.Delay(1500);
                            return r;
                        });
                        var result = await task;
                        return result;
                    })
                .Field(nameof(W4Info.Exempt))
                .Field(new FieldReflector<W4Info>(nameof(W4Info.MariageStatus))
                    .SetActive((s) => !s.Exempt)
                )
                .Field(new FieldReflector<W4Info>(nameof(W4Info.Allowances))
                    .SetActive((s) => !s.Exempt)
                )
                .Field(new FieldReflector<W4Info>(nameof(W4Info.AdditionalAmountToWithold))
                    .SetActive((s) => !s.Exempt)
                    .SetValidate(async (state, value) =>
                    {
                        Task<ValidateResult> task = Task.Factory.StartNew(() =>
                        {
                            if (value == null) value = 0;
                            var r = new ValidateResult { IsValid = true, Value = value };
                            if ((Int64)value > 0) r.Feedback = $"Ok, we'll withhold an additional ${value} from each paycheck.";
                            return r;
                        });
                        var result = await task;
                        return result;
                    })
                )
                .Confirm("Please review the data before I proceed. I have captured: {*} \r\n Is this correct?")
                .Message("Thanks for helping me to get this information captured!")
                .OnCompletion(processW4)
                .Build();
            return b;
        }

        /// <summary>
        /// Generates the Dialog.
        /// </summary>
        /// <param name="a">Applicant Info</param>
        /// <returns>IDialog instance</returns>
        public static IDialog<W4Info> MakeW4Dialog(Applicant a)
        {
            // we are implementing this as a stand alone dialog. If you want to chain with other dialogs, switch the below...
            // IDialog<W4Info> d = Chain.From(() => FormDialog.FromForm(FillW4Dialog.BuildForm));
            IDialog<W4Info> d = FormDialog.FromForm(FillW4Dialog.BuildForm);
            return d;
        }

        /// <summary>
        /// Generate PDF and put into Blob storage
        /// </summary>
        /// <param name="a">Applicant information</param>
        /// <returns>Url to completed PDF</returns>
        private static string FillForm(Applicant a)
        {
            string url = string.Empty;
            PdfReader pdfReader = new PdfReader(new Uri("https://www.irs.gov/pub/irs-pdf/fw4.pdf"));

            // enumerate form fields to figure out which is which
            // StringBuilder sb = new StringBuilder();
            // foreach (System.Collections.Generic.KeyValuePair<string, AcroFields.Item> de in pdfReader.AcroFields.Fields)
            // {
            //    sb.Append(de.Key.ToString() + Environment.NewLine);
            // }
            // Console.WriteLine(sb.ToString());


            using (MemoryStream w4Filled = new MemoryStream())
            {
                using (PdfStamper pdfStamper = new PdfStamper(pdfReader, w4Filled))
                {
                    AcroFields pdfFormFields = pdfStamper.AcroFields;
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].Line1[0].f1_09_0_[0]", a.W4Info.FirstName);
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].Line1[0].f1_10_0_[0]", a.W4Info.LastName);
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].Line1[0].f1_11_0_[0]", a.W4Info.Street);
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].Line1[0].f1_12_0_[0]", a.W4Info.City);
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].f1_13_0_[0]", a.W4Info.SSN);
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].c1_01[0]", ((int)a.W4Info.MariageStatus).ToString());
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].c1_01[1]", ((int)a.W4Info.MariageStatus).ToString());
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].c1_01[2]", ((int)a.W4Info.MariageStatus).ToString());
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].c1_02[0]", a.W4Info.FirstNameDiffers == true ? "1" : "0");
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].f1_14_0_[0]", a.W4Info.Exempt ? "" : a.W4Info.Allowances.ToString());
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].f1_15_0_[0]", a.W4Info.Exempt ? "" : a.W4Info.AdditionalAmountToWithold.ToString());
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].f1_16_0_[0]", a.W4Info.Exempt ? "Exempt" : "");
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].f1_17_0_[0]", "Infusion Development LLC");
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].f1_18_0_[0]", "456ZZx45");
                    pdfFormFields.SetField("topmostSubform[0].Page1[0].f1_19_0_[0]", "86-34413242");

                    // flatten the form to remove editting options, set it to false
                    // to leave the form open to subsequent manual edits
                    pdfStamper.FormFlattening = false;
                }
                byte[] arr = w4Filled.ToArray();
     
                string saConnectionString = System.Configuration.ConfigurationManager.AppSettings["StorageAccountConnectionString"];
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(saConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(a.W4Info.SSN.Replace("-", ""));
                container.CreateIfNotExists();

                // temporary, remove later
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                CloudBlockBlob blob = container.GetBlockBlobReference("w4.pdf");
                blob.UploadFromByteArray(arr, 0, arr.Length);
                url = blob.Uri.ToString();

            }
            return url;
        }
    }
}