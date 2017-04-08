using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Bot_First_Class.Common.Models
{
    /// <summary>
    /// Captures Applicants W4 Data
    /// </summary>
    public class W4Info
    {
        public string FirstName { get; set; }
        public string LstName { get; set; }
        public string SSN { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set;  }
        public MariageStatus MariageStatus { get; set; }
        public int Allowances { get; set; }
        public int AdditionalAmountToWithold { get; set; }
        public Boolean FirstNameDiffers { get; set;  }
        public Boolean Exempt { get; set; }
    }

    public enum MariageStatus
    {
        Single = 0, 
        Married = 1,
        MarriedButWitholdSingle = 2
    }
}
