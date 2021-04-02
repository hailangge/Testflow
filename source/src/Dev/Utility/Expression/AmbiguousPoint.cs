using System.Collections.Generic;

namespace Testflow.Utility.Expression
{
    /// <summary>
    /// 表达式出现歧义的位置
    /// </summary>
    internal class AmbiguousPoint
    {
        /// <summary>
        /// 歧义点所在的索引号
        /// </summary>
        public int ElementIndex { get; set; }

        /// <summary>
        /// 操作符栈的深度
        /// </summary>
        public int OperatorStackLength { get; set; }

        /// <summary>
        /// 表达式缓存长度
        /// </summary>
        public int ExpressionCacheLength { get; set; }

        /// <summary>
        /// 当前待处理的参数
        /// </summary>
        public string LeftArgument { get; set; }

        /// <summary>
        /// 当前的操作符实例
        /// </summary>
        public OperatorInstance CurrentOperator { get; set; }

        /// <summary>
        /// 歧义点剩余可取的符号。运算符在指定位置如果出现歧义，则在该缓存中保存一个list用于保存仍未尝试过的运算符,key为歧义点的索引
        /// </summary>
        public List<OperatorTokenInfo> PossibleTokens { get; set; }

    }
}