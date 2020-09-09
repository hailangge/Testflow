using System;
using System.Collections.Generic;
using Testflow.Data.Sequence;

namespace Testflow.Data.Actions
{
    /// <summary>
    /// 参数数据源获取方法名，该方法必须符合IList&lt;string&gt; AcquirerMethod(ISequenceStep, IStepAction, int)的接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ParameterSourceAttribute : Attribute
    {
        /// <summary>
        /// 参数值可选项的获取方法名，该方法必须符合IList&lt;string&gt; AcquirerMethod(ISequenceStep, IStepAction, int)的接口
        /// </summary>
        public string AcquirerMethod { get; set; }

        public ParameterSourceAttribute(string paramSourceAcquirer)
        {
            AcquirerMethod = paramSourceAcquirer;
        }
    }
}