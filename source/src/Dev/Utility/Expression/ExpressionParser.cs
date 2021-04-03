using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Testflow.Data.Expression;
using Testflow.Data.Sequence;
using Testflow.Modules;
using Testflow.Usr;
using Testflow.Utility.I18nUtil;
using Testflow.Utility.Utils;

namespace Testflow.Utility.Expression
{
    /// <summary>
    /// 表达式解析类
    /// </summary>
    public class ExpressionParser
    {
        private const int CacheCapacity = 500;

        // 表达式解析缓存
        private readonly StringBuilder _expressionCache;
        // 参数缓存
        private readonly Dictionary<string, string> _argumentCache;

        // 解析表达式的状态机
        private readonly ParserStateMachine _parserStateMachine;

        // 遍历时每个符号组在简单分割缓存中的索引值
        private readonly List<int> _tokenGroupIndexes;
        // 每次循环分割时上一个被写入fullSplitCache的索引值
        private readonly Stack<int> _lastEnqueueTokenIndexes;
        // 每个TokenGroup上次被使用的分割方式
        private readonly Stack<int> _lastUsedPossibleSplit;
        // 每个符号组可能的分割方式
        private readonly List<string[][]> _possibleSplitTokens;
        // 用参数分割后的表达式缓存
        private readonly List<string> _simpleSplitExpression;
        // 预设的拆分组合
        private readonly List<int[][]> _presetSplitArranges;

        // 包含运算符中用到的所有符号的集合
        private readonly HashSet<string> _operatorTokens;

        // 包含运算符中用到的所有符号字符的集合
        private readonly HashSet<char> _operatorTokenChars;

        // 操作符名称到操作符符号信息的映射
        private readonly Dictionary<string, OperatorTokenInfo> _operatorTokenMapping;
        private List<OperatorTokenInfo> _operatorTokenInfos;

        // 数值类型参数的匹配模式
        private readonly Regex _digitRegex;
        // 科学技术类型参数的匹配模式
        private readonly Regex _sciDigitRegex;
        // 字符串类型的匹配模式
        private readonly Regex _strRegex;
        // 布尔类型的匹配模式
        private readonly Regex _boolRegex;
        // 分割表达式以获取所有参与计算元素的匹配模式
        private readonly Regex _elementSplitRegex;
        // 表达式参数处理后的匹配模式
        private readonly Regex _singleArgRegex;
        // 表达式解析预处理过程中使用到的参数名称模式
        private readonly Regex _argNamePattern;
        // 表达式符号中连续符号的最长个数
        private int _maxTokenLength;

        /// <summary>
        /// 使用操作符信息初始化解析器
        /// </summary>
        /// <param name="operatorInfos">操作符描述信息</param>
        public ExpressionParser(Dictionary<string, ExpressionOperatorInfo> operatorInfos)
        {
            this._expressionCache = new StringBuilder(CacheCapacity);
            this._tokenGroupIndexes = new List<int>(100);
            this._lastEnqueueTokenIndexes = new Stack<int>(20);
            this._lastUsedPossibleSplit = new Stack<int>(20);
            this._possibleSplitTokens = new List<string[][]>(50);
            this._simpleSplitExpression = new List<string>(50);

            this._argumentCache = new Dictionary<string, string>(10);
            this._digitRegex = new Regex(Constants.NumericPattern, RegexOptions.Compiled);
            this._sciDigitRegex = new Regex(Constants.SciNumericPattern, RegexOptions.Compiled | RegexOptions.RightToLeft);
            this._strRegex = new Regex(Constants.StringPattern, RegexOptions.Compiled);
            this._boolRegex = new Regex(Constants.BoolPattern, RegexOptions.Compiled);
            this._argNamePattern = new Regex(Constants.ArgNamePattern, RegexOptions.Compiled);
            this._singleArgRegex = new Regex(Constants.SingleArgPattern, RegexOptions.Compiled);

            const int presetLevel = 7;
            this._presetSplitArranges = new List<int[][]>(presetLevel);
            InitPresetSplitArrangesToSpecifiedLevel(presetLevel);

            string elementSplitPattern = GetElementSplitPattern();
            this._elementSplitRegex = new Regex(elementSplitPattern, RegexOptions.RightToLeft | RegexOptions.Compiled);

            // 创建各个Operator的符号匹配器
            this._operatorTokenMapping = InitOperatorTokenInfoMapping(operatorInfos);

            this._operatorTokens = new HashSet<string>();
            this._operatorTokenChars = new HashSet<char>();
            this._maxTokenLength = 0;
            foreach (OperatorTokenInfo operatorTokenInfo in this._operatorTokenMapping.Values)
            {
                foreach (string token in operatorTokenInfo.TokenGroup)
                {
                    this._operatorTokens.Add(token);
                    foreach (char tokenChar in token)
                    {
                        this._operatorTokenChars.Add(tokenChar);
                    }
                    if (token.Length > this._maxTokenLength)
                    {
                        this._maxTokenLength = token.Length;
                    }
                }
            }

            this._parserStateMachine = new ParserStateMachine(this._operatorTokenInfos);
        }

        private void InitPresetSplitArrangesToSpecifiedLevel(int level)
        {
            this._presetSplitArranges.Add(new[] {new[] {1}});
            this._presetSplitArranges.Add(new[] {new[] {2}, new[] {1, 1}});
            this._presetSplitArranges.Add(new[] {new[] {3}, new[] {2, 1}, new[] {1, 2}, new[] {1, 1, 1}});
            this._presetSplitArranges.Add(new[]
            {
                new[] {4}, new[] {3, 1}, new[] {2, 2}, new[] {1, 3}, new[] {2, 1, 1},
                new[] {1, 2, 1}, new[] {1, 1, 2}, new[] {1, 1, 1, 1}
            });
            List<int[]> splitArrangeLists = new List<int[]>((int) Math.Pow(2, level) + 1);
            int[][] lastLevelSplitArrange = this._presetSplitArranges[this._presetSplitArranges.Count - 1];
            for (int i = this._presetSplitArranges.Count; i < level; i++)
            {
                SetNextLevelSplitArrange(i, lastLevelSplitArrange, splitArrangeLists);
                this._presetSplitArranges.Add(splitArrangeLists.ToArray());
                splitArrangeLists.Clear();
            }
        }

        private void SetNextLevelSplitArrange(int level, int[][] lastLevelSplitArrange, List<int[]> splitArrangeCache)
        {
            List<int> singleArrangeCache = new List<int>(level);
            int splitCount = 0;
            int startIndex = 0;
            foreach (int[] lastSplitArrange in lastLevelSplitArrange)
            {
                if (splitCount != lastSplitArrange.Length)
                {
                    startIndex = splitArrangeCache.Count;
                    splitCount = lastSplitArrange.Length;
                }
                for (int i = 0; i < lastSplitArrange.Length; i++)
                {
                    singleArrangeCache.AddRange(lastSplitArrange);
                    singleArrangeCache[i] += 1;
                    if (!IsArrayExist(splitArrangeCache, singleArrangeCache, startIndex))
                    {
                        splitArrangeCache.Add(singleArrangeCache.ToArray());
                    }
                    singleArrangeCache.Clear();
                }

                for (int i = 0; i < level; i++)
                {
                    singleArrangeCache.Add(1);
                }
                splitArrangeCache.Add(singleArrangeCache.ToArray());
            }
        }

        private bool IsArrayExist(List<int[]> splitArrangeCache, List<int> array, int startIndex)
        {
            for (int i = startIndex; i < splitArrangeCache.Count; i++)
            {
                int[] compareArray = splitArrangeCache[i];
                if (compareArray.Length != array.Count)
                {
                    continue;
                }
                for (int j = 0; j < array.Count; j++)
                {
                    if (compareArray[j] == array[j])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Dictionary<string, OperatorTokenInfo> InitOperatorTokenInfoMapping(Dictionary<string, ExpressionOperatorInfo> operatorInfos)
        {
            Dictionary < string, OperatorTokenInfo > operatorTokenMapping=  new Dictionary<string, OperatorTokenInfo>(operatorInfos.Count);
            this._operatorTokenInfos = new List<OperatorTokenInfo>(operatorInfos.Count);
            foreach (KeyValuePair<string, ExpressionOperatorInfo> operatorInfoPair in operatorInfos)
            {
                OperatorTokenInfo operatorAdapter = new OperatorTokenInfo(operatorInfoPair.Value);
                this._operatorTokenInfos.Add(operatorAdapter);
            }

            // 按照优先级，从大到小排序
            this._operatorTokenInfos.Sort(new OperatorTokenInfoComparer());
            int index = 0;
            foreach (OperatorTokenInfo operatorTokenInfo in this._operatorTokenInfos)
            {
                operatorTokenMapping.Add(operatorTokenInfo.OperatorName, operatorTokenInfo);
                operatorTokenInfo.Index = index++;
            }
            return operatorTokenMapping;
        }

        private string GetElementSplitPattern()
        {
            // 正则表达式中的元符号
            HashSet<char> metaCharacters = new HashSet<char>
            {
                '^', '[', '.', '$', '{', '*', '(', '\\', '+', ')','|', '?', '<', '>'
            };
            HashSet<string> tokenSets = new HashSet<string>();
            StringBuilder splitPattern = new StringBuilder(200);
            splitPattern.Append('(');
            foreach (OperatorTokenInfo operatorInfo in this._operatorTokenMapping.Values)
            {
                foreach (string token in operatorInfo.TokenGroup)
                {
                    if (tokenSets.Contains(token))
                    {
                        continue;
                    }
                    tokenSets.Add(token);
                    foreach (char tokenChar in token)
                    {
                        if (metaCharacters.Contains(tokenChar))
                        {
                            splitPattern.Append('\\').Append(tokenChar);
                        }
                        else
                        {
                            splitPattern.Append(tokenChar);
                        }
                    }
                    splitPattern.Append('|');
                }
            }
            if (splitPattern[splitPattern.Length - 1] == '|')
            {
                splitPattern.Remove(splitPattern.Length - 1, 1);
            }
            splitPattern.Append(')');
            return splitPattern.ToString();
        }

        #region 表达式解析

        /// <summary>
        /// 解析表达式并校验变量
        /// </summary>
        public IExpressionData ParseExpression(string expression, ISequenceStep step)
        {
            ISequence parent = SequenceUtils.GetParentSequence(step);
            return ParseExpression(expression, parent);
        }

        /// <summary>
        /// 解析表达式并校验变量
        /// </summary>
        public IExpressionData ParseExpression(string expression, ISequence parent)
        {
            ResetExpressionCache();
            try
            {
                // 参数别名到参数值的映射
                this._argumentCache.Clear();
                this._expressionCache.Clear();
                this._expressionCache.Append(expression);
                // 预处理，删除冗余的空格，替换参数为固定模式的字符串
                ParsingPreProcess(this._expressionCache, this._argumentCache);
                // 分割表达式元素
                IExpressionData expressionData = ParseExpressionData(this._expressionCache);
                ParsingPostProcess(expressionData, parent, this._argumentCache);
                ResetExpressionCache();
                return expressionData;
            }
            finally
            {
                ResetExpressionCache();
            }
        }

        /// <summary>
        /// 解析表达式
        /// </summary>
        public IExpressionData ParseExpression(string expression)
        {
            ResetExpressionCache();
            try
            {
                this._expressionCache.Append(expression);
                // 预处理，删除冗余的空格，替换参数为固定模式的字符串
                ParsingPreProcess(this._expressionCache, this._argumentCache);
                // 分割表达式元素
                IExpressionData expressionData = ParseExpressionData(this._expressionCache);
                ParsingPostProcess(expressionData, null, this._argumentCache);
                return expressionData;
            }
            finally
            {
                ResetExpressionCache();
            }
        }

        #region 表达式预处理

        private void ParsingPreProcess(StringBuilder expressionCache, Dictionary<string, string> argumentCache)
        {
            int argumentIndex = 0;
            CacheStringAndRemoveWhiteSpace(expressionCache, ref argumentIndex, argumentCache);
            CacheScientificNumericValue(expressionCache, ref argumentIndex, argumentCache);
            CacheExistingElements(expressionCache, ref argumentIndex, argumentCache);
        }

        private void CacheStringAndRemoveWhiteSpace(StringBuilder expressionCache, ref int argumentIndex, Dictionary<string, string> argumentCache)
        {
            const char quoteChar1 = '"';
            const char quoteChar2 = '\'';
            char lastQuoteChar = '\0';
            int lastQuoteIndex = -1;
            for (int i = expressionCache.Length - 1; i >= 0; i--)
            {
                char character = expressionCache[i];
                // 如果当前未包含在引号中，且当前字符为引号，则标记当前为字符串起始位置
                if (lastQuoteIndex == -1 && (character == quoteChar1 || character == quoteChar2))
                {
                    lastQuoteIndex = i;
                    lastQuoteChar = character;
                }
                else if (lastQuoteIndex > -1 && character == lastQuoteChar)
                {
                    CacheArgumentValue(expressionCache, ref argumentIndex, i, lastQuoteIndex, argumentCache);
                }
                else if (character == ' ')
                {
                    // 移除非字符串中的空格
                    expressionCache.Remove(i, 1);
                }
            }
        }

        // 科学计数中包含+/-等操作符符号，需要提前处理
        private void CacheScientificNumericValue(StringBuilder expressionCache, ref int argumentIndex, Dictionary<string, string> argumentCache)
        {
            MatchCollection scientificMatches = this._sciDigitRegex.Matches(expressionCache.ToString());
            foreach (Match match in scientificMatches)
            {
                int endIndex = match.Index + match.Length - 1;
                CacheArgumentValue(expressionCache, ref argumentIndex, match.Index, endIndex, argumentCache);
            }
        }

        private void CacheExistingElements(StringBuilder expressionCache, ref int argumentIndex, Dictionary<string, string> argumentCache)
        {
            string[] splitElements = this._elementSplitRegex.Split(expressionCache.ToString());
            int elementStartIndex = expressionCache.Length;
            foreach (string element in splitElements)
            {
                elementStartIndex -= element?.Length ?? 0;
                if (string.IsNullOrWhiteSpace(element) || argumentCache.ContainsKey(element))
                {
                    continue;
                }
                // 如果解析的元素不是已缓存项，但是却包含已缓存模式的值，说明表达式中出现了两个未经表达式分隔符分开的元素，表达式输入非法
                Match containArgMatch = this._argNamePattern.Match(element);
                if (containArgMatch.Success)
                {
                    I18N i18N = I18N.GetInstance(Constants.ExpI18nName);
                    throw new TestflowDataException(ModuleErrorCode.ExpressionError,
                        i18N.GetFStr("IllegalExpression", expressionCache.ToString()));
                }
                GetElementStartIndex(expressionCache, element, ref elementStartIndex);
                int argEndIndex = elementStartIndex + element.Length - 1;
                CacheArgumentValue(expressionCache, ref argumentIndex, elementStartIndex, argEndIndex, argumentCache);
            }
        }

        private static void GetElementStartIndex(StringBuilder expressionCache, string element, ref int elementStartIndex)
        {
            elementStartIndex -= element.Length;
            while (true)
            {
                while (element[0] != expressionCache[elementStartIndex])
                {
                    elementStartIndex--;
                }
                int elementIndex = elementStartIndex + 1;
                bool isEquals = true;
                for (int i = 1; i < element.Length; i++)
                {
                    if (expressionCache[elementIndex++] != element[i])
                    {
                        isEquals = false;
                        break;
                    }
                }
                if (isEquals)
                {
                    return;
                }
                elementStartIndex--;
            }
        }

        private void CacheArgumentValue(StringBuilder expressionCache, ref int argIndex, int argStartIndex,
            int argEndIndex, Dictionary<string, string> argumentCache)
        {
            string argName = string.Format(Constants.ArgNameFormat, argIndex++);
            string argumentValue = GetTrimmedArgValue(expressionCache, argStartIndex, argEndIndex);
            // 获取需要移除的长度，包括引号
            argumentCache.Add(argName, argumentValue);
            // 替换原来字符串位置的值为：StrX
            int argValueLength = argEndIndex - argStartIndex + 1;
            expressionCache.Remove(argStartIndex, argValueLength);
            expressionCache.Insert(argStartIndex, argName);
        }

        private string GetTrimmedArgValue(StringBuilder expressionCache, int argStartIndex, int argEndIndex)
        {
            while (this._expressionCache[argStartIndex] == ' ')
            {
                argStartIndex++;
            }
            while (this._expressionCache[argEndIndex] == ' ')
            {
                argEndIndex--;
            }
            int argValueLength = argEndIndex - argStartIndex + 1;
            if (argValueLength <= 0)
            {
                I18N i18N = I18N.GetInstance(Constants.ExpI18nName);
                throw new TestflowDataException(ModuleErrorCode.ExpressionError,
                    i18N.GetFStr("IllegalExpression", expressionCache.ToString()));
            }
            return expressionCache.ToString(argStartIndex, argValueLength);
        }

        #endregion


        #region 表达式解析

        private IExpressionData ParseExpressionData(StringBuilder expressionCache)
        {
            // 使用参数先对表达式进行简单分割，并缓存每个符号组在该列表中的索引号
            InitSimpleSplitExpressionAndTokenGroupIndexes(expressionCache);
            InitPossibleTokenGroupSplit();
            List<string> fullySplitExpression = new List<string>(50);
            for (int i = 0; i < this._tokenGroupIndexes.Count; i++)
            {
                this._lastUsedPossibleSplit.Push(-1);
            }
            // 初始化表达式分割缓存
            InitFullySplitExpression(fullySplitExpression);
            while (this._lastEnqueueTokenIndexes.Count > 0)
            {
                IExpressionData expression = this._parserStateMachine.ParseExpression(fullySplitExpression.ToArray());
                if (null != expression)
                {
                    return expression;
                }
                // 每次循环根据一种可用分配的模式执行分配
                UpdateFullySplitExpression(fullySplitExpression);
            }
            // 遍历到最后也未能找到有效的解析，则抛出异常
            I18N i18N = I18N.GetInstance(Constants.ExpI18nName);
            throw new TestflowDataException(ModuleErrorCode.ExpressionError,
                i18N.GetFStr("IllegalExpression", expressionCache.ToString()));
        }



        private void InitFullySplitExpression(List<string> fullySplitExpression)
        {
            // 当前符号组在_divergenceIndexes中的索引号
            int tokenGroupIndex = 0;
            int splitIndex = 0;
            while (splitIndex < this._simpleSplitExpression.Count)
            {
                string expressionElement = this._simpleSplitExpression[splitIndex];
                if (tokenGroupIndex < this._tokenGroupIndexes.Count && this._tokenGroupIndexes[tokenGroupIndex] != splitIndex)
                {
                    splitIndex++;
                    fullySplitExpression.Add(expressionElement);
                    continue;
                }
                fullySplitExpression.AddRange(this._possibleSplitTokens[tokenGroupIndex][0]);
                this._lastEnqueueTokenIndexes.Push(fullySplitExpression.Count);
                this._lastUsedPossibleSplit.Push(0);
                tokenGroupIndex++;
                splitIndex++;
            }
        }

        private void UpdateFullySplitExpression(List<string> fullySplitExpression)
        {
            bool hasUpdate = false;
            int tokenGroupIndex = this._lastEnqueueTokenIndexes.Count;
            // 找到最后一个可以更新的符号位置，更新为下一个可用值
            while (_lastEnqueueTokenIndexes.Count > 0 && !hasUpdate)
            {
                tokenGroupIndex--;
                int startIndex = this._lastEnqueueTokenIndexes.Pop();
                int lastUsedIndex = this._lastUsedPossibleSplit.Pop();
                string[][] possibleSplitToken = this._possibleSplitTokens[tokenGroupIndex];
                if (lastUsedIndex >= possibleSplitToken.Length - 1)
                {
                    continue;
                }
                lastUsedIndex++;
                fullySplitExpression.RemoveRange(startIndex, fullySplitExpression.Count - startIndex);
                fullySplitExpression.AddRange(possibleSplitToken[lastUsedIndex]);
                this._lastEnqueueTokenIndexes.Push(startIndex);
                this._lastUsedPossibleSplit.Push(lastUsedIndex);
                hasUpdate = true;
            }
            // 未找到可以继续更新的符号位置，抛出错误
            if (!hasUpdate)
            {
                I18N i18N = I18N.GetInstance(Constants.ExpI18nName);
                throw new TestflowDataException(ModuleErrorCode.ExpressionError,
                    i18N.GetFStr("IllegalExpression", this._expressionCache.ToString()));
            }
            // 从更新后的位置填充后续的所有元素，如果是符号组则从第0个可用的值继续遍历
            int splitIndex = this._tokenGroupIndexes[tokenGroupIndex++] + 1;
            while (splitIndex < this._simpleSplitExpression.Count)
            {
                string expressionElement = this._simpleSplitExpression[splitIndex];
                if (tokenGroupIndex < this._tokenGroupIndexes.Count && this._tokenGroupIndexes[tokenGroupIndex] != splitIndex)
                {
                    splitIndex++;
                    fullySplitExpression.Add(expressionElement);
                    continue;
                }
                this._lastEnqueueTokenIndexes.Push(fullySplitExpression.Count);
                this._lastUsedPossibleSplit.Push(0);
                fullySplitExpression.AddRange(this._possibleSplitTokens[tokenGroupIndex][0]);
                tokenGroupIndex++;
                splitIndex++;
            }
        }

        // 获取使用参数分割后的表达式数组
        private void InitSimpleSplitExpressionAndTokenGroupIndexes(StringBuilder expressionCache)
        {
            int index = 0;
            int lastIndex = -1;
            while (index < expressionCache.Length)
            {
                if (expressionCache[index].Equals('A'))
                {
                    if (lastIndex >= 0)
                    {
                        this._simpleSplitExpression.Add(expressionCache.ToString(lastIndex, index - lastIndex));
                    }

                    lastIndex = index;
                    index += Constants.ArgNamePrefix.Length + 1;
                    while (index < expressionCache.Length && expressionCache[index] >= '0' && expressionCache[index] <= '9')
                    {
                        index++;
                    }

                    this._simpleSplitExpression.Add(expressionCache.ToString(lastIndex, index - lastIndex));
                    this._tokenGroupIndexes.Add(this._simpleSplitExpression.Count - 1);
                    lastIndex = index;
                }
                else
                {
                    index++;
                }
            }
            if (lastIndex < expressionCache.Length)
            {
                this._simpleSplitExpression.Add(expressionCache.ToString(lastIndex, expressionCache.Length - lastIndex));
                this._tokenGroupIndexes.Add(this._simpleSplitExpression.Count - 1);
            }
        }

        private void InitPossibleTokenGroupSplit()
        {
            List<string[]> possibleSplits = new List<string[]>(10);
            foreach (int tokenGroupIndex in this._tokenGroupIndexes)
            {
                possibleSplits.Clear();
                InitSinglePossibleTokenSplit(this._simpleSplitExpression[tokenGroupIndex], possibleSplits);
                this._possibleSplitTokens.Add(possibleSplits.ToArray());
            }
        }

        private void InitSinglePossibleTokenSplit(string tokenGroup, List<string[]> possibleSplits)
        {
            List<string> singlePossibleSplits = new List<string>(10);
            if (tokenGroup.Length > this._presetSplitArranges.Count)
            {
                I18N i18N = I18N.GetInstance(Constants.ExpI18nName);
                throw new TestflowDataException(ModuleErrorCode.ExpressionError,
                    i18N.GetFStr("IllegalExpression", this._expressionCache.ToString()));
            }
            int[][] presetSplitArrange = this._presetSplitArranges[tokenGroup.Length - 1];
            foreach (int[] splitArrange in presetSplitArrange)
            {
                bool isPossibleSplit = FillSplitOperatorToken(tokenGroup, splitArrange, singlePossibleSplits);
                if (isPossibleSplit)
                {
                    possibleSplits.Add(singlePossibleSplits.ToArray());
                }
            }
            if (possibleSplits.Count == 0)
            {
                I18N i18N = I18N.GetInstance(Constants.ExpI18nName);
                throw new TestflowDataException(ModuleErrorCode.ExpressionError,
                    i18N.GetFStr("IllegalExpression", this._expressionCache.ToString()));
            }
        }

        private bool FillSplitOperatorToken(string tokens, int[] splitArrange, List<string> expressionSplitCache)
        {
            int startIndex = 0;
            int originalCacheLength = expressionSplitCache.Count;
            foreach (int splitLength in splitArrange)
            {
                string token = tokens.Substring(startIndex, splitLength);
                if (!this._operatorTokens.Contains(token))
                {
                    if (expressionSplitCache.Count > originalCacheLength)
                    {
                        expressionSplitCache.RemoveRange(originalCacheLength,
                            expressionSplitCache.Count - originalCacheLength);
                    }
                    return false;
                }
                expressionSplitCache.Add(token);
                startIndex += splitLength;
            }
            return true;
        }

        #endregion


        private void ParsingPostProcess(IExpressionData expressionData, ISequence parent,
            Dictionary<string, string> argumentCache)
        {
            ExpressionPostProcess(expressionData, argumentCache, parent);
        }

        #region 表达式后续处理

        private void ExpressionPostProcess(IExpressionData expressionData, Dictionary<string, string> argumentCache,
            ISequence parent)
        {
            ExpressionElementPostProcess(expressionData.Source, argumentCache, parent);
            foreach (IExpressionElement expressionElement in expressionData.Arguments)
            {
                ExpressionElementPostProcess(expressionElement, argumentCache, parent);
            }
        }

        private void ExpressionElementPostProcess(IExpressionElement expressionElement,
            Dictionary<string, string> argumentCache, ISequence parent)
        {
            if (expressionElement.Type == ParameterType.NotAvailable)
            {
                return;
            }
            if (expressionElement.Type == ParameterType.Expression)
            {
                ExpressionPostProcess(expressionElement.Expression, argumentCache, parent);
                return;
            }
            string value = argumentCache[expressionElement.Value];
            // 值是数值类型或布尔类型
            if (this._digitRegex.IsMatch(value) || this._boolRegex.IsMatch(value))
            {
                expressionElement.Value = value;
                expressionElement.Type = ParameterType.Value;
            }
            else if (this._strRegex.IsMatch(value))
            {
                // 字符串替换为去除双引号后的值
                Match matchData = this._strRegex.Match(value);
                expressionElement.Value = matchData.Groups[2].Value;
                expressionElement.Type = ParameterType.Value;
            }
            else
            {
                if (null != parent && !SequenceUtils.IsVariableExist(value, parent))
                {
                    I18N i18N = I18N.GetInstance(Constants.ExpI18nName);
                    throw new TestflowDataException(ModuleErrorCode.ExpressionError, i18N.GetFStr("ExpVariableNotExist", value));
                }
                // 否则则认为表达式为变量值
                expressionElement.Value = value;
                expressionElement.Type = ParameterType.Variable;
            }
            //
            //            // 如果双引号或者单引号不成对，则抛出异常
            //            if (inDoubleQuotation || inQuotation)
            //            {
            //                _logService.Print(LogLevel.Error, CommonConst.PlatformLogSession,
            //                    $"Illegal expression <{expressionCache}>");
            //                I18N i18N = I18N.GetInstance(Constants.I18nName);
            //                throw new TestflowDataException(ModuleErrorCode.ExpressionError,
            //                    i18N.GetFStr("IllegalExpression", expressionCache.ToString()));
            //            }
        }

        #endregion

        #endregion

        public bool RenameVariable(string expression, string varOldName, string varNewName)
        {
            if (!expression.Contains(varOldName))
            {
                return false;
            }
            int index = expression.Length;
            this._expressionCache.Clear();
            this._expressionCache.Append(expression);
            const char empty = '\0';
            int oldNameLength = varOldName.Length;
            bool varExist = false;
            while ((index = expression.LastIndexOf(varOldName, 0, index, StringComparison.Ordinal)) >= 0)
            {
                // 找出对应变量名左侧和右侧第一个非空格的字符
                char leftElement;
                int leftIndex = index - 1;
                do
                {
                    leftElement = leftIndex >= 0 ? this._expressionCache[leftIndex] : empty;
                    leftIndex--;
                } while (' ' == leftElement);
                char rightElement;
                int rightIndex = index + oldNameLength;
                do
                {
                    rightElement = rightIndex < this._expressionCache.Length ? this._expressionCache[rightIndex] : empty;
                    rightIndex++;
                } while (' ' == rightElement);
                // 如果左侧元素或右侧元素是计算分隔符或者空字符，则认为此处为变量
                // TODO 这里可能会出现字符串中有变量名且前后字符都是运算符的问题，该场景少见，后续优化
                if ((this._operatorTokenChars.Contains(leftElement) || leftElement == empty) &&
                    (this._operatorTokenChars.Contains(rightElement) || rightElement == empty))
                {
                    this._expressionCache.Replace(varOldName, varNewName, index, oldNameLength);
                    varExist = true;
                }
            }
            return varExist;
        }

        public bool IsExpression(string parameterValue)
        {
            return parameterValue.Any(valueChar => this._operatorTokenChars.Contains(valueChar));
        }

        private void ResetExpressionCache()
        {
            this._expressionCache.Clear();
            if (this._expressionCache.Capacity > CacheCapacity)
            {
                this._expressionCache.Capacity = CacheCapacity;
            }
            this._argumentCache.Clear();
            this._tokenGroupIndexes.Clear();
        }
    }
}