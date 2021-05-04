using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DigBuild.Client.Models.Expressions
{
    public sealed class ModelExpressionSubstitutionContext
    {
        public IReadOnlyDictionary<string, IModelExpression> Substitutions { get; }

        public ModelExpressionSubstitutionContext(IReadOnlyDictionary<string, IModelExpression> substitutions)
        {
            Substitutions = substitutions;
        }

        public bool TryGet(string name, [MaybeNullWhen(false)] out IModelExpression expression)
        {
            return Substitutions.TryGetValue(name, out expression);
        }
    }
}