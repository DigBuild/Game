using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DigBuild.Render.Models.Expressions
{
    public sealed class ModelExpressionCompilationContext
    {
        private readonly ParameterExpression _parameter;

        public ModelExpressionCompilationContext(ParameterExpression parameter)
        {
            _parameter = parameter;
        }


        public Expression GetRuntimeVariable(string name, bool numeric)
        {
            return Expression.Parameter(numeric ? typeof(double) : typeof(string), name);
        }
    }

    public static class ModelExpressionCompilationExtensions
    {
        public static Func<IReadOnlyDictionary<string, string>, string> CompileString(this IModelExpression expression)
        {
            var par = Expression.Parameter(typeof(IReadOnlyDictionary<string, string>));
            var exp = expression.Compile(new ModelExpressionCompilationContext(par), false);
            var lambda = Expression.Lambda<Func<IReadOnlyDictionary<string, string>, string>>(exp, par);
            return lambda.Compile();
        }
        
        public static Func<IReadOnlyDictionary<string, string>, double> CompileDouble(this IModelExpression expression)
        {
            var par = Expression.Parameter(typeof(IReadOnlyDictionary<string, string>));
            var exp = expression.Compile(new ModelExpressionCompilationContext(par), true);
            var lambda = Expression.Lambda<Func<IReadOnlyDictionary<string, string>, double>>(exp, par);
            return lambda.Compile();
        }
    }
}