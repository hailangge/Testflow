namespace Testflow.Data.Attributes
{
    public interface IAttributeArgument
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        string ArgumentName { get; set; }

        /// <summary>
        /// 参数索引
        /// </summary>
        int ArgumentIndex { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        AttributeArgumentType Type { get; set; }

        /// <summary>
        /// 参数的额外信息
        /// </summary>
        string ExtraInfo { get; set; }
    }
}