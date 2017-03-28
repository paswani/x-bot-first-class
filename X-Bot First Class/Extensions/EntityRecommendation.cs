using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Luis.Models
{
    /// <summary>
    /// Extension class for <see cref="EntityRecommendation"/>
    /// </summary>
    public static partial class EntityRecommendationExtensions
    {
        /// <summary>
        /// Concatenates the entity values from a list of <see cref="EntityRecommendation"/>
        /// </summary>
        /// <param name="entities">The list of <see cref="EntityRecommendation"/>.</param>
        /// <param name="concatValue">The string value to use when concatenating multiple entity values.</param>
        /// <returns>The concatenated string.</returns>
        public static string ConcatEntities(this List<EntityRecommendation> entities, string concatValue)
        {
            var entityValues = new List<string>();

            entities.ForEach(entity =>
            {
                entityValues.Add(entity.Entity);
            });

            return string.Join(concatValue, entityValues);
        }
    }
}