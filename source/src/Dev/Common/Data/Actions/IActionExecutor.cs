using System;
using System.Collections.Generic;
using Testflow.Data.Sequence;
using Testflow.DesignTime;
using Testflow.Runtime;

namespace Testflow.Data.Actions
{
    /// <summary>
    /// Action执行器接口
    /// </summary>
    public interface IActionExecutor
    {
        /// <summary>
        /// 初始化Action执行器
        /// </summary>
        /// <param name="context">设计时</param>
        void DesigntimeInitialize(IDesigntimeContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        void DesigntimeInitialize(ISlaveRunnerContext context);
    }
}