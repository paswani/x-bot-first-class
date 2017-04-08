using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace X_Bot_First_Class.Common.Models
{
    /// <summary>
    /// represent information about an applicant and assist in applicant tracking throughout the process
    /// </summary>
    public class Applicant
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Dictionary<string, Application> Applications { get; private set; }
        public W4Info W4Info { get; set; }
        public Applicant() {
            this.Applications = new Dictionary<string, Application>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string ToJsonString(Applicant a)
        {
            return a.ToString();
        }

        public static Applicant FromJsonString(string s)
        {
            return JsonConvert.DeserializeObject<Applicant>(s);
        }
    }
}
