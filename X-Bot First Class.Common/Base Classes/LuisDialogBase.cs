using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;

namespace X_Bot_First_Class.Common
{
    /// <summary>
    /// Public base class from which all LuisDialog classes should inherit.
    /// </summary>
    /// <typeparam name="T">Dialog parameter type.</typeparam>
    /// <seealso cref="Microsoft.Bot.Builder.Dialogs.LuisDialog{T}" />
    [Serializable]
    public class LuisDialogBase<T> : LuisDialog<T>
    {
        /// <summary>
        /// Determines whether this instance can handle the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>True if it can handle it, false otherwise.</returns>
        public async Task<bool> CanHandle(string query)
        {
            var tasks = services.Select(s => s.QueryAsync(query, CancellationToken.None)).ToArray();
            var results = await Task.WhenAll(tasks);

            var winners = from result in results.Select((value, index) => new { value, index })
                          let resultWinner = BestIntentFrom(result.value)
                          where resultWinner != null
                          select new LuisServiceResult(result.value, resultWinner, services[result.index]);

            var winner = BestResultFrom(winners);
            return winner != null && winner.BestIntent.Intent != "None";
        }
    }
}
