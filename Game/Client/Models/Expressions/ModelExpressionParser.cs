using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DigBuild.Client.Models.Expressions
{
    public static class ModelExpressionParser
    {
        private static readonly Regex VariableRegex = new(@"^\$\{([\w\/]+)(?:\|([\s\w.,_-]+))?\}", RegexOptions.Compiled);
        private static readonly Regex NumberRegex = new(@"^[+-]?(?:\d+(?:\.\d+)?|\.\d+)", RegexOptions.Compiled);

        public static IModelExpression Parse(string expressionString)
        {
            return ParseConcatenated(expressionString);
        }

        private static IModelExpression ParseConcatenated(string expressionString)
        {
            var children = new List<IModelExpression>();
            var currentString = (string?) null;

            while (expressionString.Length > 0)
            {
                if (expressionString.StartsWith("${"))
                {
                    if (currentString != null)
                    {
                        children.Add(new LiteralModelExpression(currentString));
                        currentString = null;
                    }

                    children.Add(ParseVariable(expressionString, out var nextIndex));
                    expressionString = expressionString[nextIndex..];
                }
                else if (expressionString.StartsWith("{{"))
                {
                    if (currentString != null)
                    {
                        children.Add(new LiteralModelExpression(currentString));
                        currentString = null;
                    }

                    children.Add(ParseInterpolated(expressionString, out var nextIndex));
                    expressionString = expressionString[nextIndex..];
                }
                else
                {
                    currentString = (currentString ?? "") + expressionString[0];
                    expressionString = expressionString[1..];
                }
            }
            
            if (currentString != null)
                children.Add(new LiteralModelExpression(currentString));
            
            return children.Count == 1 ? children[0] : new ConcatModelExpression(children);
        }

        private static IModelExpression ParseVariable(string expressionString, out int nextIndex)
        {
            var match = VariableRegex.Match(expressionString);
            if (!match.Success)
                throw new ArgumentException($"Failed to parse variable: {expressionString[..10]}");

            nextIndex = match.Groups[0].Length;
            var name = match.Groups[1].Value;
            var defaultValueCapture = match.Groups[2];
            return new VariableModelExpression(name, defaultValueCapture.Success ? defaultValueCapture.Value : null);
        }
        
        private static IModelExpression ParseInterpolated(string expressionString, out int nextIndex)
        {
            var initialLength = expressionString.Length;
            
            var currentExpression = default(IModelExpression);
            var nextOperation = (NumericOperationType?) null;

            expressionString = expressionString[2..].TrimStart(); // Skip "{{"
            while (expressionString.Length > 0 && !expressionString.StartsWith("}}"))
            {
                // Could be literal or numeric variable
                IModelExpression newExpression;
                if (expressionString.StartsWith("${"))
                {
                    newExpression = ParseVariable(expressionString, out var next);
                    expressionString = expressionString[next..].TrimStart();
                }
                else
                {
                    var match = NumberRegex.Match(expressionString);
                    if (match.Success)
                    {
                        var cap = match.Captures[0];
                        newExpression = new LiteralModelExpression(cap.Value);
                        expressionString = expressionString[cap.Length..].TrimStart();
                    }
                    else
                    {
                        var firstChar = expressionString[0];
                        var foundOp = firstChar switch
                        {
                            '+' => NumericOperationType.Add,
                            '-' => NumericOperationType.Subtract,
                            '*' => NumericOperationType.Multiply,
                            '/' => NumericOperationType.Divide,
                            '%' => NumericOperationType.Modulo,
                            _ => (NumericOperationType?) null
                        };

                        if (foundOp == null)
                            throw new Exception("Could not match valid expression.");

                        if (currentExpression == null || nextOperation != null)
                            throw new Exception("Operators must only be placed between two expressions.");
                        nextOperation = foundOp;
                        expressionString = expressionString[1..].TrimStart();
                        continue;
                    }
                }

                // No operation, store as initial expression
                if (nextOperation == null)
                {
                    if (currentExpression != null)
                        throw new Exception("Found two expressions without an operator between them.");

                    currentExpression = newExpression;
                    continue;
                }
                
                // There is an operation, do the thing!
                currentExpression = new OperationModelExpression(currentExpression!, newExpression, nextOperation.Value);
                nextOperation = null;
            }

            if (!expressionString.StartsWith("}}"))
                throw new Exception("Unexpected end of interpolated expression.");
            expressionString = expressionString[2..];

            if (nextOperation != null)
                throw new Exception("Found operator at the end of the expression.");

            if (currentExpression == null)
                throw new Exception("Empty interpolated expression.");

            nextIndex = initialLength - expressionString.Length;

            return currentExpression;
        }
    }
}