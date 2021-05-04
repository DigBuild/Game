using System.Linq.Expressions;

namespace DigBuild.Client.Models.Expressions
{
    public sealed class ModelExpressionCompilationContext
    {
        public Expression GetRuntimeVariable(string name, bool numeric)
        {
            return Expression.Parameter(numeric ? typeof(double) : typeof(string), name);
        }
    }
}