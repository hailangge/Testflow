using System.Collections.Generic;

namespace Testflow.Utility.Expression
{
    internal class OperatorTokenInfoComparer : IComparer<OperatorTokenInfo>
    {
        // 从打到小排序
        public int Compare(OperatorTokenInfo x, OperatorTokenInfo y)
        {
            if (x.Priority == y.Priority)
            {
                return 0;
            }
            return x.Priority < y.Priority ? 1 : -1;
        }
    }
}