using System;
using System.Linq;
using System.Threading.Tasks;
using Chronic;
using Microsoft.Bot.Builder.FormFlow;
using X_Bot_First_Class.Services;

namespace X_Bot_First_Class.Dialogs
{
    [Serializable]
    public class ScheduleInterviewForm
    {
        private DateTime ChoosenDate;

        public string Date;

        public string Time;

        public static IForm<ScheduleInterviewForm> BuildForm()
        {
            var a = new FormBuilder<ScheduleInterviewForm>()
                .Message("Hi, let me pull up my calendar so I can schedule an interview for you.")
                .Field(nameof(Date), validate: (state, value) =>
                {
                    var service = new CalendarService();
                    var result = new ValidateResult() { IsValid = true, Value = value };
                    var parsed = new Parser().Parse((string)value).Start;
                    if (parsed == null)
                    {
                        var availableDays = service.AvailableDays();
                        result.IsValid = false;
                        result.Feedback = $"I think my dyslexia is acting up, I don't understand {value}.  What about {String.Join(",", availableDays.Select(x => x.ToString("D")))}";
                    }
                    else
                    {
                        if (!service.IsDateAvailable(parsed.Value))
                        {
                            var availableDays = service.AvailableDays();
                            result.IsValid = false;
                            result.Feedback = $"Looks like I'm booked up, how about {String.Join(",", availableDays.Select(x => x.ToString("D")))}";
                        }
                        else
                        {
                            state.ChoosenDate = parsed.Value;
                            result.Value = state.ChoosenDate.ToString("D");
                        }
                    }
                    return Task.FromResult(result);
                })
                .Confirm((state) => Task.FromResult(new PromptAttribute($"Is that {state.Date}")))
                .Field(nameof(Time), validate: (state, value) =>
                {
                    var result = new ValidateResult() { IsValid = true, Value = value };
                    var parsed = new Parser().Parse((string)value).Start;
                    var service = new CalendarService();

                    if (parsed == null)
                    {
                        var availableTime = service.AvailableTime(state.ChoosenDate);
                        result.IsValid = false;
                        result.Feedback = $"I don't understand '{value}'. Please be more precise. A few options are {string.Join(", ", availableTime.Select(x => x.ToString("t")))}";
                    }
                    else
                    {
                        if (!service.IsTimeAvailable((DateTime)parsed))
                        {
                            var availableTime = service.AvailableTime(state.ChoosenDate);
                            result.IsValid = false;
                            result.Feedback = $"{value} is not available. Options are {string.Join(", ", availableTime.Select(x => x.ToString("t")))}";
                        }
                        else
                        {
                            result.Value = ((DateTime)parsed).ToString("t");
                        }
                    }
                    return Task.FromResult(result);
                })
                //.AddRemainingFields()
                .Confirm((state) => Task.FromResult(new PromptAttribute($"We're almost there, {state.ChoosenDate:D} at {state.Time}?  Right?")))
                .Message((state) => Task.FromResult(new PromptAttribute( $"Thanks, I have scheduled your appointment for {state.ChoosenDate:D} at {state.Time}. The Express office is located at 9701 Boardwalk, Oklahoma City, OK 73162")))
                .Build();
            return a;
        }
    }
}