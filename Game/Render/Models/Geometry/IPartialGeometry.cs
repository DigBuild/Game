using System.Collections.Generic;
using DigBuild.Render.Models.Expressions;

namespace DigBuild.Render.Models.Geometry
{
    /// <summary>
    /// A partially resolved geometry.
    /// </summary>
    public interface IPartialGeometry
    {
        /// <summary>
        /// The required variables.
        /// </summary>
        IEnumerable<string> RequiredVariables { get; }

        /// <summary>
        /// Applies a set of substitutions to the variables in this geometry and returns a new one.
        /// </summary>
        /// <param name="variables">The variables</param>
        /// <returns>The new partial geometry</returns>
        IPartialGeometry ApplySubstitutions(IReadOnlyDictionary<string, IModelExpression> variables);

        /// <summary>
        /// Primes the geometry once all variables have been resolved.
        /// </summary>
        /// <returns>A new raw geometry</returns>
        IRawGeometry Prime();
    }
}