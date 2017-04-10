using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using X_Bot_First_Class.Common.Models;
using Microsoft.Bot.Builder.Dialogs;

namespace X_Bot_First_Class.Factories
{
    /// <summary>
    /// Implements Applicant information and application state serialization and deserialization to cloud storage
    /// </summary>
    public class ApplicantFactory
    {

        private static CloudTable _t = null;
        private static DateTime _timestamp = DateTime.Now;

        private static CloudTable Table
        {
            get
            {
                if (_t != null) return _t;
                string saConnectionString = ConfigurationManager.AppSettings["StorageAccountConnectionString"];
                CloudStorageAccount sa = CloudStorageAccount.Parse(saConnectionString);
                CloudTableClient tc = sa.CreateCloudTableClient();
                _t = tc.GetTableReference("ApplicantTracking");
                return _t;
            }
        }

        public static async Task<Applicant> GetApplicantByPhone(string phone)
        {
            await Table.CreateIfNotExistsAsync();
            TableOperation to = TableOperation.Retrieve<ApplicantEntity>("X-Bot FirstClass", phone);
            TableResult r = await Table.ExecuteAsync(to);
            if (r.HttpStatusCode != 200 || r.Result == null) return null;

            Applicant a = Applicant.FromJsonString(((ApplicantEntity)r.Result).Data);
            return a;
        }

        public static async Task<Applicant> GetApplicantByEmail(string email)
        {
            await Table.CreateIfNotExistsAsync();

            // this is brute force retrieval and in memory filtering. Very bad, but ok for the purpose here. Unfortunately the 
            // only option with table storage. 
            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<ApplicantEntity> query = new TableQuery<ApplicantEntity>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "X-Bot FirstClass"));

            ApplicantEntity entity = Table.ExecuteQuery(query).FirstOrDefault<ApplicantEntity>(a => Applicant.FromJsonString(a.Data).Email.ToLower() == email.ToLower());
            if (entity == null) return null;
            else
            {
                return Applicant.FromJsonString(entity.Data);
            }
        }

        public static async Task<Applicant> GetApplicantByContext(IDialogContext context)
        {
            Applicant a = null;
            switch (context.Activity.ChannelId)
            {
                case "sms":
                    a = await GetApplicantByPhone(context.Activity.From.Id.TrimStart('+'));
                    break;
                case "skype":
                case "email":
                    a = await GetApplicantByEmail(context.Activity.From.Id);
                    break;
            }
            return a;
        }

        public static async Task PersistApplicant(Applicant a)
        {
            ApplicantEntity entity = new ApplicantEntity();
            entity.Data = a.ToString();
            entity.RowKey = a.Phone;
            await Table.CreateIfNotExistsAsync();
            TableOperation to = TableOperation.InsertOrReplace(entity);
            TableResult r = await Table.ExecuteAsync(to);
        }

    }

    internal class ApplicantEntity : TableEntity
    {
        public ApplicantEntity()
        {
            this.PartitionKey = "X-Bot FirstClass";
        }
        public string Data { get; set; }

    }
}