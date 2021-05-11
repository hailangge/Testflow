using System.Collections.Generic;
using System.Text.RegularExpressions;
using Testflow.Data.Expression;
using Testflow.Loader;
using Testflow.Utility.Expression;

namespace Testflow.TestflowLoaderTest
{
    public class SystemTest
    {
        public static void Main(string[] args)
        {
            Regex regex = new Regex("[abc]");
            string[] strings = regex.Split("1a2b3c");

            Stack<int> stack = new Stack<int>(10);
            stack.Push(0);
            stack.Push(1);
            stack.Push(2);
            int[] array = stack.ToArray();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                stack.Push(array[i]);
            }

            TestflowRunnerOptions runnerOptions = new TestflowRunnerOptions();
            TestflowRunner runnerInstance = TestflowActivator.CreateRunner(runnerOptions);
            runnerInstance.Initialize();
            runnerInstance.DesigntimeInitialize();
            Dictionary<string, ExpressionOperatorInfo> operatorInfos =
                    runnerInstance.SequenceManager.ConfigData.GetProperty<Dictionary<string, ExpressionOperatorInfo>>("ExpressionOperators");
            ExpressionParser parser = new ExpressionParser(operatorInfos);
            // IExpressionData expressionData = parser.ParseExpression("a[1, 2] -(- b) [1E-10]");
            // IExpressionData expressionData1 = parser.ParseExpression("e ? a[1, 2] -(- b) [1E-10] : c");
            // IExpressionData expressionData2 = parser.ParseExpression("e ? a[1, 2] -(- b) [1E-10] : c*d");
            // IExpressionData expressionData5 = parser.ParseExpression("(e ? a[1, 2] -(- b) [1E-10] : c)*d");
            // IExpressionData expressionData3 = parser.ParseExpression("a[1, 1-a*b+2] -(- b) [1E-10*5]");
            // IExpressionData expressionData6 = parser.ParseExpression("a[1, e[1,b[1,2],3]] -(- b) [1E-10*5]");
            // IExpressionData expressionData4 = parser.ParseExpression("a[1, 1-a*b+2] -(- b) [1E-10*5]+");
            IExpressionData expressionData7 = parser.ParseExpression("-a[10]");
        }
    }
}