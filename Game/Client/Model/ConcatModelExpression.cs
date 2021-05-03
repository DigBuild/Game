using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DigBuild.Client.Model
{
    public sealed class ConcatModelExpression : IModelExpression
    {
        private static readonly MethodInfo StringConcat = typeof(string).GetMethod("Concat", new []{ typeof(IEnumerator<string>) })!;

        public ImmutableList<IModelExpression> Children { get; }

        public IEnumerable<string> RequiredVariables { get; }
        public IEnumerable<string> OptionalVariables { get; }

        public ConcatModelExpression(IEnumerable<IModelExpression> children)
        {
            Children = children.ToImmutableList();
            
            RequiredVariables = Children.SelectMany(child => child.RequiredVariables).ToImmutableHashSet();
            OptionalVariables = Children.SelectMany(child => child.OptionalVariables).ToImmutableHashSet();
        }

        public IModelExpression Apply(ModelExpressionSubstitutionContext context)
        {
            return new ConcatModelExpression(Children.Select(child => child.Apply(context)));
        }

        public Expression Compile(ModelExpressionCompilationContext context, bool numeric)
        {
            if (numeric)
                throw new Exception("Cannot concatenate strings into a number.");

            var strings = Children.Select(child => child.Compile(context, false));
            var arrayOfValues = Expression.NewArrayInit(typeof(string), strings);
            return Expression.Call(StringConcat, arrayOfValues);
        }

        public string ToString(bool numeric)
        {
            if (numeric)
                throw new Exception("Cannot concatenate strings into a number.");

            return string.Join(" + ", Children.Select(child => child.ToString(false)));
        }

        public override string ToString()
        {
            return ToString(false);
        }
    }
}