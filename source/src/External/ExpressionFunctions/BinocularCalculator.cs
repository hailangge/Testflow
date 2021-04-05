using Testflow.Data.Expression;

namespace Testflow.External.ExpressionCalculators
{
    public class BinocularCalculator : IExpressionCalculator
    {
        public BinocularCalculator()
        {
            Operator = "BinocularOperation";
        }

        public string Operator { get; }

        public bool IsCalculable(object sourceValue, params object[] arguments)
        {
            return sourceValue is bool;
        }

        public object Calculate(object sourceValue, params object[] arguments)
        {
            return ((bool)sourceValue) ? arguments[0] : arguments[1];
        }
    }
}