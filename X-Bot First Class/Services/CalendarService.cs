using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Bot_First_Class.Services
{
    public interface ICalendarService
    {
        bool IsDateAvailable(DateTime date);
        bool IsTimeAvailable(DateTime date);

        IEnumerable<DateTime> AvailableDays();

        IEnumerable<DateTime> AvailableTime(DateTime date);
    }

    public class CalendarService : ICalendarService
    {
        public IEnumerable<DateTime> AvailableDays()
        {
            var current = DateTime.UtcNow.Day % 2 == 0 ? DateTime.UtcNow : DateTime.UtcNow.AddDays(1);
            var days = new List<DateTime>()
            {
                current.AddDays(2),
                current.AddDays(4),
                current.AddDays(6)
            };
            return days;
        }

        public IEnumerable<DateTime> AvailableTime(DateTime date)
        {
            var times = new List<DateTime>()
            {
                date + new TimeSpan(10, 30, 0),
                date + new TimeSpan(14, 0, 0),
                date + new TimeSpan(15, 0, 0)
            };
            return times;
        }

        public bool IsDateAvailable(DateTime date)
        {
            return date > DateTime.UtcNow && date.Day % 2 == 0;
        }

        public bool IsTimeAvailable(DateTime date)
        {
            return date != null && (date.Hour == 10 || date.Hour == 14 || date.Hour == 15);
        }
    }
}