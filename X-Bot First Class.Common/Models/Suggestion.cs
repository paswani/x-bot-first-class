using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Bot_First_Class.Common.Models
{
    /// <summary>
    /// Public class representing a suggestion returned by the Express Suggestions API
    /// </summary>
    public class Suggestion
    {
        /// <summary>
        /// Gets or sets the matches.
        /// </summary>
        /// <value>
        /// The matches.
        /// </value>
        public List<string> Matches { get; set; }

        /// <summary>
        /// Gets or sets the adjusted term.
        /// </summary>
        /// <value>
        /// The adjusted term.
        /// </value>
        public string AdjustedTerm { get; set; }

        /// <summary>
        /// Gets or sets the adjusted matches.
        /// </summary>
        /// <value>
        /// The adjusted matches.
        /// </value>
        public List<string> AdjustedMatches { get; set; }
    }
}
