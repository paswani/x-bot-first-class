using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

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
            var httpClient = new HttpClient();
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
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(string.Concat(ApiUrl, endpoint));
            var result = response.Result.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}