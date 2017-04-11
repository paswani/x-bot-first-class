using System;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Chronic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using X_Bot_First_Class.Common;
using X_Bot_First_Class.Common.Models;
using X_Bot_First_Class.Factories;
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
            OnCompletionAsyncDelegate<ScheduleInterviewForm> sendCalendarInvite = SendCalendarInvite;

            var a = new FormBuilder<ScheduleInterviewForm>()
                .Message("Ok, got my calendar in front of me.")
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
                            result.Feedback = $"Looks like I'm booked solid then, how about one of these {String.Join(", ", availableDays.Select(x => x.ToString("D")))}";
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
                        result.Feedback = $"Didn't I tell you I'm dyslexic '{value}'. Here's what I have available {string.Join(", ", availableTime.Select(x => x.ToString("t")))}";
                    }
                    else
                    {
                        if (!service.IsTimeAvailable((DateTime)parsed))
                        {
                            var availableTime = service.AvailableTime(state.ChoosenDate);
                            result.IsValid = false;
                            result.Feedback = $"{value} is not available. Here's what I have left: {string.Join(", ", availableTime.Select(x => x.ToString("t")))}";
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
                .Message((state) => Task.FromResult(new PromptAttribute($"Thanks, I have scheduled your appointment for {state.ChoosenDate:D} at {state.Time}. The Express office is located at 9701 Boardwalk, Oklahoma City, OK 73162")))
                //.Message((state) => Task.FromResult(new PromptAttribute($"Please click here to add the invitation to your calendar: http://x-bot-first-class.azurewebsites.net/api/sms/scheduleinterview?{state.ChoosenDate:d}:{state.Time}")))
                .OnCompletion(sendCalendarInvite)
                .Build();
            return a;
        }

        private static async Task SendCalendarInvite(IDialogContext context, ScheduleInterviewForm state)
        {
            Applicant a = null;
            context.UserData.TryGetValue<Applicant>("applicant", out a);
            var applicationKey = a.Applications.Keys.First();
            a.Applications[applicationKey].Interview = state.ChoosenDate;
            a.Applications[applicationKey].State = ConversationType.None;            
            await ApplicantFactory.PersistApplicant(a);
            await context.PostAsync($"Please click here to add the invitation to your calendar: http://x-bot-first-class.azurewebsites.net/api/ics?apptDateTime={state.ChoosenDate:s}");
            
            context.Done<Applicant>(a);
        }
    }
}