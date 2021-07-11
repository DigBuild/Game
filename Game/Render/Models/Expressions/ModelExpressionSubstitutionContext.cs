using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DigBuild.Render.Models.Expressions
{
    public sealed class ModelExpressionSubstitutionContext
    {
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

        public bool TryGet(string name, [MaybeNullWhen(false)] out IModelExpression expression)
        {
            return Substitutions.TryGetValue(name, out expression);
        }
    }
}