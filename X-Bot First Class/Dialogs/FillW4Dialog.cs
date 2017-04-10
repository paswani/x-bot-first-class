using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using X_Bot_First_Class.Common.Models;
using X_Bot_First_Class.Factories;
using iTextSharp.text.pdf;


namespace X_Bot_First_Class.Dialogs
{
    /// <summary>
    /// Fill W4 dialog class
    /// </summary>
    public class FillW4Dialog
    {
        public static IForm<W4Info> BuildForm()
        {
            OnCompletionAsyncDelegate<W4Info> processW4 = async (context, state) =>
            {
                Applicant a = null;
                context.UserData.TryGetValue<Applicant>("applicant", out a);
                a.W4Info = state;
                await ApplicantFactory.PersistApplicant(a);
                await context.PostAsync("Please hold a moment while I create your form...");

                FillW4Dialog.FillForm(a);
        
                await context.PostAsync("Please save this form for future reference! Thanks and I hope we'll talk again soon.");
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
                        Task<ValidateResult> task = Task.Factory.StartNew(() => {
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
                        Task<ValidateResult> task = Task.Factory.StartNew(() => {
                            var r = new ValidateResult { IsValid = false, Value = value };
                            Regex regex = new Regex(@"^(?!219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}-(?!00)\d{2}-(?!0{4})\d{4}$", RegexOptions.IgnoreCase);
                            if (regex.IsMatch((string)value))
                            {
                                r.IsValid = true;
                                r.Feedback = "Your social security number has been validated with DHS e-Verify.";
                            }
                            else
                            {
                                r.Feedback = "Your social security number has am invalid format. Please correct and try again.";
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
                        Task<ValidateResult> task = Task.Factory.StartNew(() => {
                            if (value == null) value = 0;
                            var r = new ValidateResult { IsValid = true, Value = value };
                            if ((Int64)value > 0) r.Feedback = $"Ok, we'll withold an additional ${value} from each paycheck.";
                            return r;
                        });
                        var result = await task;
                        return result;
                    })
                )
                .Confirm("Please review your your data")
                .Confirm(async (state) => {
                    Task<PromptAttribute> task = Task.Factory.StartNew(() => {
                        PromptAttribute attr = new PromptAttribute($"I have captured: {state.ToString()}. Is this data correct? {{||}}");
                        return attr;
                    });
                    var result = await task;
                    return result;
                })
                .Message("Thanks for helping me to get this information captured!")
                .OnCompletion(processW4)
                .Build();
            return b;
        }

        public static IDialog<W4Info> MakeW4Dialog(Applicant a)
        {
            IDialog<W4Info> d = Chain.From(() => FormDialog.FromForm(FillW4Dialog.BuildForm));
            return d;
        }

        private static void FillForm(Applicant a)
        {
            Stream w4Filled = new MemoryStream();
            PdfReader pdfReader = new PdfReader(new Uri("https://www.irs.gov/pub/irs-pdf/fw4.pdf"));
            PdfStamper pdfStamper = new PdfStamper(pdfReader, w4Filled);
            AcroFields pdfFormFields = pdfStamper.AcroFields;
            // set form pdfFormFields
            // The first worksheet and W-4 form

            //pdfFormFields.SetField("f1_01(0)", "1");
            //pdfFormFields.SetField("f1_02(0)", "1");
            //pdfFormFields.SetField("f1_03(0)", "1");
            //pdfFormFields.SetField("f1_04(0)", "8");
            //pdfFormFields.SetField("f1_05(0)", "0");
            //pdfFormFields.SetField("f1_06(0)", "1");
            //pdfFormFields.SetField("f1_07(0)", "16");
            //pdfFormFields.SetField("f1_08(0)", "28");
            pdfFormFields.SetField("f1_09(0)", a.W4Info.FirstName);
            pdfFormFields.SetField("f1_10(0)", a.W4Info.LastName);
            pdfFormFields.SetField("f1_11(0)", "532");
            pdfFormFields.SetField("f1_12(0)", "12");
            pdfFormFields.SetField("f1_13(0)", "1234");
            // The form's checkboxes
            pdfFormFields.SetField("c1_01(0)", a.W4Info.MariageStatus == MariageStatus.Single ? "Yes" : "0");
            pdfFormFields.SetField("c1_02(0)", a.W4Info.MariageStatus == MariageStatus.Married ? "Yes" : "0");
            pdfFormFields.SetField("c1_03(0)", a.W4Info.MariageStatus == MariageStatus.MarriedWitholdingSingle ? "Yes" : "0");
            pdfFormFields.SetField("c1_04(0)", a.W4Info.FirstNameDiffers == true ? "Yes" : "0");
            // The rest of the form pdfFormFields
            pdfFormFields.SetField("f1_14(0)", a.W4Info.Street);
            pdfFormFields.SetField("f1_15(0)", a.W4Info.City);
            pdfFormFields.SetField("f1_16(0)", "9");
            pdfFormFields.SetField("f1_17(0)", "10");
            pdfFormFields.SetField("f1_18(0)", "11");
            pdfFormFields.SetField("f1_19(0)", "Infusion");
            pdfFormFields.SetField("f1_20(0)", "Inf666");
            pdfFormFields.SetField("f1_21(0)", "AB");
            pdfFormFields.SetField("f1_22(0)", "4321");
            // Second Worksheets pdfFormFields
            // In order to map the fields, I just pass them a sequential
            // number to mark them; once I know which field is which, I 
            // can pass the appropriate value
            //pdfFormFields.SetField("f2_01(0)", "1");
            //pdfFormFields.SetField("f2_02(0)", "2");
            //pdfFormFields.SetField("f2_03(0)", "3");
            //pdfFormFields.SetField("f2_04(0)", "4");
            //pdfFormFields.SetField("f2_05(0)", "5");
            //pdfFormFields.SetField("f2_06(0)", "6");
            //pdfFormFields.SetField("f2_07(0)", "7");
            //pdfFormFields.SetField("f2_08(0)", "8");
            //pdfFormFields.SetField("f2_09(0)", "9");
            //pdfFormFields.SetField("f2_10(0)", "10");
            //pdfFormFields.SetField("f2_11(0)", "11");
            //pdfFormFields.SetField("f2_12(0)", "12");
            //pdfFormFields.SetField("f2_13(0)", "13");
            //pdfFormFields.SetField("f2_14(0)", "14");
            //pdfFormFields.SetField("f2_15(0)", "15");
            //pdfFormFields.SetField("f2_16(0)", "16");
            //pdfFormFields.SetField("f2_17(0)", "17");
            //pdfFormFields.SetField("f2_18(0)", "18");
            //pdfFormFields.SetField("f2_19(0)", "19");

            // flatten the form to remove editting options, set it to false
            // to leave the form open to subsequent manual edits
            pdfStamper.FormFlattening = true;
            // close the pdf

            pdfStamper.Close();

            string saConnectionString = System.Configuration.ConfigurationManager.AppSettings["StorageAccountConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(saConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(a.W4Info.SSN.Replace("-", ""));
            container.CreateIfNotExists();

            // temporary, remove later
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CloudBlockBlob blob = container.GetBlockBlobReference("w4.pdf");
            blob.UploadFromStream(w4Filled);
            w4Filled.Dispose();

        }
    }
}