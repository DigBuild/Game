using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DigBuild.Render.Models.Expressions
{
    /// <summary>
    /// A model expression compilation context that substitutes variables with map lookups.
    /// </summary>
    public sealed class ModelExpressionCompilationContext
    {
        private static readonly MethodInfo ParseDouble = typeof(double).GetMethod(
            "Parse", BindingFlags.Public | BindingFlags.Static,
            null, CallingConventions.Any, new[]{typeof(string)},
            null
        )!;
        
        private readonly ParameterExpression _parameter;

        public ModelExpressionCompilationContext(ParameterExpression parameter)
        {
            _parameter = parameter;
        }
        
        public Expression GetRuntimeVariable(string name, bool numeric)
        {
            var baseExp = Expression.Call(_parameter, "get_Item", null, Expression.Constant(name));
            
            return !numeric ? baseExp : Expression.Call(null, ParseDouble, baseExp);
        }
    }

    /// <summary>
    /// Model expression compilation utilities.
    /// </summary>
    public static class ModelExpressionCompilationExtensions
    {
        /// <summary>
        /// Compiles the expression into a Dictionary->string function.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>The funtion</returns>
        public static Func<IReadOnlyDictionary<string, string>, string> CompileString(this IModelExpression expression)
        {
            var par = Expression.Parameter(typeof(IReadOnlyDictionary<string, string>));
            var exp = expression.Compile(new ModelExpressionCompilationContext(par), false);
            var lambda = Expression.Lambda<Func<IReadOnlyDictionary<string, string>, string>>(exp, par);
            return lambda.Compile();
        }
        
        /// <summary>
        /// Compiles the expression into a Dictionary->double function.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>The funtion</returns>
        public static Func<IReadOnlyDictionary<string, string>, double> CompileDouble(this IModelExpression expression)
        {
            var par = Expression.Parameter(typeof(IReadOnlyDictionary<string, string>));
            var exp = expression.Compile(new ModelExpressionCompilationContext(par), true);
            var lambda = Expression.Lambda<Func<IReadOnlyDictionary<string, string>, double>>(exp, par);
            return lambda.Compile();
        }
    }
}