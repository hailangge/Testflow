using Testflow.Data.Expression;

namespace Testflow.External.ExpressionCalculators
{
    public class StringAddCalculator : IExpressionCalculator
    {
        public StringAddCalculator()
        {
            Operator = "Addition";
        }

        public string Operator { get; }

        public bool IsCalculable(object sourceValue, params object[] arguments)
        {
            return sourceValue is string && arguments[0] is string;
        }

        public object Calculate(object sourceValue, params object[] arguments)
        {
            return sourceValue.ToString() + arguments[0];
        }
    }
}