namespace PGB.WPF.Views.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    internal class MathConverter : MarkupExtension, IMultiValueConverter, IValueConverter
    {
        private readonly Dictionary<string, IExpression> _storedExpressions = new Dictionary<string, IExpression>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var num = Parse(parameter.ToString()).Eval(values);
                if (targetType == typeof(string))
                {
                    return num.ToString();
                }
                if (targetType == typeof(int))
                {
                    return (int)num;
                }
                if (targetType == typeof(double))
                {
                    return (double)num;
                }
                if (targetType == typeof(long))
                {
                    return (long)num;
                }

                return num;
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(new object[1] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        protected virtual void ProcessException(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        private IExpression Parse(string s)
        {
            IExpression expression = null;
            if (!_storedExpressions.TryGetValue(s, out expression))
            {
                expression = new Parser().Parse(s);
                _storedExpressions[s] = expression;
            }
            return expression;
        }

        private interface IExpression
        {
            decimal Eval(object[] args);
        }

        private class Constant : IExpression
        {
            private readonly decimal _value;

            public Constant(string text)
            {
                if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _value))
                {
                    throw new ArgumentException(string.Format("'{0}' is not a valid number", text));
                }
            }

            public decimal Eval(object[] args)
            {
                return _value;
            }
        }

        private class Variable : IExpression
        {
            private readonly int _index;

            public Variable(string text)
            {
                if (!int.TryParse(text, out _index) || _index < 0)
                {
                    throw new ArgumentException(string.Format("'{0}' is not a valid parameter index", text));
                }
            }

            public Variable(int n)
            {
                _index = n;
            }

            public decimal Eval(object[] args)
            {
                if (_index >= args.Length)
                {
                    throw new ArgumentException(
                        string.Format("MathConverter: parameter index {0} is out of range. {1} parameter(s) supplied",
                            _index, args.Length));
                }

                return System.Convert.ToDecimal(args[_index]);
            }
        }

        private class BinaryOperation : IExpression
        {
            private readonly IExpression _left;
            private readonly Func<decimal, decimal, decimal> _operation;
            private readonly IExpression _right;

            public BinaryOperation(char operation, IExpression left, IExpression right)
            {
                _left = left;
                _right = right;
                switch (operation)
                {
                    case '*':
                        _operation = (a, b) => a * b;
                        break;
                    case '+':
                        _operation = (a, b) => a + b;
                        break;
                    case '-':
                        _operation = (a, b) => a - b;
                        break;
                    case '/':
                        _operation = (a, b) => a / b;
                        break;
                    default:
                        throw new ArgumentException("Invalid operation " + operation);
                }
            }

            public decimal Eval(object[] args)
            {
                return _operation(_left.Eval(args), _right.Eval(args));
            }
        }

        private class Negate : IExpression
        {
            private readonly IExpression _param;

            public Negate(IExpression param)
            {
                _param = param;
            }

            public decimal Eval(object[] args)
            {
                return -_param.Eval(args);
            }
        }

        private class Parser
        {
            private int pos;
            private string text;

            public IExpression Parse(string text)
            {
                try
                {
                    pos = 0;
                    this.text = text;
                    var expression = ParseExpression();
                    RequireEndOfText();
                    return expression;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        string.Format("MathConverter: error parsing expression '{0}'. {1} at position {2}", text,
                            ex.Message, pos), ex);
                }
            }

            private IExpression ParseExpression()
            {
                var left = ParseTerm();
                while (pos < text.Length)
                {
                    var operation = text[pos];
                    switch (operation)
                    {
                        case '+':
                        case '-':
                            pos = pos + 1;
                            var term = ParseTerm();
                            left = new BinaryOperation(operation, left, term);
                            continue;
                        default:
                            return left;
                    }
                }

                return left;
            }

            private IExpression ParseTerm()
            {
                var left = ParseFactor();
                while (pos < text.Length)
                {
                    var operation = text[pos];
                    switch (operation)
                    {
                        case '*':
                        case '/':
                            pos = pos + 1;
                            var factor = ParseFactor();
                            left = new BinaryOperation(operation, left, factor);
                            continue;
                        default:
                            return left;
                    }
                }

                return left;
            }

            private IExpression ParseFactor()
            {
                SkipWhiteSpace();
                if (pos >= text.Length)
                {
                    throw new ArgumentException("Unexpected end of text");
                }

                var ch = text[pos];
                switch (ch)
                {
                    case '+':
                        pos = pos + 1;
                        return ParseFactor();
                    case '-':
                        pos = pos + 1;
                        return new Negate(ParseFactor());
                    case 'x':
                    case 'a':
                        return CreateVariable(0);
                    case 'y':
                    case 'b':
                        return CreateVariable(1);
                    case 'z':
                    case 'c':
                        return CreateVariable(2);
                    case 't':
                    case 'd':
                        return CreateVariable(3);
                    case '(':
                        pos = pos + 1;
                        var expression = ParseExpression();
                        SkipWhiteSpace();
                        Require(')');
                        SkipWhiteSpace();
                        return expression;
                    case '{':
                        pos = pos + 1;
                        var num = text.IndexOf('}', pos);
                        if (num < 0)
                        {
                            pos = pos - 1;
                            throw new ArgumentException("Unmatched '{'");
                        }

                        if (num == pos)
                        {
                            throw new ArgumentException("Missing parameter index after '{'");
                        }

                        var variable = new Variable(text.Substring(pos, num - pos).Trim());
                        pos = num + 1;
                        SkipWhiteSpace();
                        return variable;
                    default:
                        var match = Regex.Match(text.Substring(pos), "(\\d+\\.?\\d*|\\d*\\.?\\d+)");
                        if (!match.Success)
                        {
                            throw new ArgumentException(string.Format("Unexpeted character '{0}'", ch));
                        }

                        pos = pos + match.Length;
                        SkipWhiteSpace();
                        return new Constant(match.Value);
                }
            }

            private IExpression CreateVariable(int n)
            {
                pos = pos + 1;
                SkipWhiteSpace();
                return new Variable(n);
            }

            private void SkipWhiteSpace()
            {
                while (pos < text.Length && char.IsWhiteSpace(text[pos]))
                {
                    pos = pos + 1;
                }
            }

            private void Require(char c)
            {
                if (pos >= text.Length || text[pos] != c)
                {
                    throw new ArgumentException("Expected '" + c + "'");
                }

                pos = pos + 1;
            }

            private void RequireEndOfText()
            {
                if (pos != text.Length)
                {
                    throw new ArgumentException("Unexpected character '" + text[pos] + "'");
                }
            }
        }
    }
}