namespace Testflow.Data.Attributes
{
    /// <summary>
    /// 属性参数的类型
    /// </summary>
    public enum AttributeArgumentType
    {
        /// <summary>
        /// 普通字符串
        /// </summary>
        String = 0,

        /// <summary>
        /// 带有表达式参数的字符串
        /// </summary>
        FunctionString = 1,

        /// <summary>
        /// 整型数值
        /// </summary>
        Integer = 2,

        /// <summary>
        /// 普通数值类型
        /// </summary>
        Numeric = 3,

        /// <summary>
        /// 条件类型
        /// </summary>
        Condition = 4,

        /// <summary>
        /// 枚举类型
        /// </summary>
        Enumeration = 5,

        /// <summary>
        /// 动态枚举类型
        /// </summary>
        DynamicEnumeration = 6
    }
}