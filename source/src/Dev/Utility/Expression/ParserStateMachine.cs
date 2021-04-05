using System;
using System.Collections.Generic;
using Testflow.Data.Expression;
using Testflow.Data.Sequence;

/*
 * 该状态机的维度划分有两个方面
 * 1. 当前元素为：参数、操作符、表达式遍历结束。状态分别用1,2,3标记
 * 2. 是否存在待处理参数值，当前是否存在待完成操作符，操作符堆栈中是否为空，歧义点堆栈是否为空进行状态划分。状态分别用a-p划分，详细的分配方式如下表所示：
 *      CO(CurrentOperator), LE(LeftArgument), OS(OperatorStack), AS(AmbiguousStack)。0代表NULL或者元素个数为0；1代表不为NULL或者元素个数大于0
 *          a:CO=0,LE=0,OS=0,AS=0  
 *          b:CO=0,LE=0,OS=0,AS=1
 *          c:CO=0,LE=0,OS=1,AS=0
 *          d:CO=0,LE=0,OS=1,AS=1
 *          e:CO=0,LE=1,OS=0,AS=0
 *          f:CO=0,LE=1,OS=0,AS=1
 *          g:CO=0,LE=1,OS=1,AS=0
 *          h:CO=0,LE=1,OS=1,AS=1
 *          i:CO=1,LE=0,OS=0,AS=0
 *          j:CO=1,LE=0,OS=0,AS=1
 *          k:CO=1,LE=0,OS=1,AS=0
 *          l:CO=1,LE=0,OS=1,AS=1
 *          m:CO=1,LE=1,OS=0,AS=0
 *          n:CO=1,LE=1,OS=0,AS=1
 *          o:CO=1,LE=1,OS=1,AS=0
 *          p:CO=1,LE=1,OS=1,AS=1
 *      因为AS=0和AS=1的区别在于解析失败后直接终止还是回退到上一个歧义点继续，所以在当前状态机中只区分了奇数索引的状态，将AS不同状态的处理在PopAmbiguous方法中实现
 * 状态的描述和状态转移操作参见文档《表达式解析状态迁移》
 */

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
        private readonly List<OperatorTokenInfo> _fetchTokenInfoCache;

        public ParserStateMachine(IList<OperatorTokenInfo> operators)
        {
            this._operators = operators;
            this._operatorStack = new Stack<OperatorInstance>(20);
            this._ambiguousStack = new Stack<AmbiguousPoint>(20);
            this._expressionCache = new Dictionary<string, IExpressionData>(50);
            this._fetchTokenInfoCache = new List<OperatorTokenInfo>(10);

            Reset();
        }

        private void Reset()
        {
            if (_elementIndex < 0)
            {
                return;
            }
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
            // 将当前元素回退到最近歧义点的上一个元素
            this._elementIndex = ambiguousPoint.ElementIndex - 1;
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

                if (string.IsNullOrWhiteSpace(_leftArgument))
                {
                    return null;
                }

                return GetResultExpression();
            }
            catch (ApplicationException)
            {
                return null;
            }
            finally
            {
                Reset();
            }
        }

        private IExpressionData GetResultExpression()
        {
            IExpressionData rootExpression = this._expressionCache[this._leftArgument];
            ProcessExpressionArguments(rootExpression);
            return rootExpression;
        }

        // 将表达式中用表达式占位符替代的表达式元素的值更新为真实的子表达式
        private void ProcessExpressionArguments(IExpressionData expression)
        {
            IExpressionElement source = expression.Source;
            if (source.Type == ParameterType.Expression)
            {
                if (!this._expressionCache.ContainsKey(source.Value))
                {
                    throw new ApplicationException("Source expression not set.");
                }
                source.Expression = this._expressionCache[source.Value];
                source.Value = string.Empty;
                this._expressionCache.Remove(source.Value);
                ProcessExpressionArguments(source.Expression);
            }
            foreach (IExpressionElement argument in expression.Arguments)
            {
                if (argument.Type != ParameterType.Expression)
                {
                    continue;
                }
                if (!this._expressionCache.ContainsKey(argument.Value))
                {
                    throw new ApplicationException("Argument expression not set.");
                }
                argument.Expression = this._expressionCache[argument.Value];
                argument.Value = string.Empty;
                this._expressionCache.Remove(argument.Value);
                ProcessExpressionArguments(argument.Expression);
            }
        }

        private Func<bool> GetNextStateFunc()
        {
            Func<bool> stateFunc = null;
            bool hasCurrentOperator = null != this._currentOperator;
            bool hasLeftArgument = null != this._leftArgument;
            bool operatorStackNotEmpty = this._operatorStack.Count > 0;
            bool ambiguousStackNotEmpty = this._ambiguousStack.Count > 0;
            int funcFlag = (hasCurrentOperator ? 1 << 3 : 0) | (hasLeftArgument ? 1 << 2 : 0) |
                           (operatorStackNotEmpty ? 1 << 1 : 0) | (ambiguousStackNotEmpty ? 1 : 0);
            bool elementIterationNotOver = this._elementIndex < this._splitExpression.Length;
            bool elementIsArgument = false;
            if (elementIterationNotOver)
            {
                elementIsArgument = this._splitExpression[this._elementIndex].StartsWith(UtilityConstants.ArgNamePrefix);
            }
            // 遍历到参数
            if (elementIterationNotOver && elementIsArgument)
            {
                switch (funcFlag)
                {
                    case 0:
                    case 1:
                        stateFunc = State_1_a_Action;
                        break;
                    case 2:
                    case 3:
                        stateFunc = State_1_c_Action;
                        break;
                    case 4:
                    case 5:
                        stateFunc = State_1_e_Action;
                        break;
                    case 6:
                    case 7:
                        stateFunc = State_1_g_Action;
                        break;
                    case 8:
                    case 9:
                        stateFunc = State_1_i_Action;
                        break;
                    case 10:
                    case 11:
                        stateFunc = State_1_k_Action;
                        break;
                    case 12:
                    case 13:
                        stateFunc = State_1_m_Action;
                        break;
                    case 14:
                    case 15:
                        stateFunc = State_1_o_Action;
                        break;
                }
            }
            // 遍历到运算符
            else if (elementIterationNotOver && !elementIsArgument)
            {
                switch (funcFlag)
                {
                    case 0:
                    case 1:
                        stateFunc = State_2_a_Action;
                        break;
                    case 2:
                    case 3:
                        stateFunc = State_2_c_Action;
                        break;
                    case 4:
                    case 5:
                        stateFunc = State_2_e_Action;
                        break;
                    case 6:
                    case 7:
                        stateFunc = State_2_g_Action;
                        break;
                    case 8:
                    case 9:
                        stateFunc = State_2_i_Action;
                        break;
                    case 10:
                    case 11:
                        stateFunc = State_2_k_Action;
                        break;
                    case 12:
                    case 13:
                        stateFunc = State_2_m_Action;
                        break;
                    case 14:
                    case 15:
                        stateFunc = State_2_o_Action;
                        break;
                }
            }
            // 遍历结束
            else
            {
                switch (funcFlag)
                {
                    case 0:
                    case 1:
                        stateFunc = State_3_a_Action;
                        break;
                    case 2:
                    case 3:
                        stateFunc = State_3_c_Action;
                        break;
                    case 4:
                    case 5:
                        stateFunc = State_3_e_Action;
                        break;
                    case 6:
                    case 7:
                        stateFunc = State_3_g_Action;
                        break;
                    case 8:
                    case 9:
                        stateFunc = State_3_i_Action;
                        break;
                    case 10:
                    case 11:
                        stateFunc = State_3_k_Action;
                        break;
                    case 12:
                    case 13:
                        stateFunc = State_3_m_Action;
                        break;
                    case 14:
                    case 15:
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
            return false;
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
            return false;
        }

        private bool State_1_g_Action()
        {
            PopAmbiguousPoint();
            return false;
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
            return false;
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
            return false;
        }

        private bool State_1_m_Action()
        {
            PopAmbiguousPoint();
            return false;
        }

        private bool State_1_o_Action()
        {
            PopAmbiguousPoint();
            return false;
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
            return false;
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
                OperatorTokenInfo firstTokenInfo = tokenInfos[0];
                tokenInfos.RemoveAt(0);
                PushAmbiguousPoint(tokenInfos);

                this._currentOperator = new OperatorInstance(firstTokenInfo, this._elementIndex);
                this._currentOperator.AddArgument(this._leftArgument);
                this._leftArgument = null;
            }
            else
            {
                PopAmbiguousPoint();
            }
            CurrentOperatorOverCheck();
            return false;
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
                OperatorTokenInfo firstTokenInfo = tokenInfos[0];
                tokenInfos.RemoveAt(0);
                PushAmbiguousPoint(tokenInfos);

                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(firstTokenInfo, this._elementIndex);
                this._leftArgument = null;
            }
            else
            {
                PopAmbiguousPoint();
            }
            return false;
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
                OperatorTokenInfo firstTokenInfo = tokenInfos[0];
                tokenInfos.RemoveAt(0);
                PushAmbiguousPoint(tokenInfos);

                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(firstTokenInfo, this._elementIndex);
                this._leftArgument = null;
            }
            else
            {
                PopAmbiguousPoint();
            }
            return false;
        }

        private bool State_2_m_Action()
        {
            string token = this._splitExpression[this._elementIndex];
            // 向右匹配获取的操作符集合
            List<OperatorTokenInfo> rightTokenInfos = GetAvailableToken(true);
            bool isCurrentTokenFit = this._currentOperator.IsCurrentTokenFit(token);
            bool oneElementLeft = this._currentOperator.ElementCountToFill() == 1;
            bool onlyOneArgumentNeed = this._currentOperator.IsNeedRightElement() &&
                                      this._currentOperator.IsTokenIterationOver();
            if (onlyOneArgumentNeed && rightTokenInfos.Count > 0)
            {
                OperatorTokenInfo firstRightTokenInfo = rightTokenInfos[0];
                if (rightTokenInfos.Count > 1)
                {
                    rightTokenInfos.RemoveAt(0);
                    PushAmbiguousPoint(rightTokenInfos);
                }
                if (this._currentOperator.Priority >= firstRightTokenInfo.Priority)
                {
                    this._currentOperator.AddArgument(this._leftArgument);
                    IExpressionData expression = this._currentOperator.CreateExpression();
                    UpdateNameAndAddToCache(expression);
                    this._currentOperator = new OperatorInstance(firstRightTokenInfo, this._elementIndex);
                    this._currentOperator.AddArgument(expression.Name);
                    this._leftArgument = null;
                    CurrentOperatorOverCheck();
                }
                else
                {
                    this._operatorStack.Push(this._currentOperator);
                    this._currentOperator = new OperatorInstance(firstRightTokenInfo, this._elementIndex);
                    this._currentOperator.AddArgument(this._leftArgument);
                    this._leftArgument = null;
                    CurrentOperatorOverCheck();
                }
            }
            else if (onlyOneArgumentNeed && rightTokenInfos.Count == 0)
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
                this._leftArgument = null;
                CurrentOperatorOverCheck();
            }
            else if (rightTokenInfos.Count > 1)
            {
                OperatorTokenInfo firstTokenInfo = rightTokenInfos[0];
                rightTokenInfos.RemoveAt(0);
                PushAmbiguousPoint(rightTokenInfos);

                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(firstTokenInfo, this._elementIndex);
                this._currentOperator.AddArgument(this._leftArgument);
                this._leftArgument = null;

                CurrentOperatorOverCheck();
            }
            return false;
        }

        private bool State_2_o_Action()
        {
            string token = this._splitExpression[this._elementIndex];
            // 向右匹配获取的操作符集合
            List<OperatorTokenInfo> rightTokenInfos = GetAvailableToken(true);
            bool isCurrentTokenFit = this._currentOperator.IsCurrentTokenFit(token);
            bool oneElementLeft = this._currentOperator.ElementCountToFill() == 1;
            bool onlyOneArgumentNeed = this._currentOperator.IsNeedRightElement() &&
                                      this._currentOperator.IsTokenIterationOver();
            if (onlyOneArgumentNeed)
            {
                if (rightTokenInfos.Count > 0)
                {
                    OperatorTokenInfo firstRightTokenInfo = rightTokenInfos[0];
                    if (rightTokenInfos.Count > 1)
                    {
                        rightTokenInfos.RemoveAt(0);
                        PushAmbiguousPoint(rightTokenInfos);
                    }
                    if (this._currentOperator.Priority >= firstRightTokenInfo.Priority)
                    {
                        this._currentOperator.AddArgument(this._leftArgument);
                        IExpressionData expression = this._currentOperator.CreateExpression();
                        UpdateNameAndAddToCache(expression);
                        this._currentOperator = new OperatorInstance(firstRightTokenInfo, this._elementIndex);
                        this._currentOperator.AddArgument(expression.Name);
                        this._leftArgument = null;
                        CurrentOperatorOverCheck();
                    }
                    else
                    {
                        this._operatorStack.Push(this._currentOperator);
                        this._currentOperator = new OperatorInstance(firstRightTokenInfo, this._elementIndex);
                        this._currentOperator.AddArgument(this._leftArgument);
                        this._leftArgument = null;
                        CurrentOperatorOverCheck();
                    }
                }
                else if (rightTokenInfos.Count == 0)
                {
                    OperatorInstance stackTopOperator = this._operatorStack.Peek();
                    bool topOperatorOneArgumentNeed = stackTopOperator.IsNeedRightElement() &&
                                                      stackTopOperator.IsTokenIterationOver();
                    bool topOperatorTokenFit = stackTopOperator.IsCurrentTokenFit(token) && stackTopOperator.IsNeedRightElement();
                    if (topOperatorTokenFit || topOperatorOneArgumentNeed)
                    {
                        this._currentOperator.AddArgument(this._leftArgument);
                        IExpressionData expressionData = this._currentOperator.CreateExpression();
                        UpdateNameAndAddToCache(expressionData);
                        this._leftArgument = expressionData.Name;
                        this._currentOperator = this._operatorStack.Pop();
                        Func<bool> nextStateFunc = GetNextStateFunc();
                        return nextStateFunc();
                    }
                    PopAmbiguousPoint();
                }
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
                this._leftArgument = null;
                CurrentOperatorOverCheck();
            }
            else if (rightTokenInfos.Count > 1)
            {
                OperatorTokenInfo firstTokenInfo = rightTokenInfos[0];
                rightTokenInfos.RemoveAt(0);
                PushAmbiguousPoint(rightTokenInfos);

                this._operatorStack.Push(this._currentOperator);
                this._currentOperator = new OperatorInstance(firstTokenInfo, this._elementIndex);
                this._currentOperator.AddArgument(this._leftArgument);
                this._leftArgument = null;
                CurrentOperatorOverCheck();
            }
            return false;
        }

        #endregion

        #region 遍历结束后的状态函数

        private bool State_3_a_Action()
        {
            this._leftArgument = null;
            return true;
        }

        private bool State_3_c_Action()
        {
            this._currentOperator = this._operatorStack.Pop();
            Func<bool> nextStateFunc = GetNextStateFunc();
            return nextStateFunc();
        }

        private bool State_3_e_Action()
        {
            // LeftElement就是最终的结果
            return true;
        }

        private bool State_3_g_Action()
        {
            this._currentOperator = this._operatorStack.Pop();
            Func<bool> nextStateFunc = GetNextStateFunc();
            return nextStateFunc();
        }

        private bool State_3_i_Action()
        {
            if (!this._currentOperator.IsOver())
            {
                PopAmbiguousPoint();
                return false;
            }
            IExpressionData expressionData = this._currentOperator.CreateExpression();
            UpdateNameAndAddToCache(expressionData);
            this._leftArgument = expressionData.Name;
            this._currentOperator = null;
            return true;
        }

        private bool State_3_k_Action()
        {
            if (!this._currentOperator.IsOver())
            {
                PopAmbiguousPoint();
                return false;
            }
            IExpressionData expressionData = this._currentOperator.CreateExpression();
            UpdateNameAndAddToCache(expressionData);
            this._leftArgument = expressionData.Name;
            this._currentOperator = this._operatorStack.Pop();
            Func<bool> nextStateFunc = GetNextStateFunc();
            return nextStateFunc();
        }

        private bool State_3_m_Action()
        {
            if (this._currentOperator.IsNeedRightElement())
            {
                this._currentOperator.AddArgument(this._leftArgument);
                this._leftArgument = null;
                if (this._currentOperator.IsOver())
                {
                    IExpressionData expressionData = this._currentOperator.CreateExpression();
                    UpdateNameAndAddToCache(expressionData);
                    this._leftArgument = expressionData.Name;
                    this._currentOperator = null;
                    return true;
                }
            }
            PopAmbiguousPoint();
            return false;
        }

        private bool State_3_o_Action()
        {
            if (this._currentOperator.IsNeedRightElement())
            {
                this._currentOperator.AddArgument(this._leftArgument);
                this._leftArgument = null;
                if (this._currentOperator.IsOver())
                {
                    IExpressionData expressionData = this._currentOperator.CreateExpression();
                    UpdateNameAndAddToCache(expressionData);
                    this._leftArgument = expressionData.Name;
                    this._currentOperator = this._operatorStack.Pop();

                    Func<bool> nextStateFunc = GetNextStateFunc();
                    return nextStateFunc();
                }
            }
            PopAmbiguousPoint();
            return false;
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
            expression.Name = string.Format(UtilityConstants.ExpPlaceHolderFormat, this._expressionIndex++.ToString());
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