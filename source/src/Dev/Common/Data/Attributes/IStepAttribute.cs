using Testflow.Data.Sequence;

namespace Testflow.Data.Attributes
{
    /// <summary>
    /// Step属性
    /// </summary>
    public interface IStepAttribute : ISequenceElement
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 属性的目标名称
        /// </summary>
        string Target { get; set; }

        /// <summary>
        /// 属性的类型名称
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// 完整类型
        /// </summary>
        string FullType { get; set; }

        /// <summary>
        /// 属性的触发条件
        /// </summary>
        string Condition { get; set; }

        /// <summary>
        /// 属性的值，可以包含变量、表达式等
        /// </summary>
        string Value { get; set; }
    }
}