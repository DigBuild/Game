using System.Collections.Generic;
using System.Linq.Expressions;

namespace DigBuild.Render.Models.Expressions
{
    /// <summary>
    /// A model expression.
    /// </summary>
    public interface IModelExpression
    {
        /// <summary>
        /// The required variable names.
        /// </summary>
        IEnumerable<string> RequiredVariables { get; }
        /// <summary>
        /// The optional variable names.
        /// </summary>
        IEnumerable<string> OptionalVariables { get; }

        /// <summary>
        /// Applies a set of substitutions to the current expression and returns a new one.
        /// </summary>
        /// <param name="context">The substitution context</param>
        /// <returns>The new expression</returns>
        IModelExpression Apply(ModelExpressionSubstitutionContext context);

        /// <summary>
        /// Compiles the current expression to a native C# expression.
        /// </summary>
        /// <param name="context">The compilation context</param>
        /// <param name="numeric">Whether the expression is numeric or string</param>
        /// <returns>The expression</returns>
        Expression Compile(ModelExpressionCompilationContext context, bool numeric);

        /// <summary>
        /// Creates a string representation of this expression.
        /// </summary>
        /// <param name="numeric">Whether the expression is numeric or string</param>
        /// <returns>The string representation</returns>
        string ToString(bool numeric);
    }
}