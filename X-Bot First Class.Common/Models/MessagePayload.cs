using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace X_Bot_First_Class.Common
{
    /// <summary>
    /// Public class that represents the message payload
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MessagePayload
    {
        /// <summary>
        /// Gets or sets from identifier.
        /// </summary>
        /// <value>
        /// From identifier.
        /// </value>
        [JsonProperty("fromId")]
        public string FromId { get; set; }

        ///// <summary>
        ///// Gets or sets from name.
        ///// </summary>
        ///// <value>
        ///// From name.
        ///// </value>
        [JsonProperty("fromName")]
        public string FromName { get; set; }

        ///// <summary>
        ///// Gets or sets to identifier.
        ///// </summary>
        ///// <value>
        ///// To identifier.
        ///// </value>
        [JsonProperty("toId")]
        public string ToId { get; set; }

        ///// <summary>
        ///// Gets or sets to name.
        ///// </summary>
        ///// <value>
        ///// To name.
        ///// </value>
        [JsonProperty("toName")]
        public string ToName { get; set; }

        ///// <summary>
        ///// Gets or sets the text.
        ///// </summary>
        ///// <value>
        ///// The text.
        ///// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        ///// <summary>
        ///// Gets or sets the locale.
        ///// </summary>
        ///// <value>
        ///// The locale.
        ///// </value>
        [JsonProperty("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the service URL.
        /// </summary>
        /// <value>
        /// The service URL.
        /// </value>
        public string ServiceUrl { get; set; }
    }
}
