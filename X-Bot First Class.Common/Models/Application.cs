using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Bot_First_Class.Common.Models
{
    /// <summary>
    /// Represents a a Job Application
    /// </summary>
    public class Application
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public ConversationType State { get; set; }
        public DateTime? Applied { get; set; }
        public DateTime? Interview { get; set; }
        public DateTime? Accepted { get; set; }
        public DateTime? Starting { get; set; }
        public List<Sentiment> SentimentData { get; set; }
        public Recruiter Recrutier { get; set; }

        public Application()
        {
            this.SentimentData = new List<Sentiment>();
        }
    }
}
