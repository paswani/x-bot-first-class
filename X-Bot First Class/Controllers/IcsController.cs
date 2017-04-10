using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace X_Bot_First_Class.Controllers
{
    public class IcsController : ApiController
    {
        // GET: api/Ics
        public HttpResponseMessage Get(DateTime apptDateTime)
        {                        
            // replace db with however you call your Entity Framework or however you get your data.
            // In this example, we have an Events collection in our model.
                
            var icalStringbuilder = new StringBuilder();

            icalStringbuilder.AppendLine("BEGIN:VCALENDAR");
            icalStringbuilder.AppendLine("PRODID:-//MyTestProject//EN");
            icalStringbuilder.AppendLine("VERSION:2.0");

            icalStringbuilder.AppendLine("BEGIN:VEVENT");
            icalStringbuilder.AppendLine("SUMMARY;LANGUAGE=en-us: Express Interview");
            icalStringbuilder.AppendLine("CLASS:PUBLIC");
            icalStringbuilder.AppendLine($"CREATED:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            icalStringbuilder.AppendLine("DESCRIPTION: Express Interview @ Headquarters");
            icalStringbuilder.AppendLine($"DTSTART:{apptDateTime:yyyyMMddTHHmmssZ}");
            icalStringbuilder.AppendLine($"DTEND:{apptDateTime.AddHours(1):yyyyMMddTHHmmssZ}");
            icalStringbuilder.AppendLine("SEQUENCE:0");
            icalStringbuilder.AppendLine("UID:" + Guid.NewGuid());
            icalStringbuilder.AppendLine(
                $"LOCATION: Express Headquarters\\, 9701 Boardwalk\\, Oklahoma City\\, OK 73162".Trim());
            icalStringbuilder.AppendLine("END:VEVENT");
            icalStringbuilder.AppendLine("END:VCALENDAR");


            var bytes = Encoding.UTF8.GetBytes(icalStringbuilder.ToString());

            var ms = new MemoryStream(bytes);

            var resp = new HttpResponseMessage()
            {
                Content = new StreamContent(ms)
            };

            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("text/calendar");

            return resp;                            
        }
    }
}
