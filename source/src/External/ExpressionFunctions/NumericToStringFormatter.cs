using Testflow.Data.Expression;

namespace Testflow.External.ExpressionCalculators
{
    public class NumericToStringFormatter : IExpressionCalculator
    {
        public NumericToStringFormatter()
        {
            Operator = "StringFormat";
        }

        public string Operator { get; }

        public bool IsCalculable(object sourceValue, params object[] arguments)
        {
            return sourceValue is double && arguments[0] is string;
        }

        public object Calculate(object sourceValue, params object[] arguments)
        {
            string format = arguments[0] as string;
            return string.IsNullOrWhiteSpace(format)
                ? ((double) sourceValue).ToString()
                : ((double) sourceValue).ToString(format);
        }
    }
}