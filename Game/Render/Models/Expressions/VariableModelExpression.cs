using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace DigBuild.Render.Models.Expressions
{
    /// <summary>
    /// An expression representing a variable with an optional default value.
    /// </summary>
    public sealed class VariableModelExpression : IModelExpression
    {
        /// <summary>
        /// The variable name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The default value literal.
        /// </summary>
        public LiteralModelExpression? DefaultValue { get; }
        
        public IEnumerable<string> RequiredVariables { get; }
        public IEnumerable<string> OptionalVariables { get; }

        public VariableModelExpression(string name, string? defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue == null ? null : new LiteralModelExpression(defaultValue);

            var variables = ImmutableHashSet.Create(name);
            RequiredVariables = defaultValue == null ? variables : Array.Empty<string>();
            OptionalVariables = defaultValue != null ? variables : Array.Empty<string>();
        }

        public IModelExpression Apply(ModelExpressionSubstitutionContext context)
        {
            return context.TryGet(Name, out var exp) ? exp : this;
        }

        public Expression Compile(ModelExpressionCompilationContext context, bool numeric)
        {
            if (DefaultValue != null)
                return DefaultValue.Compile(context, numeric);

            return context.GetRuntimeVariable(Name, numeric);
        }

        public string ToString(bool numeric)
        {
            if (DefaultValue != null)
                return $"({Name} ?? {DefaultValue.ToString(numeric)})";
            return Name;
        }

        public override string ToString()
        {
            return ToString(false);
        }
    }
}