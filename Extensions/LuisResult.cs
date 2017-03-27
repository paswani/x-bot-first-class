using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Luis.Models
{
    /// <summary>
    /// Extension class for <see cref="LuisResult"/>
    /// </summary>
    public static partial class LuisResultExtensions
    {
        /// <summary>
        /// Tries to find multiple entities with a specific type.
        /// </summary>
        /// <param name="result">The <see cref="LuisResult"/>.</param>
        /// <param name="entityName">Name of the entity type.</param>
        /// <param name="entities">The out parameter that will contain the list of found <see cref="EntityRecommendation"/>.</param>
        /// <returns>True if at least one entity was found, false otherwise.</returns>
        public static bool TryFindEntities(this LuisResult result, string entityName, out List<EntityRecommendation> entities)
        {
            entities = new List<EntityRecommendation>();

            for (var i = 0; i < result.Entities.Count; i++)
            {
                var entity = result.Entities[i];

                if (entity.Type.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    entities.Add(entity);
                }
            }

            return entities.Count > 0 == true;
        }
    }
}