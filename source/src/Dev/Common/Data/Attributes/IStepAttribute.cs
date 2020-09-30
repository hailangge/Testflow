﻿using System.Collections.Generic;
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
        /// 属性名称
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
        string FullType { get; }

        /// <summary>
        /// 属性参数
        /// </summary>
        IList<string> ParameterValues { get; set; }
    }
}