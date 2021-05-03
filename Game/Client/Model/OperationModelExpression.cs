using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace DigBuild.Client.Model
{
    public sealed class OperationModelExpression : IModelExpression
    {
        public IModelExpression First { get; }
        public IModelExpression Second { get; }
        public NumericOperationType Operation { get; }
        
        public IEnumerable<string> RequiredVariables { get; }
        public IEnumerable<string> OptionalVariables { get; }

        public OperationModelExpression(IModelExpression first, IModelExpression second, NumericOperationType operation)
        {
            First = first;
            Second = second;
            Operation = operation;
            RequiredVariables = first.RequiredVariables.Concat(second.RequiredVariables).ToImmutableHashSet();
            OptionalVariables = first.OptionalVariables.Concat(second.OptionalVariables).ToImmutableHashSet();
        }

        public IModelExpression Apply(ModelExpressionSubstitutionContext context)
        {
            return new OperationModelExpression(
                First.Apply(context),
                Second.Apply(context),
                Operation
            );
        }

        public Expression Compile(ModelExpressionCompilationContext context, bool numeric)
        {
            var first = First.Compile(context, true);
            var second = Second.Compile(context, true);

            Expression exp = 
                first is ConstantExpression firstConst && second is ConstantExpression secondConst ?
                    OperateDirect((double) firstConst.Value!, (double) secondConst.Value!, Operation) :
                    OperateDeferred(first, second, Operation);

            return numeric ? exp : Expression.Call(exp, "ToString", Array.Empty<Type>());
        }

        public string ToString(bool numeric)
        {
            var first = First is not OperationModelExpression ? $"{First.ToString(true)}" : $"({First.ToString(true)})";
            var second = Second is not OperationModelExpression ? $"{Second.ToString(true)}" : $"({Second.ToString(true)})";
            var str = $"{first} {GetOperationString(Operation)} {second}";
            return numeric ? str : $"({str})";
        }

        public override string ToString()
        {
            return ToString(true);
        }

        private static BinaryExpression OperateDeferred(Expression first, Expression second, NumericOperationType operation)
        {
            return operation switch
            {
                NumericOperationType.Add => Expression.Add(first, second),
                NumericOperationType.Subtract => Expression.Subtract(first, second),
                NumericOperationType.Multiply => Expression.Multiply(first, second),
                NumericOperationType.Divide => Expression.Divide(first, second),
                NumericOperationType.Modulo => Expression.Modulo(first, second),
                _ => throw new Exception("Invalid operation.")
            };
        }

        private static ConstantExpression OperateDirect(double first, double second, NumericOperationType operation)
        {
            return Expression.Constant(
                operation switch
                {
                    NumericOperationType.Add => first + second,
                    NumericOperationType.Subtract => first - second,
                    NumericOperationType.Multiply => first * second,
                    NumericOperationType.Divide => first / second,
                    NumericOperationType.Modulo => first % second,
                    _ => throw new Exception("Invalid operation.")
                }
            );
        }

        private static string GetOperationString(NumericOperationType operation)
        {
            return operation switch
            {
                NumericOperationType.Add => "+",
                NumericOperationType.Subtract => "-",
                NumericOperationType.Multiply => "*",
                NumericOperationType.Divide => "/",
                NumericOperationType.Modulo => "%",
                _ => throw new Exception("Invalid operation.")
            };
        }
    }

    public enum NumericOperationType : byte
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }
}