using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace DigBuild.Client.Model
{
    public sealed class LiteralModelExpression : IModelExpression
    {
        public double DoubleValue { get; }
        public string StringValue { get; }
        
        public IEnumerable<string> RequiredVariables => Array.Empty<string>();
        public IEnumerable<string> OptionalVariables => Array.Empty<string>();
        
        public LiteralModelExpression(string value)
        {
            StringValue = value;
            DoubleValue = double.TryParse(StringValue, out var doubleValue) ? doubleValue : double.NaN;
        }

        public IModelExpression Apply(ModelExpressionSubstitutionContext context)
        {
            return this;
        }

        public Expression Compile(ModelExpressionCompilationContext context, bool numeric)
        {
            if (numeric && double.IsNaN(DoubleValue))
                throw new Exception($"Cannot interpret string as number: \"{StringValue}\"");
            return Expression.Constant(numeric ? DoubleValue : StringValue);
        }

        public string ToString(bool numeric)
        {
            if (numeric && double.IsNaN(DoubleValue))
                throw new Exception($"Cannot interpret string as number: \"{StringValue}\"");
            return numeric ? DoubleValue.ToString(CultureInfo.InvariantCulture) : $"\"{StringValue}\"";
        }

        public override string ToString()
        {
            return !double.IsNaN(DoubleValue) ? DoubleValue.ToString(CultureInfo.InvariantCulture) : $"\"{StringValue}\"";
        }
    }
}