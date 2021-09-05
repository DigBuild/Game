using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DigBuild.Render.Models.Expressions
{
    /// <summary>
    /// An expression substitution context.
    /// </summary>
    public sealed class ModelExpressionSubstitutionContext
    {
        /// <summary>
        /// The substitutions.
        /// </summary>
        public IReadOnlyDictionary<string, IModelExpression> Substitutions { get; }

        public ModelExpressionSubstitutionContext(IReadOnlyDictionary<string, IModelExpression> substitutions)
        {
            Substitutions = substitutions;
        }

        public ModelExpressionSubstitutionContext(IReadOnlyDictionary<string, string> substitutions)
        {
            var subs = new Dictionary<string, IModelExpression>();
            foreach (var (key, expressionString) in substitutions)
                subs[key] = ModelExpressionParser.Parse(expressionString);
            Substitutions = subs;
        }

        /// <summary>
        /// Tries to get the expression to substitute the given variable name.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="expression">The expression, or null if missing</param>
        /// <returns>Whether an expression for that name was found</returns>
        public bool TryGet(string name, [MaybeNullWhen(false)] out IModelExpression expression)
        {
            return Substitutions.TryGetValue(name, out expression);
        }
    }
}