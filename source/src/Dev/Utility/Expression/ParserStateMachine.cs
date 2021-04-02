using System;
using System.Collections.Generic;
using Testflow.Data.Expression;
using Testflow.Usr;
using Testflow.Utility.I18nUtil;

namespace Testflow.Utility.Expression
{
    /// <summary>
    /// 表达式解析状态机
    /// </summary>
    internal class ParserStateMachine
    {
        private readonly IList<OperatorTokenInfo> _operators;

        /// <summary>
        /// 当前检查的运算符
        /// </summary>
        private OperatorInstance _currentOperator;

        /// <summary>
        /// 运算符解析栈
        /// </summary>
        private readonly Stack<OperatorInstance> _operatorStack;

        /// <summary>
        /// 运算符解析出现歧义点的栈
        /// </summary>
        private readonly Stack<AmbiguousPoint> _ambiguousStack;

        /// <summary>
        /// 临时表达式缓存
        /// </summary>
        private Dictionary<string, IExpressionData> _expressionCache;

        /// <summary>
        /// 临时表达式下一个可用的索引号
        /// </summary>
        private int _expressionIndex;

        /// <summary>
        /// 当前遍历到的元素索引
        /// </summary>
        private int _elementIndex;

        /// <summary>
        /// 待处理的表达式
        /// </summary>
        private string[] _splitExpression;

        /// <summary>
        /// 待处理的参数值
        /// </summary>
        private string _leftArgument;
        
        /// <summary>
        /// 在AmbiguousStack执行Pop后会将上次歧义点的下一个可用操作符传递给该字段，该字段在调用GetAvailableToken方法后会被置为null。
        /// </summary>
        private OperatorTokenInfo _lastAmbiguousPointToken;

        /// <summary>
        /// 获取可用符号的缓存
        /// </summary>
        private List<OperatorTokenInfo> _fetchTokenInfoCache;

        /// <summary>
        /// 中间的表达式缓存项
        /// </summary>
        public Dictionary<string, IExpressionData> ExpressionCache => this._expressionCache;

        public ParserStateMachine(IList<OperatorTokenInfo> operators)
        {
            this._operators = operators;
            this._operatorStack = new Stack<OperatorInstance>(20);
            this._ambiguousStack = new Stack<AmbiguousPoint>(20);
            this._expressionCache = new Dictionary<string, IExpressionData>(50);
            this._fetchTokenInfoCache = new List<OperatorTokenInfo>(10);

            ResetCache();
        }

        private void ResetCache()
        {
            this._splitExpression = null;
            this._leftArgument = null;
            this._expressionIndex = 0;
            this._currentOperator = null;
            this._operatorStack.Clear();
            this._ambiguousStack.Clear();
            this._expressionCache.Clear();
            this._elementIndex = -1;
            this._lastAmbiguousPointToken = null;
            this._fetchTokenInfoCache.Clear();
        }

        private void PushAmbiguousPoint(List<OperatorTokenInfo> possibleTokens)
        {
            AmbiguousPoint ambiguousPoint = new AmbiguousPoint()
            {
                ElementIndex = this._elementIndex,
                OperatorStackLength = this._operatorStack.Count,
                ExpressionCacheLength = this._expressionCache.Count,
                CurrentOperator = this._currentOperator?.Clone(),
                LeftArgument = this._leftArgument,
                PossibleTokens = new List<OperatorTokenInfo>(possibleTokens)
            };
            this._ambiguousStack.Push(ambiguousPoint);
            possibleTokens.Clear();
        }

        private void PopAmbiguousPoint()
        {
            if (this._ambiguousStack.Count == 0)
            {
                throw new ApplicationException("Pop failed.");
            }
            AmbiguousPoint ambiguousPoint = this._ambiguousStack.Peek();
            this._elementIndex = ambiguousPoint.ElementIndex;
            while (this._operatorStack.Count > ambiguousPoint.OperatorStackLength)
            {
                this._operatorStack.Pop();
            }
            this._currentOperator = ambiguousPoint.CurrentOperator;
            this._leftArgument = ambiguousPoint.LeftArgument;
            this._lastAmbiguousPointToken = ambiguousPoint.PossibleTokens[0];
            ambiguousPoint.PossibleTokens.RemoveAt(0);
            if (ambiguousPoint.PossibleTokens.Count == 0)
            {
                this._ambiguousStack.Pop();
            }
        }

        public IExpressionData ParseExpression(string[] fullySplitExpression)
        {
            this._splitExpression = fullySplitExpression;
            this._elementIndex = 0;
            try
            {
                bool parseOver = false;
                do
                {
                    Func<bool> stateFunc = GetNextStateFunc();
                    parseOver = stateFunc();
                    this._elementIndex++;
                } while (!parseOver);
            }
            catch (ApplicationException)
            {
                return null;
            }
        }

        private Func<bool> GetNextStateFunc()
        {
            Func<bool> stateFunc = null;
            bool noCurrentOperator = null == this._currentOperator;
            bool noLeftArgument = null == this._leftArgument;
            bool operatorStackEmpty = this._operatorStack.Count <= 0;
            bool ambiguousStackEmpty = this._ambiguousStack.Count <= 0;
            int funcFlag = (noCurrentOperator ? 1 << 2 : 0) | (noLeftArgument ? 1 << 1 : 0) |
                           (operatorStackEmpty ? 1 << 0 : 0);
            if (this._elementIndex >= this._splitExpression.Length)
            {
                switch (funcFlag)
                {
                    case 0:
                        stateFunc = State_1_a_Action;
                        break;
                    case 1:
                        stateFunc = State_1_c_Action;
                        break;
                    case 2:
                        stateFunc = State_1_e_Action;
                        break;
                    case 3:
                        stateFunc = State_1_g_Action;
                        break;
                    case 4:
                        stateFunc = State_1_i_Action;
                        break;
                    case 5:
                        stateFunc = State_1_k_Action;
                        break;
                    case 6:
                        stateFunc = State_1_m_Action;
                        break;
                    case 7:
                        stateFunc = State_1_o_Action;
                        break;
                }
            }
            else if (this._splitExpression[this._elementIndex].StartsWith(Constants.ArgNamePrefix))
            {
                switch (funcFlag)
                {
                    case 0:
                        stateFunc = State_2_a_Action;
                        break;
                    case 1:
                        stateFunc = State_2_c_Action;
                        break;
                    case 2:
                        stateFunc = State_2_e_Action;
                        break;
                    case 3:
                        stateFunc = State_2_g_Action;
                        break;
                    case 4:
                        stateFunc = State_2_i_Action;
                        break;
                    case 5:
                        stateFunc = State_2_k_Action;
                        break;
                    case 6:
                        stateFunc = State_2_m_Action;
                        break;
                    case 7:
                        stateFunc = State_2_o_Action;
                        break;
                }
            }
            else
            {
                switch (funcFlag)
                {
                    case 0:
                        stateFunc = State_3_a_Action;
                        break;
                    case 1:
                        stateFunc = State_3_c_Action;
                        break;
                    case 2:
                        stateFunc = State_3_e_Action;
                        break;
                    case 3:
                        stateFunc = State_3_g_Action;
                        break;
                    case 4:
                        stateFunc = State_3_i_Action;
                        break;
                    case 5:
                        stateFunc = State_3_k_Action;
                        break;
                    case 6:
                        stateFunc = State_3_m_Action;
                        break;
                    case 7:
                        stateFunc = State_3_o_Action;
                        break;
                }
            }
            return stateFunc;
        }

        #region 元素为参数时的状态函数

        private bool State_1_a_Action()
        {
            this._leftArgument = this._splitExpression[this._elementIndex];
            return true;
        }

        private bool State_1_c_Action()
        {
            this._currentOperator = this._operatorStack.Pop();
            Func<bool> nextStateFunc = GetNextStateFunc();
            return nextStateFunc();
        }

        private bool State_1_e_Action()
        {
            PopAmbiguousPoint();
            return true;
        }

        private bool State_1_g_Action()
        {
            PopAmbiguousPoint();
            return true;
        }

        private bool State_1_i_Action()
        {
            if (this._currentOperator.IsNeedRightElement())
            {
                this._leftArgument = this._splitExpression[this._elementIndex];
            }
            else
            {
                PopAmbiguousPoint();
            }
            return true;
        }

        private bool State_1_k_Action()
        {
            if (this._currentOperator.IsNeedRightElement())
            {
                this._leftArgument = this._splitExpression[this._elementIndex];
            }
            else
            {
                PopAmbiguousPoint();
            }
            return true;
        }

        private bool State_1_m_Action()
        {
            PopAmbiguousPoint();
            return true;
        }

        private bool State_1_o_Action()
        {
            PopAmbiguousPoint();
            return true;
        }

        #endregion

        #region 元素为操作符时的状态函数

        private bool State_2_a_Action()
        {
            List<OperatorTokenInfo> tokenInfos = GetAvailableToken(false);
            if (tokenInfos.Count == 1)
            {
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                this._leftArgument = null;
            }
            else if (tokenInfos.Count > 1)
            {
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                tokenInfos.RemoveAt(0);
                PushAmbiguousPoint(tokenInfos);
            }
            else
            {
                PopAmbiguousPoint();
            }
            return true;
        }

        private bool State_2_c_Action()
        {
            this._currentOperator = this._operatorStack.Pop();
            Func<bool> nextStateFunc = GetNextStateFunc();
            return nextStateFunc();
        }

        private bool State_2_e_Action()
        {
            List<OperatorTokenInfo> tokenInfos = GetAvailableToken(true);
            if (tokenInfos.Count == 1)
            {
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                this._currentOperator.AddArgument(this._leftArgument);
                this._leftArgument = null;
            }
            else if (tokenInfos.Count > 1)
            {
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                this._currentOperator.AddArgument(this._leftArgument);
                this._leftArgument = null;
                tokenInfos.RemoveAt(0);
                PushAmbiguousPoint(tokenInfos);
            }
            else
            {
                PopAmbiguousPoint();
            }
            CurrentOperatorOverCheck();
            return true;
        }

        private bool State_2_g_Action()
        {
            this._currentOperator = this._operatorStack.Pop();
            Func<bool> nextStateFunc = GetNextStateFunc();
            return nextStateFunc();
        }

        private bool State_2_i_Action()
        {
            List<OperatorTokenInfo> tokenInfos = GetAvailableToken(false);
            if (tokenInfos.Count == 1)
            {
                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                this._leftArgument = null;
            }
            else if (tokenInfos.Count > 1)
            {
                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                this._leftArgument = null;
                tokenInfos.RemoveAt(0);
                PushAmbiguousPoint(tokenInfos);
            }
            else
            {
                PopAmbiguousPoint();
            }
            return true;
        }

        private bool State_2_k_Action()
        {
            List<OperatorTokenInfo> tokenInfos = GetAvailableToken(false);
            if (tokenInfos.Count == 1)
            {
                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                this._leftArgument = null;
            }
            else if (tokenInfos.Count > 1)
            {
                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(tokenInfos[0], this._elementIndex);
                this._leftArgument = null;
                tokenInfos.RemoveAt(0);
                PushAmbiguousPoint(tokenInfos);
            }
            else
            {
                PopAmbiguousPoint();
            }
            return true;
        }

        private bool State_2_m_Action()
        {
            string token = this._splitExpression[this._elementIndex];
            // 向右匹配获取的操作符集合
            List<OperatorTokenInfo> rightTokenInfos = GetAvailableToken(true);
            bool isCurrentTokenFit = this._currentOperator.IsCurrentTokenFit(token);
            bool oneElementLeft = this._currentOperator.ElementCountToFill() == 1;
            bool onlyOneElementNeed = this._currentOperator.IsNeedRightElement() &&
                                      this._currentOperator.IsTokenIterationOver();
            if (onlyOneElementNeed && rightTokenInfos.Count > 0)
            {
                OperatorTokenInfo availableRightTokenInfo = rightTokenInfos[0];
                if (rightTokenInfos.Count > 1)
                {
                    rightTokenInfos.RemoveAt(0);
                    PushAmbiguousPoint(rightTokenInfos);
                }
                if (this._currentOperator.Priority >= availableRightTokenInfo.Priority)
                {
                    this._currentOperator.AddArgument(this._leftArgument);
                    IExpressionData expression = this._currentOperator.CreateExpression();
                    UpdateNameAndAddToCache(expression);
                    this._currentOperator = new OperatorInstance(availableRightTokenInfo, this._elementIndex);
                    this._currentOperator.AddArgument(expression.Name);
                    this._leftArgument = null;
                    CurrentOperatorOverCheck();
                }
                else
                {
                    this._operatorStack.Push(this._currentOperator);
                    this._currentOperator = new OperatorInstance(availableRightTokenInfo, this._elementIndex);
                    this._currentOperator.AddArgument(this._leftArgument);
                    this._leftArgument = null;
                    CurrentOperatorOverCheck();
                }
            }
            else if (onlyOneElementNeed && rightTokenInfos.Count == 0)
            {
                PopAmbiguousPoint();
            }
            else if (isCurrentTokenFit)
            {
                this._currentOperator.AddArgument(this._leftArgument);
                this._currentOperator.MoveToNextToken();
                this._leftArgument = null;
                CurrentOperatorOverCheck();
            }
            else if (rightTokenInfos.Count == 0)
            {
                PopAmbiguousPoint();
            }
            else if (rightTokenInfos.Count == 1)
            {
                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(rightTokenInfos[0], this._elementIndex);
                this._currentOperator.AddArgument(this._leftArgument);
                CurrentOperatorOverCheck();
            }
            else if (rightTokenInfos.Count > 1)
            {
                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(rightTokenInfos[0], this._elementIndex);
                this._currentOperator.AddArgument(this._leftArgument);
                rightTokenInfos.RemoveAt(0);
                PushAmbiguousPoint(rightTokenInfos);
                CurrentOperatorOverCheck();
            }
        }

        private bool State_2_o_Action()
        {

        }

        #endregion

        #region 遍历结束后的状态函数

        private bool State_3_a_Action()
        {

        }

        private bool State_3_c_Action()
        {

        }

        private bool State_3_e_Action()
        {

        }

        private bool State_3_g_Action()
        {

        }

        private bool State_3_i_Action()
        {

        }

        private bool State_3_k_Action()
        {

        }

        private bool State_3_m_Action()
        {

        }

        private bool State_3_o_Action()
        {

        }

        private void CurrentOperatorOverCheck()
        {
            if (!this._currentOperator.IsOver()) return;
            IExpressionData expressionData = this._currentOperator.CreateExpression();
            UpdateNameAndAddToCache(expressionData);
            this._leftArgument = expressionData.Name;
            this._currentOperator = null;
            if (this._operatorStack.Count > 0)
            {
                this._currentOperator = this._operatorStack.Pop();
            }
        }

        /// <summary>
        /// 更新表达式名称并将表达式写入缓存
        /// </summary>
        private void UpdateNameAndAddToCache(IExpressionData expression)
        {
            expression.Name = string.Format(Constants.ExpPlaceHolderFormat, this._expressionIndex++.ToString());
            this._expressionCache.Add(expression.Name, expression);
        }

        #endregion

        private List<OperatorTokenInfo> GetAvailableToken(bool hasLeftElement)
        {
            this._fetchTokenInfoCache.Clear();
            if (null != this._lastAmbiguousPointToken)
            {
                this._fetchTokenInfoCache.Add(this._lastAmbiguousPointToken);
                this._lastAmbiguousPointToken = null;
                return _fetchTokenInfoCache;
            }
            string token = this._splitExpression[this._elementIndex];
            this._fetchTokenInfoCache.Clear();
            foreach (OperatorTokenInfo tokenInfo in this._operators)
            {
                if (tokenInfo.TokenGroup[0].Equals(token) && tokenInfo.HasLeftElement == hasLeftElement)
                {
                    this._fetchTokenInfoCache.Add(tokenInfo);
                }
            }
            return this._fetchTokenInfoCache;
        }
    }
}