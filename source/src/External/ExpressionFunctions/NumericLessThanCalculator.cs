using System;
using System.Collections;
using Testflow.Data.Expression;
using Testflow.Usr;

namespace Testflow.External.ExpressionCalculators
{
    public class NumericLessThanCalculator : IExpressionCalculator
    {
        public NumericLessThanCalculator()
        {
            this.Operator = "LessThan";
        }

        public string Operator { get; }
        public bool IsCalculable(object sourceValue, params object[] arguments)
        {
            return sourceValue is double && arguments[0] is double;
        }

        public object Calculate(object sourceValue, params object[] arguments)
        {
            return (double) sourceValue < (double) arguments[0];
        }
    }
}
