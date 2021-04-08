using System;
using System.Collections.Generic;
using Testflow.Data.Sequence;
using Testflow.Usr;

namespace Testflow.Data.Attributes
{
    /// <summary>
    /// Step属性
    /// </summary>
    public interface IStepAttribute : ICloneableClass<IStepAttribute>, ISequenceElement
    {
        /// <summary>
        /// StepAttribute的名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 属性的索引号
        /// </summary>
        int Index { get; set; }

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
        /// 属性的生效条件
        /// </summary>
        string Condition { get; set; }

        /// <summary>
        /// 属性参数
        /// </summary>
        IList<string> ParameterValues { get; set; }

        /// <summary>
        /// StepAttribute的值
        /// </summary>
        [Obsolete]
        string Value { get; set; }
    }
}