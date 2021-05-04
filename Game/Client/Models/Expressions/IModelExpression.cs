using System.Collections.Generic;
using System.Linq.Expressions;

namespace DigBuild.Client.Models.Expressions
{
    public interface IModelExpression
    {
        IEnumerable<string> RequiredVariables { get; }
        IEnumerable<string> OptionalVariables { get; }

        IModelExpression Apply(ModelExpressionSubstitutionContext context);
        Expression Compile(ModelExpressionCompilationContext context, bool numeric);
        string ToString(bool numeric);
    }
}