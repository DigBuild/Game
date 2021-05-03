using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DigBuild.Client.Model
{
    public interface IModelExpression
    {
        IEnumerable<string> RequiredVariables { get; }
        IEnumerable<string> OptionalVariables { get; }

        IModelExpression Apply(ModelExpressionSubstitutionContext context);
        Expression Compile(ModelExpressionCompilationContext context, bool numeric);
        string ToString(bool numeric);
    }

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

    public sealed class ModelExpressionCompilationContext
    {
        public Expression GetRuntimeVariable(string name, bool numeric)
        {
            return Expression.Parameter(numeric ? typeof(double) : typeof(string), name);
        }
    }
}