using System.Collections.Generic;
using System.Text.RegularExpressions;
using Testflow.Data.Expression;
using Testflow.Usr;
using Testflow.Utility.I18nUtil;

namespace Testflow.Utility.Expression
{
    /// <summary>
    /// 运算符的符号信息
    /// </summary>
    internal class OperatorTokenInfo
    {
        /// <summary>
        /// 运算符名称
        /// </summary>
        public string OperatorName { get; }

        /// <summary>
        /// 运算符模式
        /// </summary>
        public string OperatorPattern { get; }

        /// <summary>
        /// 运算符符号集合
        /// </summary>
        public string[] TokenGroup { get; }

        /// <summary>
        /// 运算符左侧是否包含元素
        /// </summary>
        public bool HasLeftElement { get; }

        /// <summary>
        /// 运算符右侧是否包含元素
        /// </summary>
        public bool HasRightElement { get; }

        /// <summary>
        /// 当前符号描述信息在整体列表中的索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 参数个数
        /// </summary>
        public int ArgumentCount { get; set; }

        /// <summary>
        /// 运算优先级
        /// </summary>
        public int Priority { get; }

        internal OperatorTokenInfo(ExpressionOperatorInfo operationInfo)
        {
            this.OperatorName = operationInfo.Name;
            this.OperatorPattern = operationInfo.FormatString;
            string[] splitTokens = Regex.Split(operationInfo.FormatString, UtilityConstants.OperatorPlaceHolderRegex);
            List<string> tokenList = new List<string>(splitTokens);
            for (int i = tokenList.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(tokenList[i]))
                {
                    tokenList.RemoveAt(i);
                }
            }
            TokenGroup = tokenList.ToArray();
            this.Priority = operationInfo.Priority;
            this.HasLeftElement = Regex.IsMatch(operationInfo.FormatString, UtilityConstants.LeftValuePattern);
            this.HasRightElement = Regex.IsMatch(operationInfo.FormatString, UtilityConstants.RightValuePattern);
            this.ArgumentCount = operationInfo.ArgumentsCount;

            int expectArgumentCount = TokenGroup.Length - 1 + (HasLeftElement ? 1 : 0) + (HasRightElement ? 1 : 0);
            if (expectArgumentCount != operationInfo.ArgumentsCount)
            {
                I18N i18N = I18N.GetInstance(UtilityConstants.ExpI18nName);
                throw new TestflowDataException(ModuleErrorCode.ExpressionError,
                    i18N.GetFStr("IllegalOperatorFormat", operationInfo.Symbol));
            }
        }

        public int GetTokenIndex(string token)
        {
            for (int i = TokenGroup.Length - 1; i >= 0; i++)
            {
                if (TokenGroup[i].Equals(token))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}