using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Bot_First_Class.Common.Models
{
    /// <summary>
    /// Captures Sentiment
    /// </summary>
    public class Sentiment
    {
        public DateTime SentimentTaken { get; set; }
        public double SentimentScore { get; set; }
        public Boolean AdvisorContacted { get; set; }
    }
}
