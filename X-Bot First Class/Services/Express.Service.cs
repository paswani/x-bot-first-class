using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using X_Bot_First_Class.Common;

namespace X_Bot_First_Class.Services
{
    /// <summary>
    /// Public class representing the Express API service.
    /// </summary>
    public class Express
    {
        /// <summary>
        /// Base API URL
        /// </summary>
        private const string ApiUrl = "http://devinf-express-mobileapi.azurewebsites.net";

        /// <summary>
        /// Performs a post to the Express API.
        /// </summary>
        /// <typeparam name="T">Type to de-serialize the resulting json into.</typeparam>
        /// <param name="endpoint">The endpoint to hit. Do not include the base API URL</param>
        /// <param name="data">The data.</param>
        /// <returns>De-serialized data.</returns>
        public static T PostData<T>(string endpoint, object data)
        {
            var httpClient = GetHttpClient();
            var response = httpClient.PostAsync(string.Concat(ApiUrl, endpoint), new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
            var result = response.Result.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// Performs a GET to the Express API.
        /// </summary>
        /// <typeparam name="T">Type to de-serialize the resulting json into.</typeparam>
        /// <param name="endpoint">The endpoint to hit. Do not include the base API URL</param>
        /// <returns>De-serialized data.</returns>
        public static T GetData<T>(string endpoint)
        {
            var httpClient = GetHttpClient();
            var response = httpClient.GetAsync(string.Concat(ApiUrl, endpoint));
            var result = response.Result.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <returns></returns>
        private static HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("DeviceId", "123");
            return httpClient;
        }

        /// <summary>
        /// Gets the job suggestions.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="jobLimit">The job limit.</param>
        public static List<string> GetJobSuggestions(string job, int jobLimit = 3)
        {
            var jobTitles = new List<string>();
            var jobCount = 0;

            var suggestion = GetData<Suggestion>("/api/suggestions?parameters.text=chef&parameters.filters=Jobs");
            if (suggestion.Matches?.Count > 0)
            {
                foreach (var match in suggestion.Matches)
                {
                    var jobSearch = new JobSearch()
                    {
                        Address = "TX",
                        Keywords = new List<Keyword>()
                        {
                            new Keyword()
                            {
                                Value = job,
                                Source = 1
                            }
                        },
                        Distance = new Distance()
                        {
                            Unit = "mile",
                            Value = 0
                        }
                    };
                    var jobs = Express.PostData<JobSearchResult>("/api/jobs/search", jobSearch);

                    foreach (var pin in jobs.Pins)
                    {
                        foreach (var j in pin.Jobs)
                        {
                            jobTitles.Add(j.Title);

                            if (++jobCount >= jobLimit)
                            {
                                break;
                            }
                        }

                        if (jobCount >= jobLimit)
                        {
                            break;
                        }
                    }

                    if (jobCount >= jobLimit)
                    {
                        break;
                    }
                }
            }

            return jobTitles;
        }
    }
}