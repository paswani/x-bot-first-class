using System.Collections.Generic;

namespace X_Bot_First_Class.Common
{
    /// <summary>
    /// Public class representing a job search query
    /// </summary>
    public class JobSearch
    {
        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>
        /// The keywords.
        /// </value>
        public List<Keyword> Keywords { get; set; }

        /// <summary>
        /// Gets or sets the distance.
        /// </summary>
        /// <value>
        /// The distance.
        /// </value>
        public Distance Distance { get; set; }
    }
}
