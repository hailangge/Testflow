using System.Collections.Generic;
using System.Text;
using Testflow.Data.Expression;
using Testflow.Data.Sequence;
using Testflow.Usr;

namespace Testflow.Utility.Expression
{
    /// <summary>
    /// 单个表达式符号遍历状态
    /// </summary>
    internal class OperatorInstance : ICloneableClass<OperatorInstance>
    {
        public string Name { get; set; }
        public int Priority => this.TokenInfo.Priority;

        /// <summary>
        /// 当前符号在分割缓存中的起始索引
        /// </summary>
        public int StartIndex { get; }

        public OperatorTokenInfo TokenInfo { get; }

        /// <summary>
        /// 写入的参数
        /// </summary>
        private List<string> Arguments { get; }

        // 当前已经匹配到的符号的索引
        private int _currentTokenIndex;

        public OperatorInstance(OperatorTokenInfo token, int startIndex)
        {
            this.TokenInfo = token;
            this.StartIndex = startIndex;
            this._currentTokenIndex = 0;
            this.Arguments = new List<string>(token.ArgumentCount);
        }

        public bool IsCurrentTokenFit(string token)
        {
            return this._currentTokenIndex < TokenInfo.TokenGroup.Length - 1 && 
                   token.Equals(this.TokenInfo.TokenGroup[this._currentTokenIndex + 1]);
        }

        public void MoveToNextToken()
        {
            this._currentTokenIndex++;
        }

        public bool IsTokenIterationOver()
        {
            return this._currentTokenIndex >= TokenInfo.TokenGroup.Length - 1;
        }

        public bool IsOver()
        {
            return IsTokenIterationOver() && Arguments.Count == TokenInfo.ArgumentCount;
        }

        public bool IsNeedRightElement()
        {
            int leftOffset = TokenInfo.HasLeftElement ? 1 : 0;
            int rightOffset = TokenInfo.HasRightElement ? 1 : 0;
            int expectArgumentCount = this._currentTokenIndex + leftOffset + rightOffset;
            return expectArgumentCount > Arguments.Count;
        }

        public int ElementCountToFill()
        {
            int leftOffset = TokenInfo.HasLeftElement ? 1 : 0;
            int rightOffset = TokenInfo.HasRightElement ? 1 : 0;
            int expectArgumentCount = this._currentTokenIndex + leftOffset + rightOffset;
            return expectArgumentCount - Arguments.Count;
        }

        public void AddArgument(string argument)
        {
            this.Arguments.Add(argument);
        }

        public IExpressionData CreateExpression()
        {
            ExpressionData expressionData = new ExpressionData(Arguments.Count) {Operation = Name};
            foreach (string argument in Arguments)
            {
                ParameterType argumentType = argument.StartsWith(UtilityConstants.ExpNamePrefix)
                    ? ParameterType.Expression
                    : ParameterType.Value;
                expressionData.Arguments.Add(new ExpressionElement(argumentType, argument));
            }
            return expressionData;
        }

        public OperatorInstance Clone()
        {
            OperatorInstance instance = new OperatorInstance(TokenInfo, StartIndex)
            {
                Name = this.Name,
                
            };
            instance.Arguments.AddRange(this.Arguments);
            instance.SetCurrentIndex(this._currentTokenIndex);
            return instance;
        }

        internal void SetCurrentIndex(int value)
        {
            this._currentTokenIndex = value;
        }
    }
}